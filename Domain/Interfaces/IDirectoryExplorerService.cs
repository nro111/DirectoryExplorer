using DirectoryExplorer.Models;

namespace DirectoryExplorer.Domain.Interfaces
{
    public interface IDirectoryExplorerService
    {
        List<DirectoryItem> Browse(string path);
        List<DirectoryItem> Search(string query);
        string GetHomeDirectory();
        bool SetHomeDirectory(string path);
        bool CreateFolder(string relativePath);
        bool DeleteFile(string relativePath);
        bool DeleteFolder(string relativePath);
        byte[]? Download(string relativePath, out string fileName);
        bool Upload(string relativePath, IFormFile file);
    }
}