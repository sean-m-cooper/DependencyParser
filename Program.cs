using DependencyParser;
using DependencyParser.Interfaces;
using System.Reflection;

static async Task Main(string[] args)
{
    string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestData");
    string packageJsonPath = Path.Combine(filePath, "Package_Test.json");
    string testProjectPath = Path.Combine(filePath, "Test_Project.xml");

    NpmDependencyParser npmParser = new();
    NugetDependencyParser nugetParser = new();

    List<IPackageInfoItem> packageInfoItems = new();
    packageInfoItems.AddRange(await npmParser.GetPackageInfosAsync(packageJsonPath));
    packageInfoItems.AddRange(await nugetParser.GetPackageInfosAsync(testProjectPath));

    await DependencyParserBase.WriteCSVFileAsync(@"c:\test\packages.csv", packageInfoItems);
}