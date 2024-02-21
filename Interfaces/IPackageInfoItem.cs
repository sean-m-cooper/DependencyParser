using DependencyParser.Enums;
using System.Runtime.CompilerServices;

namespace DependencyParser.Interfaces
{
    public interface IPackageInfoItem
    {
        string PackageName { get; set; }
        PackageSource Source { get; set; }
        Version CurrentVersion { get; set; }
        Version MaxVersion { get; set; }

        bool Equals(object? obj);
        bool Equals(IPackageInfoItem? other);
        int GetHashCode();
    }
}