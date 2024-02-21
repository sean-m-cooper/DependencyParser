namespace DependencyParser
{
    public class FileWriter
    {
        public string FilePath { get; set; }
        public bool OverWriteIfFileExists { get; set; }

        public FileWriter(string filePath)
        {
            FilePath = filePath;
        }

        public async Task WriteFileAsync(string[] fileLines)
        {
            await File.WriteAllLinesAsync(FilePath, fileLines);
        }
    }
}
