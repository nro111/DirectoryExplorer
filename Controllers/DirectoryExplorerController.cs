using DirectoryExplorer.Domain.Interfaces;
using DirectoryExplorer.Models;
using Microsoft.AspNetCore.Mvc;

namespace TestProject.Controllers
{
    [ApiController]
    [Route("[controller]/api")]
    public class FileSystemController : ControllerBase
    {
        private readonly IDirectoryExplorerService _directoryExplorerService;
        private readonly ILogger<FileSystemController> _logger;

        public FileSystemController(
            IDirectoryExplorerService directoryExplorerService,
            ILogger<FileSystemController> logger)
        {
            _directoryExplorerService = directoryExplorerService;
            _logger = logger;
        }

        #region Get
        [HttpGet("browse")]
        [ProducesResponseType(typeof(IEnumerable<DirectoryItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public IActionResult Browse([FromQuery]string path)
        {
            try
            {
                var result = _directoryExplorerService.Browse(path);
                if(result == null)
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new { error = "Unable to browse at this time." });

                return Ok(new { items = result, count = result.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Browse");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Unable to browse at this time." });
            }
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<DirectoryItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public IActionResult Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { error = "Search query required." });

            try
            {
                var result = _directoryExplorerService.Search(query);
                if (result == null || !result.Any())
                    return NotFound(new { error = "No results found." });

                return Ok(new { items = result, count = result.Count, query });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Search");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Unable to search at this time." });
            }
        }

        [HttpGet("home")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult GetHomeDirectory()
        {
            return Ok(new { home = _directoryExplorerService.GetHomeDirectory() });
        }

        [HttpGet("download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Download([FromQuery] string path)
        {
            if (string.IsNullOrEmpty(path))
                return BadRequest(new { error = "Path is required." });

            try
            {
                var fileBytes = _directoryExplorerService.Download(path, out string fileName);

                if (fileBytes == null || fileBytes.Length == 0)
                    return NotFound(new { error = "File not found." });

                return File(fileBytes, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Download");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Unable to download file." });
            }
        }
        #endregion

        #region Create

        [HttpPost("upload")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public IActionResult Upload([FromForm] string path, IFormFile file)
        {
            try
            {
                var result = _directoryExplorerService.Upload(path, file);
                if (!result)
                    return BadRequest(new { error = "File failed to upload." });

                return Ok(new { message = "File uploaded successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Upload");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "File failed to upload." });
            }
        }

        [HttpPost("create/folder/{relativePath}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public IActionResult CreateFolder(string relativePath)
        {
            try
            {
                var result = _directoryExplorerService.CreateFolder(relativePath);
                if (!result)
                    return BadRequest(new { error = "Unable to create folder." });

                return Ok(new { message = "Folder created successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateFolder");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Unable to create folder." });
            }
        }

        [HttpPost("home")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public IActionResult SetHomeDirectory([FromBody] string path)
        {
            var success = _directoryExplorerService.SetHomeDirectory(path);
            if (!success)
                return BadRequest(new { error = "Invalid path." });

            return Ok(new { message = "Home directory updated.", path });
        }

        #endregion

        #region Delete

        [HttpDelete("delete/file/{path}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteFile(string path)
        {
            try
            {
                var result = _directoryExplorerService.DeleteFile(path);
                if (!result)
                    return BadRequest(new { error = "Unable to delete file." });

                return Ok(new { message = "File deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteFile");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Unable to delete file." });
            }
        }

        [HttpDelete("delete/folder/{path}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteFolder(string path)
        {
            try
            {
                var result = _directoryExplorerService.DeleteFolder(path);
                if (!result)
                    return BadRequest(new { error = "Unable to delete folder." });

                return Ok(new { message = "Folder deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteFolder");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "Unable to delete folder." });
            }
        }

        #endregion
    }
}