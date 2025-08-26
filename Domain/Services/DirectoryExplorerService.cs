using DirectoryExplorer.Domain.Interfaces;
using DirectoryExplorer.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.IO;

namespace DirectoryExplorer.Domain.Services
{
    public class DirectoryExplorerService : IDirectoryExplorerService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly IOptionsMonitor<DirectoryExplorerOptions> _options;
        private readonly string _root;

        public DirectoryExplorerService(
            ILogger<DirectoryExplorerService> logger, 
            IConfiguration config,
            IOptionsMonitor<DirectoryExplorerOptions> options)
        {
            _logger = logger;
            _config = config;
            _options = options;
            _root = options.CurrentValue.HomeDirectory;
        }

        private string GetRootedPath(string relativePath)
        {
            var fullPath = Path.GetFullPath(Path.Combine(_root, relativePath ?? ""));

            // Prevent 
            //if (!fullPath.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
            //    throw new UnauthorizedAccessException("Path outside root not allowed");

            return fullPath;
        }

        public List<DirectoryItem> Browse(string path)
        {
            try
            {
                var fullPath = GetRootedPath(path);
                var entries = Directory.GetFileSystemEntries(fullPath)
                                       .Select(p => new DirectoryItem
                                       {
                                           Name = Path.GetFileName(p),
                                           Path = Path.Combine(path ?? "", Path.GetFileName(p)),
                                           Type = File.GetAttributes(p).HasFlag(FileAttributes.Directory) ? "Folder" : "File",
                                           Size = File.GetAttributes(p).HasFlag(FileAttributes.Directory) ? "0" : new FileInfo(p).Length.ToString()
                                       })
                                       .ToList();
                return entries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Browse for {Path}", path);
                return new List<DirectoryItem>();
            }
        }

        public List<DirectoryItem> Search(string query)
        {
            var results = new List<DirectoryItem>();
            try
            {
                var root = GetHomeDirectory();

                // Fail fast if not found or query is null
                if (string.IsNullOrEmpty(root) || !Directory.Exists(root) || string.IsNullOrEmpty(query))
                    return results;

                // Grab all the directories that fully/partly match the query
                var directories = Directory.GetDirectories(root, "*", SearchOption.AllDirectories)
                    .Where(d => Path.GetFileName(d).Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Select(d => new DirectoryItem
                    {
                        Name = Path.GetFileName(d),
                        Type = "Folder",
                        Path = Path.GetRelativePath(root, d)
                    });

                // Grab all the files that fully/partly match the query
                var files = Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                    .Where(f => Path.GetFileName(f).Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Select(f => new DirectoryItem
                    {
                        Name = Path.GetFileName(f),
                        Type = "File",
                        Path = Path.GetRelativePath(root, f)
                    });

                results = directories.Concat(files).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for {Query}", query);
            }

            return results;
        }

        public string GetHomeDirectory() => _options.CurrentValue.HomeDirectory;

        public bool SetHomeDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    return false;

                _options.CurrentValue.HomeDirectory = path;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting home directory {Path}", path);
                return false;
            }
        }

        public bool Upload(string relativePath, IFormFile file)
        {
            try
            {
                // Treat 'relativePath' as a folder inside Origin
                var directoryPath = GetRootedPath(relativePath);

                if (!Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                var safeName = Path.GetFileName(file.FileName);
                var fullPath = Path.Combine(directoryPath, safeName);

                using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
                file.CopyTo(stream);
                return true;
            }
            //catch (UnauthorizedAccessException uae)
            //{
            //    _logger.LogError(uae, "No write permission for {Path}", relativePath);
            //    return false;
            //}
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Upload {Path}", relativePath);
                return false;
            }
        }

        public bool CreateFolder(string relativePath)
        {
            try
            {
                var fullPath = GetRootedPath(relativePath);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateFolder {Path}", relativePath);
                return false;
            }
        }

        public bool DeleteFile(string relativePath)
        {
            try
            {
                var fullPath = GetRootedPath(relativePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteFile {Path}", relativePath);
                return false;
            }
        }

        public bool DeleteFolder(string relativePath)
        {
            try
            {
                var fullPath = GetRootedPath(relativePath);
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, true);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteFolder {Path}", relativePath);
                return false;
            }
        }

        public byte[]? Download(string relativePath, out string fileName)
        {
            fileName = Path.GetFileName(relativePath);

            try
            {
                var fullPath = GetRootedPath(relativePath);
                if (File.Exists(fullPath))
                {
                    return File.ReadAllBytes(fullPath);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Download {Path}", relativePath);
                return null;
            }
        }
    }
}
