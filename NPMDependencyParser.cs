using DependencyParser.Enums;
using DependencyParser.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DependencyParser
{
    public class NpmDependencyParser : DependencyParserBase, IDependencyParser, IDisposable
    {
        private HttpClient _httpClient;
        public NpmDependencyParser()
        {
            _httpClient = new HttpClient();
        }
        /// <summary>
        /// Returns a <see cref="List{T}"/> of <see cref="PackageInfoItem"/> based on the provided file path
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<List<IPackageInfoItem>> GetPackageInfosAsync(string filePath)
        {
            JObject packageInfo = await GetPackageInfoFromFileAsync(filePath);

            List<IPackageInfoItem>? packageInfos = CreatePackageInfosFromJson(packageInfo);
            packageInfos.ForEach(async pi => pi.MaxVersion = await GetMaxVersionValueAsync(pi));
            return packageInfos.OrderBy(d => d.PackageName).ToList();
        }

        private List<IPackageInfoItem>? CreatePackageInfosFromJson(JObject packageInfo)
        {
            var dependencies = packageInfo.SelectToken("dependencies");
            var packageInfos = dependencies?
                .Children<JProperty>()
                .Select(p => CreateDependencyInfo(p))
                .ToList();
            return packageInfos;
        }

        private async Task<JObject> GetPackageInfoFromFileAsync(string filePath)
        {
            var fileContents = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<JObject>(fileContents);
        }

        private IPackageInfoItem CreateDependencyInfo(JProperty prop)
        {
            var name = prop.Name.Replace("@", "");
            string versionValue = ParsePackageVersion(prop);

            var packageInfo = new PackageInfoItem(name, HygieneVersion(versionValue), PackageSource.NPM);
            return packageInfo;
        }

        private string ParsePackageVersion(JProperty prop)
        {
            var versionValue = prop.Value.ToString().Replace("^", "").Replace("=", "").Replace("~", "").Trim();
            if (versionValue != null && versionValue.StartsWith("git")) versionValue = "0.0.0";
            if (versionValue != null && versionValue.Contains('-'))
            {
                var index = versionValue.IndexOf("-");
                versionValue = versionValue[..index];
            }

            return !String.IsNullOrEmpty(versionValue) ? versionValue : "";
        }

        /// <summary>
        /// Returns a <see cref="PackageInfoItem" /> with the version set to the latest version available from NPM
        /// </summary>
        /// <param name="currentPackageInfo"></param>
        /// <returns></returns>
        public async Task<Version> GetMaxVersionValueAsync(IPackageInfoItem currentPackageInfo)
        {
            string maxVersion = PackageInfoItem.DEFAULT_VERSION_NUMBER;
            var packageName = currentPackageInfo.PackageName;

            var npmCacheStream = await TryGetNPMCacheStreamAsync(packageName, _httpClient);
            if (npmCacheStream != null)
            {
                var npmPackageJson = GetJsonFromStream<JObject>(npmCacheStream);
                string versionString = npmPackageJson["dist-tags"]["latest"].Value<string>();
                maxVersion = HygieneVersion(versionString);
            }
            return new Version(maxVersion);
        }

        public async Task<Stream?> TryGetNPMCacheStreamAsync(string packageName, HttpClient client)
        {
            var npmCacheStream = await DownloadStreamFromUrlAsync($"https://registry.npmjs.org/{packageName}", client);
            if (npmCacheStream == null)
            {
                packageName = "@" + packageName;
                npmCacheStream = await DownloadStreamFromUrlAsync($"https://registry.npmjs.org/{packageName}", client);
            }
            return npmCacheStream;
        }

        private T GetJsonFromStream<T>(Stream stream)
        {
            var serializer = new JsonSerializer();
            using var sr = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(sr);
            return serializer.Deserialize<T>(jsonTextReader);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
