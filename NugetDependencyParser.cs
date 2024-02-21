using DependencyParser.Enums;
using DependencyParser.Interfaces;
using System.Text;
using System.Xml.Linq;

namespace DependencyParser
{
    public class NugetDependencyParser : DependencyParserBase, IDependencyParser
    {
        private NugetRepositoryHelper _nugetRepoHelper { get; set; }

        public NugetDependencyParser()
        {
            _nugetRepoHelper = new NugetRepositoryHelper();
        }

        /// <summary>
        /// Returns a <see cref="List{T}"/> of <see cref="IPackageInfoItem"/> based on the provided file path
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<List<IPackageInfoItem>> GetPackageInfosAsync(string filePath)
        {
            var fileContents = await EnsureProjectFileAsync(filePath);

            var dependencyInfoElements = GetPackageReferenceElements(fileContents);

            List<IPackageInfoItem> packageInfos = new List<IPackageInfoItem>();
            packageInfos = dependencyInfoElements.Select(p =>
                new PackageInfoItem(p.Attribute("Include").Value.ToString(), HygieneVersion(p.Attribute("Version").Value.ToString()), PackageSource.Nuget))
                .ToList<IPackageInfoItem>();

            packageInfos.ForEach(async pi => pi.MaxVersion = await GetMaxVersionValueAsync(pi));
            return packageInfos.OrderBy(d => d.PackageName).ToList();
        }

        private async Task<string> EnsureProjectFileAsync(string filePath)
        {
            string fileContents = "";
            if (File.Exists(filePath))
            {
                fileContents = await File.ReadAllTextAsync(filePath);
            }
            return (!string.IsNullOrEmpty(fileContents)) ? HygieneProjectFile(fileContents) : fileContents;
        }

        private List<XElement> GetPackageReferenceElements(string fileContents)
        {
            var projectDoc = XDocument.Parse(fileContents);
            List<XElement> dependencyInfoElements = new List<XElement>();
            return projectDoc.Descendants("PackageReference").ToList();
        }

        private string HygieneProjectFile(string file)
        {
            string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (file.StartsWith(_byteOrderMarkUtf8))
            {
                file = file.Remove(0, _byteOrderMarkUtf8.Length);
                if (!file.StartsWith("<"))
                {
                    file = "<" + file;
                }
            }

            return file;
        }

        /// <summary>
        /// returns a <see cref="PackageInfoItem"/> with the version set to the latest version available from Nuget
        /// </summary>
        /// <param name="currentPackageInfo"></param>
        /// <returns></returns>
        public async Task<Version> GetMaxVersionValueAsync(IPackageInfoItem currentPackageInfo)
        {
            var packageVersions = await _nugetRepoHelper.GetPackageVersionsAsync(currentPackageInfo.PackageName);
            return packageVersions.OrderByDescending(v => HygieneVersion(v.Version.ToString())).FirstOrDefault().Version;
        }
    }
}
