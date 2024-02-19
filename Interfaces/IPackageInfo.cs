using DependencyParser.Enums;

namespace DependencyParser.Interfaces
{
    public interface IPackageInfo
    {
        string PackageName { get; set; }
        PackageSource Source { get; set; }
        Version CurrentVersion { get; set; }
        Version MaxVersion { get; set; }

        bool Equals(object? obj);
        bool Equals(IPackageInfo? other);
        int GetHashCode();
    }
}