
using DependencyParser;
using DependencyParser.Interfaces;
using System.Reflection;

static async Task Main(string[] args)
{
    string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestData");
    string packageJsonPath = Path.Combine(filePath, "Package_Test.json");
    string testProjectPath = Path.Combine(filePath, "Test_Project.xml");

    NpmDependencyParser npmParser = new NpmDependencyParser();
    NugetDependencyParser nugetParser = new NugetDependencyParser();

    List<IPackageInfoItem> packages = new List<IPackageInfoItem>();
    packages.AddRange(await npmParser.GetPackageInfosAsync(packageJsonPath));
    packages.AddRange(await nugetParser.GetPackageInfosAsync(testProjectPath));

    await DependencyParserBase.WriteCSVFileAsync("", packages);
}