using MenuChanger.MenuElements;
using Modding;
using Newtonsoft.Json;
using RandomizerMod.Menu;
using RandomizerMod.Settings;
using RandoSettingsManager.SettingsManagement.Filer;
using RandoSettingsManager.SettingsManagement.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RandoSettingsManager.SettingsManagement
{
    internal record ProxyMetadata(ISerializableSettingsProxy Proxy, ISerializableVersioningPolicy VersioningPolicy);

    internal class SettingsManager
    {
        const string RANDO_JSON = "rando.json";
        const string VERSION_TXT = "version.txt";

        readonly Dictionary<string, ProxyMetadata> metadata = new();
        readonly ISerializableVersioningPolicy randoVersionPolicy = new StrictModVersioningPolicy((Mod)ModHooks.GetMod("Randomizer 4"));

        public List<string> LastSentMods { get; } = new();
        public List<string> LastReceivedMods { get; } = new();
        public List<string> LastModsReceivedWithoutSettings { get; } = new();

        public void Register(string key, ProxyMetadata md)
        {
            metadata[key] = md;
        }

        public void WriteRandoProfile(IDirectory targetDir, bool storeVersion, bool storeSeed, GenerationSettings gs)
        {
            if (!storeSeed)
            {
                gs.Seed = int.MinValue;
            }
            IFile rando = targetDir.CreateFile(RANDO_JSON);
            rando.WriteContent(JsonConvert.SerializeObject(gs));
            if (storeVersion)
            {
                IFile version = targetDir.CreateFile(VERSION_TXT);
                version.WriteContent(randoVersionPolicy.SerializedVersion);
            }
        }

        public void SaveSettings(IDirectory targetDir, bool storeVersion, bool storeSeed)
        {
            GenerationSettings gs = (GenerationSettings)RandomizerMod.RandomizerMod.GS.DefaultMenuSettings.Clone();
            WriteRandoProfile(targetDir, storeVersion, storeSeed, gs);

            LastSentMods.Clear();
            LastSentMods.Add("Randomizer 4");

            foreach (KeyValuePair<string, ProxyMetadata> proxyData in metadata)
            {
                string key = proxyData.Key;
                var (proxy, vp) = proxyData.Value;
                if (proxy.TryProvideSerializedSettings(out string? settings) && settings != null)
                {
                    IDirectory modDir = targetDir.CreateDirectory(key);
                    IFile modSettings = modDir.CreateFile(key + ".json");
                    modSettings.WriteContent(settings);
                    if (storeVersion)
                    {
                        IFile version = modDir.CreateFile(VERSION_TXT);
                        version.WriteContent(vp.SerializedVersion);
                    }
                    LastSentMods.Add(key);
                }
            }
        }

        public void LoadSettings(IDirectory sourceDir, bool checkVersion)
        {
            List<string> errors = new();
            GenerationSettings? randoSettings = null;
            HashSet<string> receivedConnections = new();
            List<Action> modSettingsSetters = new();

            // don't start setting any settings until we've done all the validations!
            if (sourceDir.GetFile(RANDO_JSON) is not IFile randoJson)
            {
                errors.Add("Rando settings were not found in the provided settings");
            }
            else
            {
                randoSettings = JsonConvert.DeserializeObject<GenerationSettings>(randoJson.ReadContent());
                if (randoSettings == null)
                {
                    errors.Add("Failed to read randomizer settings from the provided settings");
                }
            }

            if (checkVersion)
            {
                string receivedRandoVersion;
                if (sourceDir.GetFile(VERSION_TXT) is not IFile randoVersion)
                {
                    errors.Add("A randomizer version was expected, but was missing in the provided settings");
                }
                else if (!randoVersionPolicy.AllowSerialized(receivedRandoVersion = randoVersion.ReadContent()))
                {
                    errors.Add($"The current randomizer version {randoVersionPolicy.SerializedVersion} "
                        + $"did not match the provided version {receivedRandoVersion}");
                }
            }

            foreach (IDirectory modDir in sourceDir.ListDirectories())
            {
                string key = modDir.Name;
                if (!metadata.TryGetValue(key, out ProxyMetadata md))
                {
                    errors.Add($"Received settings for {key} which is not installed or does "
                        + $"not support receiving settings");
                    continue;
                }

                string? settings = modDir.GetFile(key + ".json")?.ReadContent();
                // stage the file consumption after validation checks are done
                modSettingsSetters.Add(() => md.Proxy.ReceiveSerializedSettings(settings));
                // note that we've received settings for this connection
                receivedConnections.Add(key);

                if (checkVersion)
                {
                    string receivedVersion;
                    if (modDir.GetFile(VERSION_TXT) is not IFile ver)
                    {
                        errors.Add($"A version for {key} was expected, but was missing in the provided settings");
                    }
                    else if (!md.VersioningPolicy.AllowSerialized(receivedVersion = ver.ReadContent()))
                    {
                        errors.Add($"The current version for {key} {md.VersioningPolicy.SerializedVersion} "
                            + $"did not match the provided version {receivedVersion}");
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new ValidationException("One or more mods failed validation checks:\n  - "
                    + string.Join("\n  - ", errors));
            }

            LastReceivedMods.Clear();

            ReflectionHelper.CallMethod(RandomizerMenuAPI.Menu, "ApplySettingsToMenu", randoSettings);
            if (randoSettings!.Seed != int.MinValue)
            {
                NumericEntryField<int> seed = ReflectionHelper.GetField<RandomizerMenu, NumericEntryField<int>>(
                    RandomizerMenuAPI.Menu, "SeedEntryField");
                seed.SetValue(randoSettings.Seed);
            }
            LastReceivedMods.Add("Randomizer 4");
            foreach (Action stagedSetter in modSettingsSetters)
            {
                stagedSetter();
            }
            LastReceivedMods.AddRange(receivedConnections);
            foreach (string unreceived in metadata.Keys.Except(receivedConnections))
            {
                metadata[unreceived].Proxy.ReceiveSerializedSettings(null);
                LastModsReceivedWithoutSettings.Add(unreceived);
            }
        }
    }
}
