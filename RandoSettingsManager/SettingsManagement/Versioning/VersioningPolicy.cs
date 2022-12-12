using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;

namespace RandoSettingsManager.SettingsManagement.Versioning
{
    /// <summary>
    /// A policy to determine what the current version is, and whether a received version is allowable
    /// </summary>
    /// <remarks>
    /// <para>
    /// Versioning is used as an early validation mechanism for settings sharing. Changes to settings themselves,
    /// or logic (or anything else hash-impacting) should result in a change in version. Long-lived profiles are
    /// never version-checked.
    /// </para>
    /// 
    /// <para>
    /// When choosing or implementing a versioning policy consider that, for any pair of builds of the same mod,
    /// the versioning policies should symmetrically accept or reject the version provided by the other build.
    /// A version should be accepted if and only if it can be verified that any possible settings sent by the
    /// sender will generate the same hash when applied, based on the sent version.
    /// </para>
    /// 
    /// <para>
    /// When in doubt, prefer a stricter policy. <see cref="StrictModVersioningPolicy"/> is the most strict
    /// policy available in this library.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type used to store version info</typeparam>
    public abstract class VersioningPolicy<T> : ISerializableVersioningPolicy
    {
        /// <summary>
        /// The current version
        /// </summary>
        public abstract T Version { get; }

        /// <summary>
        /// Determines whether the given version is allowed
        /// </summary>
        /// <param name="version">The version to check against the policy</param>
        /// <returns>Whether the given version is allowed</returns>
        public abstract bool Allow(T version);

        string ISerializableVersioningPolicy.SerializedVersion => JsonConvert.SerializeObject(Version,
            new StringEnumConverter(new DefaultNamingStrategy()));

        bool ISerializableVersioningPolicy.AllowSerialized(string version)
        {
            T? ver;
            try
            {
                ver = JsonConvert.DeserializeObject<T>(version, new StringEnumConverter(new DefaultNamingStrategy()));
            }
            catch (Exception ex)
            {
                RandoSettingsManagerMod.Instance.Log($"Encountered deserialization failure while reading {version}");
                RandoSettingsManagerMod.Instance.LogError(ex);
                return false;
            }
            // if we're unable to recognize the type, of course we cannot allow the provided version
            if (ver == null)
            {
                return false;
            }
            return Allow(ver);
        }
    }
}
