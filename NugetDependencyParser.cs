using DependencyParser.Enums;
using DependencyParser.Interfaces;
using System.Text;
using System.Xml.Linq;

namespace DependencyParser
{
    public class NugetDependencyParser : DependencyParserBase
    {
        private NugetRepositoryHelper _nugetRepoHelper { get; set; }

        public NugetDependencyParser()
        {
            _nugetRepoHelper = new NugetRepositoryHelper();
        }

        /// <summary>
        /// Returns a <see cref="List{T}"/> of <see cref="IPackageInfo"/> based on the provided file path
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public override async Task<List<IPackageInfo>> GetPackageInfosAsync(string filePath)
        {
            var fileContents = await EnsureProjectFileAsync(filePath);

            var dependencyInfoElements = GetPackageReferenceElements(fileContents);

            List<IPackageInfo> packageInfos = new List<IPackageInfo>();
            packageInfos = dependencyInfoElements.Select(p =>
                new PackageInfo(p.Attribute("Include").Value.ToString(), HygieneVersion(p.Attribute("Version").Value.ToString()), PackageSource.Nuget))
                .ToList<IPackageInfo>();

            packageInfos.ForEach(async pi => pi.MaxVersion = await GetNugetMaxVersionValueAsync(pi));
            return packageInfos;
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
        /// returns a <see cref="PackageInfo"/> with the version set to the latest version available from Nuget
        /// </summary>
        /// <param name="currentPackageInfo"></param>
        /// <returns></returns>
        private async Task<Version> GetNugetMaxVersionValueAsync(IPackageInfo currentPackageInfo)
        {
            var packageVersions = await _nugetRepoHelper.GetPackageVersionsAsync(currentPackageInfo.PackageName);
            return packageVersions.OrderByDescending(v => HygieneVersion(v.Version.ToString())).FirstOrDefault().Version;
        }

        public override Task<Version> GetMaxVersionValueAsync(IPackageInfo currentPackageInfo)
        {
            throw new NotImplementedException();
        }
    }
}
