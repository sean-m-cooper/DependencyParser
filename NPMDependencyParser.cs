using DependencyParser.Enums;
using DependencyParser.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DependencyParser
{
    public class NpmDependencyParser : DependencyParserBase
    {

        /// <summary>
        /// Returns a <see cref="List{T}"/> of <see cref="PackageInfo"/> based on the provided file path
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public Task<List<IPackageInfo>> GetPackageInfosAsync(string filePath)
        {
            var fileContents = File.ReadAllText(filePath);
            var packageInfo = JsonConvert.DeserializeObject<JObject>(fileContents);
            var dependencies = (JObject)packageInfo.SelectToken("dependencies");
            var packageInfos = dependencies
                .Children<JProperty>()
                .Select(p => CreateDependencyInfo(p))
                .ToList();
            return packageInfos.OrderBy(d => d.PackageName).ToList<IPackageInfo>();
        }

        /// <summary>
        /// Writes a csv file to the specified path
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void WriteCSVFile(string filePath)
        {
            throw new NotImplementedException();
        }

        private PackageInfo CreateDependencyInfo(JProperty prop)
        {
            var name = prop.Name.Replace("@", "");
            var val = prop.Value.ToString().Replace("^", "").Replace("=", "").Replace("~", "").Trim();
            if (val != null && val.StartsWith("git")) val = "0.0.0";
            if (val != null && val.Contains('-'))
            {
                var index = val.IndexOf("-");
                val = val[..index];
            }
            var packageInfo = new PackageInfo(name, "0.0.0", PackageSource.NPM);
            packageInfo = new PackageInfo(name, HygieneVersion(val), PackageSource.NPM);
            return packageInfo;
        }

        /// <summary>
        /// Returns a <see cref="PackageInfo" /> with the version set to the latest version available from NPM
        /// </summary>
        /// <param name="currentPackageInfo"></param>
        /// <returns></returns>
        public async Task<Version> GetMaxVersionValueAsync(IPackageInfo currentPackageInfo)
        {
            using (var client = new HttpClient())
            {
                var npmPackageInfo = new PackageInfo(currentPackageInfo.PackageName, currentPackageInfo.CurrentVersion.ToString(), currentPackageInfo.Source);

                string maxVersion = PackageInfo.DEFAULT_VERSION_NUMBER;
                var packageName = currentPackageInfo.PackageName;
                var npmCacheStream = await DownloadFileToStreamAsync($"https://registry.npmjs.org/{packageName}");
                if (npmCacheStream == null)
                {
                    packageName = "@" + packageName;
                    npmCacheStream = await DownloadFileToStreamAsync($"https://registry.npmjs.org/{packageName}");
                }
                if (npmCacheStream != null)
                {
                    var npmPackageJson = GetJsonFromStream<JObject>(npmCacheStream);
                    maxVersion = npmPackageJson["dist-tags"]["latest"].Value<string>();
                }
               return new Version(HygieneVersion(maxVersion));
            }

        }
        private async Task<Stream?> DownloadFileToStreamAsync(string url)
        {
            using (HttpClient client = new())
            {
                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var responseBody = await response.Content.ReadAsStreamAsync();
                    return responseBody;
                }
                catch (HttpRequestException httpEx)
                {
                    var message = $"HttpError - Url: {url} Message :{httpEx.Message}";
                }
                catch (Exception ex)
                {
                    var message = $"Error - Url: {url} Message :{ex.Message}";
                }
            }
            return null;
        }

        private T GetJsonFromStream<T>(Stream stream)
        {
            var serializer = new JsonSerializer();
            using var sr = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(sr);
            return serializer.Deserialize<T>(jsonTextReader);
        }
    }
}
