# Settings Management Cookbook

This article lists some common patterns which may be used to accomplish tasks in RandoSettingsManager, including some
pointers on more advanced topics and corresponding technical challenges.

These snippets are meant to be mostly copy-pasteable. Imports and other details not related to RSM integration are
omitted for brevity.

## Basic Implementation

```cs
class YourMod
{
    // ...
    
    void Initialize()
    {
        if (ModHooks.GetMod("RandoSettingsManager") is Mod)
        {
            HookRSM();
        }
    }
    
    void HookRSM()
    {
        RandoSettingsManagerMod.Instance.RegisterConnection(new SimpleSettingsProxy<YourSettings>(
            this,
            (settings) => RandomizerMenu.Instance.PasteSettings(settings),
            () => GS.Enabled ? GS : null,
        );
    }
}

class RandomizerMenu
{
    // ...

    // note that we're setting values in the IValueElements/MEF which updates both the menu
    // and the underlying data. The inverse is not true.
    void PasteSettings(YourSettings? settings)
    {
        if (settings == null)
        {
            mef.ElementLookup[nameof(YourSettings.Enabled)].SetValue(false);
            return;
        }
        
        mef.SetMenuValues(settings);
    }
}
```

## Custom RandoSettingsProxy

```cs
internal static class SettingsManagement
{
    // to be called after checking RSM exists
    public static void Hook()
    {
        RandoSettingsManagerMod.Instance.RegisterConnection(new YourSettingsProxy());
    }
}

internal class YourSettingsProxy : RandoSettingsProxy<YourRandomizationSettings, 
    (string, string, string, string, string)>
{
    public override string ModKey => YourMod.Instance.GetName();

    public override VersioningPolicy<(string, string, string, string, string)> VersioningPolicy { get; }

    public YourSettingsProxy()
    {
        Assembly a = typeof(YourSettingsProxy).Assembly;
        using Stream macros = a.GetManifestResourceStream("YourMod.Resources.Logic.macros.json");
        using Stream terms = a.GetManifestResourceStream("YourMod.Resources.Logic.terms.json");
        using Stream waypoints = a.GetManifestResourceStream("YourMod.Resources.Logic.waypoints.json");
        using Stream location = a.GetManifestResourceStream("YourMod.Resources.Logic.locations.json");

        // version based off major.minor for settings (changes to settings will bump mod version),
        // and the content of all logic-modifying files
        VersioningPolicy = CompoundVersioningPolicy.Of(
            new EqualityVersioningPolicy<string>(YourMod.Instance.GetVersion(), new SemVerComparator(places: 2)),
            new ContentHashVersioningPolicy(locations),
            new ContentHashVersioningPolicy(macros),
            new ContentHashVersioningPolicy(terms),
            new ContentHashVersioningPolicy(waypoints),
            new ContentHashVersioningPolicy(locations)
        );
    }

    public override void ReceiveSettings(YourRandomizationSettings? settings)
    {
        if (settings != null)
        {
            ConnectionMenu.Instance!.ApplySettingsToMenu(settings);
        }
        else
        {
            ConnectionMenu.Instance!.Disable();
        }
    }

    public override bool TryProvideSettings(out YourRandomizationSettings? settings)
    {
        settings = RandoInterop.Settings;
        return settings.Enabled;
    }
}
```

## Structured Versioning

Structured versioning is a high-effort pattern which essentially lists the enabled settings as part of the mod version.
This approach is useful if you have several toggleable settings which only affect logic if enabled. This example is
taken from Transcendence, where Pimpas wanted changes to logic files to impact the version because it's hash-impacting,
but only if that subset of logic was enabled (Transcendence logic is togglable). This means that, for example, a
future Transcendence version would be backward compatible with the current version if all logic is off.

```cs
internal class StructuralVersioningPolicy : VersioningPolicy<Signature>
{
    internal Func<RandoSettings> settingsGetter;

    public override Signature Version => new() { FeatureSet = FeatureSetForSettings(settingsGetter()) };

    // list only enabled features in the version
    private static List<string> FeatureSetForSettings(RandoSettings rs) =>
        SupportedFeatures.Where(f => f.feature(rs)).Select(f => f.name).ToList();

    // allow if the feature set provided by the sender only contains supported features (i.e. doesn't contain extra
    // things we don't know how to handle)
    public override bool Allow(Signature s) => s.FeatureSet.All(name => SupportedFeatures.Any(sf => sf.name == name));

    private static List<(Predicate<RandoSettings> feature, string name)> SupportedFeatures = new()
    {
        (rs => rs.AddCharms, "AddCharms"),
        (rs => rs.IncreaseMaxCharmCostBy > 0, "IncreaseMaxCharmCost"),
        (rs => rs.Logic.AntigravityAmulet, "AntigravLogic"),
        (rs => rs.Logic.BluemothWings == GeoCharmLogicMode.OnWithGeo, "BluemothGeoLogic"),
        (rs => rs.Logic.BluemothWings == GeoCharmLogicMode.On, "BluemothLogic"),
        (rs => rs.Logic.SnailSoul, "SnailSoulLogic"),
        (rs => rs.Logic.SnailSlash, "SnailSlashLogic"),
        (rs => rs.Logic.MillibellesBlessing, "MillibelleLogic"),
        (rs => rs.Logic.NitroCrystal, "NitroLogic"),
        (rs => rs.Logic.Crystalmaster == GeoCharmLogicMode.OnWithGeo, "CrystalmasterGeoLogic"),
        (rs => rs.Logic.Crystalmaster == GeoCharmLogicMode.On, "CrystalmasterLogic"),
        (rs => rs.Logic.ChaosOrb != ChaosOrbMode.Off, "ChaosLogic"),
        (rs => rs.Logic.AnyEnabled(), "LogicHash:" + LogicHash())
    };

    private static string LogicHash()
    {
        if (!Transcendence.LogicAvailable())
        {
            return "NIL";
        }

        using var hash = SHA256.Create();
        using var hstream = new CryptoStream(Stream.Null, hash, CryptoStreamMode.Write);
        var modDir = Path.GetDirectoryName(typeof(StructuralVersioningPolicy).Assembly.Location);
        foreach (var name in new string[] { "LogicMacros.json", "LogicPatches.json", "ConnectionLogicPatches.json" })
        {
            using (var logicFile = File.OpenRead(Path.Combine(modDir, name)))
            {
                logicFile.CopyTo(hstream);
            }
        }
        hash.TransformFinalBlock(new byte[] {}, 0, 0);
        return ToHex(hash.Hash);
    }

    private static string ToHex(byte[] stuff)
    {
        var sb = new StringBuilder(stuff.Length * 2);
        foreach (var b in stuff)
        {
            sb.AppendFormat("{0:x2}", b);
        }
        return sb.ToString();
    }
}

internal struct Signature
{
    public List<string> FeatureSet;
}
```

