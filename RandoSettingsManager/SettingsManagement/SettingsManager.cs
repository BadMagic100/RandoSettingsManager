using MenuChanger.MenuElements;
using Modding;
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
        readonly JsonConverter jsonConverter = new();
        ISerializableVersioningPolicy? randoVersionPolicy;

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
            rando.WriteContent(jsonConverter.Serialize(gs));
            if (storeVersion)
            {
                IFile version = targetDir.CreateFile(VERSION_TXT);
                randoVersionPolicy ??= new StrictModVersioningPolicy((Mod)ModHooks.GetMod("Randomizer 4"));
                version.WriteContent(randoVersionPolicy.GetSerializedVersion(jsonConverter));
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
                if (proxy.TryProvideSerializedSettings(jsonConverter, out string? settings) && settings != null)
                {
                    IDirectory modDir = targetDir.CreateDirectory(key);
                    IFile modSettings = modDir.CreateFile(key + ".json");
                    modSettings.WriteContent(settings);
                    if (storeVersion)
                    {
                        IFile version = modDir.CreateFile(VERSION_TXT);
                        version.WriteContent(vp.GetSerializedVersion(jsonConverter));
                    }
                    LastSentMods.Add(key);
                }
            }
        }

        public void DisableAllConnections()
        {
            LastReceivedMods.Clear();
            LastModsReceivedWithoutSettings.Clear();
            foreach (KeyValuePair<string, ProxyMetadata> md in metadata)
            {
                md.Value.Proxy.ReceiveSerializedSettings(jsonConverter, null);
                LastModsReceivedWithoutSettings.Add(md.Key);
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
                randoSettings = jsonConverter.Deserialize<GenerationSettings>(randoJson.ReadContent());
                if (randoSettings == null)
                {
                    errors.Add("Failed to read randomizer settings from the provided settings");
                }
            }

            if (checkVersion)
            {
                randoVersionPolicy ??= new StrictModVersioningPolicy((Mod)ModHooks.GetMod("Randomizer 4"));

                string receivedRandoVersion;
                if (sourceDir.GetFile(VERSION_TXT) is not IFile randoVersion)
                {
                    errors.Add("A randomizer version was expected, but was missing in the provided settings");
                }
                else if (!randoVersionPolicy.AllowSerialized(jsonConverter, receivedRandoVersion = randoVersion.ReadContent()))
                {
                    errors.Add($"The current randomizer version {randoVersionPolicy.GetSerializedVersion(jsonConverter)} "
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
                modSettingsSetters.Add(() => md.Proxy.ReceiveSerializedSettings(jsonConverter, settings));
                // note that we've received settings for this connection
                receivedConnections.Add(key);

                if (checkVersion)
                {
                    string receivedVersion;
                    if (modDir.GetFile(VERSION_TXT) is not IFile ver)
                    {
                        errors.Add($"A version for {key} was expected, but was missing in the provided settings");
                    }
                    else if (!md.VersioningPolicy.AllowSerialized(jsonConverter,receivedVersion = ver.ReadContent()))
                    {
                        errors.Add($"The current version for {key} {md.VersioningPolicy.GetSerializedVersion(jsonConverter)} "
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
            LastModsReceivedWithoutSettings.Clear();

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
                try
                {
                    stagedSetter();
                }
                catch (ValidationException ve)
                {
                    errors.Add(ve.Message);
                }
            }
            LastReceivedMods.AddRange(receivedConnections);
            foreach (string unreceived in metadata.Keys.Except(receivedConnections))
            {
                metadata[unreceived].Proxy.ReceiveSerializedSettings(jsonConverter, null);
                LastModsReceivedWithoutSettings.Add(unreceived);
            }

            if (errors.Count > 0)
            {
                throw new LateValidationException("One or more mods encountered partial validation errors. Some mods may not" +
                    " have had their settings completely applied.\n - " + string.Join("\n - ", errors));
            }
        }
    }
}
