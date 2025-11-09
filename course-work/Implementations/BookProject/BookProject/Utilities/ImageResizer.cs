namespace BookProject.Utilities
{
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;
    using System.IO;

    public static class ImageResizer
    {
        public static void ResizeImage(string inputPath, string outputPath, int width, int height)
        {
            using (Image image = Image.Load(inputPath))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(width, height),
                    Mode = ResizeMode.Crop
                }));

                image.Save(outputPath);
            }
        }
    }
}