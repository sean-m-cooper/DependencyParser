namespace DependencyParser.Interfaces
{
    public interface IDependencyParser
    {
        Task<List<IPackageInfoItem>> GetPackageInfosAsync(string filePath);
        Task<Version> GetMaxVersionValueAsync(IPackageInfoItem currentPackageInfo);
    }
}
