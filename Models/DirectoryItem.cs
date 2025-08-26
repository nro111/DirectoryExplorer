namespace DirectoryExplorer.Models
{
    public class DirectoryItem
    {
        // File/Folder Name
        public string Name { get; set; } = string.Empty;
        // Relative path from the origin
        public string Type { get; set; } = string.Empty;
        // Relative path from origin
        public string Path { get; set; } = string.Empty;
        // File/Folder Size
        public string Size { get; set; } = string.Empty;
    }
}
