using System;
using System.Globalization;

namespace Unity.AssetManager.Core.Editor
{
    /// <summary>
    /// Simple semantic versioning for Unity packages.
    /// We only need to know major, minor and patch versions.
    /// Pre-release identifiers are not supported and mark the version as not a release.
    /// </summary>
    struct SemanticVersion : IComparable<SemanticVersion>
    {
        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }
        public string PreReleaseIdentifiers { get; }
        public bool IsRelease => string.IsNullOrEmpty(PreReleaseIdentifiers);


        SemanticVersion(int major, int minor, int patch, string preReleaseIdentifiers)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            PreReleaseIdentifiers = preReleaseIdentifiers ?? string.Empty;
        }

        /// <summary>
        /// Parses a semantic version string in the format "MAJOR.MINOR.PATCH[-PRERELEASE]"
        /// Build metadata (+...) is ignored.
        /// Only major, minor and patch are considered for comparison.
        /// Pre-release versions are considered lower precedence than the associated normal version.
        /// Examples of valid versions: "1.0.0", "2.1.3-pre.1", "0.9.0-exp"
        /// <see href="https://semver.org/">https://semver.org/</see>
        /// </summary>
        /// <param name="input">Version string to parse</param>
        /// <param name="version">Parsed SemanticVersion if successful, otherwise default</param>
        /// <returns>Success status of the parse operation</returns>
        public static bool TryParse(string input, out SemanticVersion version)
        {
            version = default;
            if (string.IsNullOrEmpty(input))
                return false;

            // Strip build metadata (+...)
            var plusIndex = input.IndexOf('+');
            var core = plusIndex >= 0 ? input.Substring(0, plusIndex) : input;

            // Split pre-release (-...)
            var hyphenIndex = core.IndexOf('-');
            var coreVersion = hyphenIndex >= 0 ? core.Substring(0, hyphenIndex) : core;
            var preRelease = hyphenIndex >= 0 ? core.Substring(hyphenIndex + 1) : string.Empty;

            var parts = coreVersion.Split('.');
            if (parts.Length < 3)
                return false;

            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var major))
                return false;
            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var minor))
                return false;
            if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var patch))
                return false;

            version = new SemanticVersion(major, minor, patch, preRelease);
            return true;
        }

        /// <summary>
        /// <see cref="TryParse(string, out SemanticVersion)"/>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if the input string is not a valid semantic version</exception>
        public static SemanticVersion Parse(string input)
        {
            if (!TryParse(input, out var v))
            {
                throw new ArgumentException($"Invalid semantic version: {input}");
            }

            return v;
        }

        /// <summary>
        /// Compares this version to another semantic version.
        /// Comparison is done by major, minor, patch and pre-release identifiers.
        /// Pre-release versions have lower precedence than the associated normal version.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(SemanticVersion other)
        {
            var c = Major.CompareTo(other.Major);
            if (c != 0) return c;
            c = Minor.CompareTo(other.Minor);
            if (c != 0) return c;
            c = Patch.CompareTo(other.Patch);
            if (c != 0) return c;

            // Handle pre-release precedence: absence of pre-release has higher precedence
            var hasPre = !string.IsNullOrEmpty(PreReleaseIdentifiers);
            var otherHasPre = !string.IsNullOrEmpty(other.PreReleaseIdentifiers);

            // Pre-release has lower precedence
            if (hasPre && !otherHasPre) return -1;
            // Release has higher precedence
            if (!hasPre && otherHasPre) return 1;
            // If both are pre-release, or both are release, consider equal

            return 0;
        }

        /// <summary>
        /// Returns the string representation of the semantic version.
        /// </summary>
        /// <returns>String representation of the semantic version</returns>
        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}" + (string.IsNullOrEmpty(PreReleaseIdentifiers) ? string.Empty : $"-{PreReleaseIdentifiers}");
        }

        public static int Compare(SemanticVersion a, SemanticVersion b)
        {
            return a.CompareTo(b);
        }

        public static int Compare(string a, string b)
        {
            var va = Parse(a);
            var vb = Parse(b);
            return va.CompareTo(vb);
        }
    }
}