## Sending and Receiving External Presets

For mods which have substantially large preset settings which are stored and managed on disk, it can be desirable to
share the content of the selected preset(s) using RSM, rather than requiring the preset files to be shared separately
beforehand.

```cs
// In this example, each pack is a single plaintext file in the DataPacks directory of the saves folder.
//
// This approach can easily be adapted to multi-file packs by searching for directories, and then including the content
// of each file. Depending on how you choose to store the names of profiles, you may desire to store the path as well
// (e.g. if name is defined in an additional metadata file). You can also easily deserialize your content to some data
// structure (e.g. from JSON) when loading from disk if desired.
//
// If you allow multiple selected packs, you would just replace most DataPack with List<DataPack>
internal class DataPack
{
    public static readonly string PacksDirectory = Path.Combine(Application.persistentDataPath, "DataPacks");

    public string PackName { get; set; }

    public string PackContent { get; }

    [JsonConstructor]
    private DataPack(string packName, string packContent)
    {
        PackName = packName;
        PackContent = packContent;
    }

    private DataPack(string path)
    {
        PackName = Path.GetFileNameWithoutExtension(path);
        using StreamReader sr = File.OpenText(path);
        // you can deserialize this to a data structure of your choosing if desired
        PackContent = sr.ReadToEnd();
    }

    public void WriteToDisk()
    {
        if (!Directory.Exists(PacksDirectory))
        {
            Directory.CreateDirectory(PacksDirectory);
        }

        using StreamWriter sw = File.CreateText(Path.Combine(PacksDirectory, PackName + ".txt"));
        sw.Write(PackContent);
    }

    // if pack content is large, it may be preferable to make the constructor above public,
    // and only create a shareable pack for the single selected pack on-demand when sending
    // settings - this kills several birds with one stone though as it lets you list available
    // packs and loads/hashes the content all in one step
    public static List<DataPack> LoadPacksFromDisk()
    {
        List<DataPack> results = new();
        if (Directory.Exists(PacksDirectory))
        {
            foreach (string file in Directory.EnumerateFiles(PacksDirectory, "*.txt"))
            {
                results.Add(new DataPack(file));
            }
        }
        return results;
    }
}

internal class Menu
{
    private static List<DataPack> loadedPacks = new();

    public static void LoadPacks()
    {
        loadedPacks = DataPack.LoadPacksFromDisk();
    }

    public DataPack? GetSelectedPack()
    {
        // Implementation of this is highly dependent on how your menu does pack selection/display.
        ...
    }

    public void ApplyPack(DataPack pack)
    {
        // Implementation of this is highly dependent on how your menu does pack selection/display.
        // You may need to add new elements/options to your menu to accomodate new packs; this is
        // stubbed out here (by SelectPackByName);

        bool reapply = false;
        foreach (DataPack p in loadedPacks)
        {
            if (pack.PackName == p.PackName)
            {
                // duplicate detection - if using a custom structure, override Equals/HashCode and use that.
                if (pack.PackContent == p.PackContent)
                {
                    // this is a known profile and we have it already, select it
                    SelectPackByName(pack.PackName);
                    return;
                }
                else
                {
                    // rename it and reapply with new name
                    pack.PackName += " (2)";
                    reapply = true;
                    break;
                }
            }
        }

        if (reapply)
        {
            ApplyPack(pack);
        }
        else
        {
            // never-before-seen, completely new pack with unique name
            pack.WriteToDisk();
            loadedPacks.Add(pack);
            SelectPackByName(pack.PackName);
        }
    }
}

internal class SettingsProxy : RandoSettingsProxy<DataPack, string>
{
    public override string ModKey => YourMod.Instance.GetName();

    public override VersioningPolicy<string> VersioningPolicy { get; } 
        = new StrictModVersioningPolicy(YourMod.Instance);

    public override bool TryProvideSettings(out DataPack? settings)
    {
        settings = Menu.Instance.GetSelectedPack();
        return YourMod.Instance.GS.Enabled;
    }

    public override void ReceiveSettings(DataPack? settings)
    {
        if (settings == null)
        {
            Menu.Instance.Disable();
        }
        else
        {
            Menu.Instance.ApplyPack(settings);
        }
    }
}
```