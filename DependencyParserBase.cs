using DependencyParser.Interfaces;
namespace DependencyParser
{
    public abstract class DependencyParserBase
    {
        public abstract Task<List<IPackageInfo>> GetPackageInfosAsync(string filePath);
        public abstract Task<Version> GetMaxVersionValueAsync(IPackageInfo currentPackageInfo);

        /// <summary>
        /// Writes a csv file to the specified path
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="NotImplementedException"></exception>
        public Task WriteCSVFileAsync(string filePath)
        {
            throw new NotImplementedException();
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
                version = PackageInfo.DEFAULT_VERSION_NUMBER;
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


    }
}
