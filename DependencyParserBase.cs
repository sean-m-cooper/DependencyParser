using DependencyParser.Interfaces;

namespace DependencyParser
{

    public abstract class DependencyParserBase
    {
        /// <summary>
        /// Writes a csv file to the specified path
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task WriteCSVFileAsync(string filePath, List<IPackageInfoItem> packageInfoItems)
        {
            if (packageInfoItems is null || !packageInfoItems.Any())
            {
                throw new ArgumentNullException("No package info provided", nameof(packageInfoItems));
            }
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(filePath, nameof(filePath));
            }

            var fileLines = packageInfoItems.Select(p => p.ToString()).ToArray();
            var writer = new FileWriter(filePath);
            await writer.WriteFileAsync(fileLines);
        }

        /// <summary>
        /// Replaces extra \ and single " with blanks
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string HygieneString(string input)
        {
            input = input.Replace("\"", "").Replace("\\", "");
            return input;
        }

        /// <summary>
        /// Returns a string that can be parsed by the <see cref="Version"/> class
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public string HygieneVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                version = PackageInfoItem.DEFAULT_VERSION_NUMBER;
            }
            version = HygieneString(version);

            if (version.Contains('-'))
            {
                var dashIndex = version.IndexOf("-");
                version = version[..dashIndex];
            }

            var vals = version.Split(".").ToList();
            while (vals.Count < 3)
            {
                vals.Add("0");
                version = String.Join(".", vals);
            }
            return version;

        }

        protected async Task<Stream?> DownloadStreamFromUrlAsync(string url, HttpClient client)
        {
            using (client)
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


    }
}
