using BookProject.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BookProject.Tests.Utilities
{
    public class FileServiceTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly FileService _fileService;

        public FileServiceTests()
        {
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(env => env.WebRootPath).Returns("wwwroot");
            _fileService = new FileService(_mockEnvironment.Object);
        }

        [Fact]
        public void DeleteFile_ShouldThrowFileNotFoundException_IfFileDoesNotExist()
        {
            var fileName = "nonexistent.jpg";
            var ex = Assert.Throws<FileNotFoundException>(() => _fileService.DeleteFile(fileName));
            Assert.NotEqual(fileName, ex.FileName);
        }


        [Fact]
        public async Task SaveFile_ShouldThrowInvalidOperationException_IfExtensionIsNotAllowed()
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.exe");
            var allowedExtensions = new[] { ".jpg", ".png" };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _fileService.SaveFile(fileMock.Object, allowedExtensions));
            Assert.Contains("Only .jpg,.png files allowed", ex.Message);
        }

        [Fact]
        public async Task SaveFile_ShouldSaveFile_IfExtensionIsAllowed()
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.jpg");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                    .Returns((Stream stream, System.Threading.CancellationToken _) =>
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write("dummy content");
                        }
                        return Task.CompletedTask;
                    });

            var allowedExtensions = new[] { ".jpg", ".png" };
            var result = await _fileService.SaveFile(fileMock.Object, allowedExtensions);

            Assert.StartsWith("resized_", result);
            Assert.EndsWith(".jpg", result);
        }
    }
}
