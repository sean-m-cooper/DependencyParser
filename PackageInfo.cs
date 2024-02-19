using DependencyParser.Enums;
using DependencyParser.Interfaces;

namespace DependencyParser
{
    /// <summary>
    /// Represents the information about a package dependency
    /// </summary>
    public partial class PackageInfo : IEquatable<IPackageInfo>, IPackageInfo
    {
        public const string DEFAULT_VERSION_NUMBER = "0.0.0";

        public string PackageName { get; set; }
        public Version CurrentVersion { get; set; }
        public PackageSource Source { get; set; }
        public Version MaxVersion { get; set; }

        public PackageInfo(string packageName, string version, PackageSource source)
        {
            if (string.IsNullOrEmpty(version))
            {
                CurrentVersion = new Version(0, 0, 0);
            }
            else
            {
                CurrentVersion = new Version(version);
            }
            PackageName = packageName;

            Source = source;
        }

        public bool Equals(IPackageInfo? other)
        {
            if (other == null) throw new ArgumentException(nameof(other));
            return other.PackageName == this.PackageName;
        }
        public override bool Equals(object? obj) => Equals(obj as IPackageInfo);
        public override int GetHashCode() => (PackageName).GetHashCode();

    }
}
