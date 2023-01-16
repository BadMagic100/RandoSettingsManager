# Introduction

This article gives an introduction to RandoSettingsManager (RSM) and some key concepts.

## Why should you care about RandoSettingsManager?

RandoSettingsManager provides improved support for connections on top of Rando's built-in settings management features.
This allows players to more easily save their settings in profiles for later reuse, or quickly share their settings
with other players for races or itemsyncs without having to dig into various connection menus. To put it simply,
creating settings is one of the least pleasant parts of the rando experience, and integrating with RSM is the best
way to help players simplify that process while your mod is installed.

## The Settings Proxy

The Settings Proxy is your primary interface to interact with RSM. It is responsible for collecting your settings to be
serialized, and applying received settings to your connection menu after they have been deserialized. The simplest way
get started with RSM is to create a [`SimpleSettingsProxy`](~/api/RandoSettingsManager.SettingsManagement.SimpleSettingsProxy-1.yml).

```cs
RandoSettingsManagerMod.Instance.RegisterConnection(
    new SimpleSettingsProxy<MySettings>(MyMod.Instance,
        (MySettings? settings) => { ... },
        () => { ... }
    )
);
```

SimpleSettingsProxy provides some shortcuts around naming and versioning for you. Now is also a good time to note that
providing your settings is optional, but receiving settings is required. This is because even if your settings are too
complex to share (provide) easily, it's recommended to provide code to disable your mod when no settings are received.
This results in less manual effort and less margin for error, especially when players are setting up for races and 
itemsyncs, and therefore a more pleasant experience. Conversely, also note that connections should not send settings
if they are disabled to prevent issues for players which do not have the mod installed.

> [!WARNING]
> Remember that the binding from menu to settings is one way; when you receive settings you should apply them to your
> menu and let MenuChanger update the settings. If you update your settings directly, your menu will not reflect the
> changes and any further changes will not be reflected as your menu will likely hold a dangling reference to the old
> settings object.

`SimpleSettingsProxy` is the simplest way to get started with RSM, but you may find that you need more control over
naming and versioning policies. It is also possible to derive directly from
[`RandoSettingsProxy`](~/api/RandoSettingsManager.SettingsManagement.RandoSettingsProxy-2.yml) to achieve this control.

## The Versioning Policy

Versioning in RSM is used as a way to perform early validation on settings codes. Any potentially hash-impacting
changes should result in an incompatibility in version. This is used only with RSM's sharing features to prevent
hash mismatches when setting up races and itemsyncs. Saved profiles do not store or check version information.

The easiest way to version your mod is by using a [`StrictModVersioningPolicy`](~/api/RandoSettingsManager.SettingsManagement.Versioning.StrictModVersioningPolicy.yml).
As the name implies, this is the strictest policy available, and will break compatibility any time your mod version is
updated, effectively forcing everyone to always be on the same (usually latest) mod version. This is the versioning
policy used by `SimpleSettingsProxy` and by Rando itself.

While strict versioning guarantees correctness by breaking compatibility every release, it may be desirable for some
developers to employ permissive policies, especially if you yourself race with dev builds installed or your mod updates
very frequently. A more flexible versioning policy can be a great convenience for players if implemented property.
However, doing so is a very complex topic and can leave a lot of room for error if. Versioning best practices are
discussed in more detail in the article on [versioning best practices](~/articles/versioning.md).

> [!TIP]
> Don't feel at all obligated to implement a versioning policy! For many mods, strict versioning will work perfectly
> fine. Using a versioning policy which breaks compatibility more often then it needs to, such as strict versioning, 
> is much better than implementing versioning incorrectly and causing preventable hash mismatches for opaque reasons.