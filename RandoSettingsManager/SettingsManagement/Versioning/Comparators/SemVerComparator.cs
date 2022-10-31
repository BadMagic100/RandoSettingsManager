using System.Collections.Generic;
using System.Linq;

namespace RandoSettingsManager.SettingsManagement.Versioning.Comparators
{
    /// <summary>
    /// A comparer which implements semantic version comparison.
    /// </summary>
    public class SemVerComparator : Comparer<string>
    {
        readonly string? suffixSeparator;

        /// <summary>
        /// Constructs a SemVerComparer
        /// </summary>
        /// <param name="suffixSeparator">A string to use to remove suffixes, eg + for a mod versioned like X.Y.Z.W+hash</param>
        public SemVerComparator(string? suffixSeparator = null)
        {
            this.suffixSeparator = suffixSeparator;
        }

        /// <inheritdoc/>
        public override int Compare(string x, string y)
        {
            string x1 = RemoveSuffix(x);
            string y1 = RemoveSuffix(y);

            // if either version has "extra" versions, zip will ignore them; this allows, for example, saying that
            // you're backwards compatible until X.Y.*.* in a system where * is auto-set
            IEnumerable<(int, int)> versions = x.Split('.').Zip(y.Split('.'), (a, b) => (int.Parse(a), int.Parse(b)));
            foreach ((int a, int b) in versions)
            {
                int c = a.CompareTo(b);
                if (c != 0)
                {
                    return c;
                }
            }
            return 0;
        }

        private string RemoveSuffix(string s)
        {
            if (suffixSeparator == null)
            {
                return s;
            }

            int i = s.IndexOf(suffixSeparator);
            if (i != -1)
            {
                return s.Remove(i);
            }
            else
            {
                return s;
            }
        }
    }
}
