using BookProject.Utilities;
using SixLabors.ImageSharp;
using System;
using System.IO;
using Xunit;

namespace BookProject.Tests.Utilities
{
    public class ImageResizerTests
    {
        [Fact]
        public void ResizeImage_ShouldResizeAndSaveImage()
        {
            var inputPath = "input.jpg";
            var outputPath = "output.jpg";

            using (var image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(100, 100))
            {
                image.Save(inputPath);
            }

            ImageResizer.ResizeImage(inputPath, outputPath, 50, 50);

            using (var resizedImage = Image.Load(outputPath))
            {
                Assert.Equal(50, resizedImage.Width);
                Assert.Equal(50, resizedImage.Height);
            }

            File.Delete(inputPath);
            File.Delete(outputPath);
        }

        [Fact]
        public void ResizeImage_ShouldThrowException_IfInputFileDoesNotExist()
        {
            var inputPath = "nonexistent.jpg";
            var outputPath = "output.jpg";

            var ex = Assert.Throws<FileNotFoundException>(() => ImageResizer.ResizeImage(inputPath, outputPath, 50, 50));
            Assert.Contains("nonexistent.jpg", ex.Message);
        }
    }
}
