using System.Security.Cryptography;
using System.Text;

namespace RandoSettingsManager.SettingsManagement.Versioning
{
    /// <summary>
    /// A versioning policy which compares the hash of provided content
    /// </summary>
    public class ContentHashVersioningPolicy : VersioningPolicy<string>
    {
        /// <inheritdoc/>
        public override string Version { get; }

        /// <summary>
        /// Constructs a policy based off of the provided content
        /// </summary>
        /// <param name="content">The content to hash</param>
        public ContentHashVersioningPolicy(byte[] content)
        {
            using SHA1Managed sha1 = new();

            byte[] hash = sha1.ComputeHash(content);
            StringBuilder sb = new(hash.Length * 2);
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("X2"));
            }
            Version = sb.ToString();
        }

        /// <inheritdoc/>
        public override bool Allow(string version) => version == Version;
    }
}
