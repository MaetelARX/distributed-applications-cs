using BookProject.Utilities.Interfaces;

namespace BookProject.Utilities
{
	using SixLabors.ImageSharp;
	using SixLabors.ImageSharp.Processing;
	using System.IO;

	public class FileService : IFileService
	{
		private readonly IWebHostEnvironment _environment;
		public FileService(IWebHostEnvironment environment)
		{
			_environment = environment;
		}

		public void DeleteFile(string fileName)
		{
			var wwwPath = _environment.WebRootPath;
			var fileNameWithPath = Path.Combine(wwwPath, "images\", fileName");

			if (!File.Exists(fileNameWithPath))
			{
				throw new FileNotFoundException(fileName);
			}
			File.Delete(fileNameWithPath);
		}

		public async Task<string> SaveFile(IFormFile file, string[] allowedExtensions)
		{
			var wwwPath = _environment.WebRootPath;
			var path = Path.Combine(wwwPath, "images/books");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			var extension = Path.GetExtension(file.FileName);
			if (!allowedExtensions.Contains(extension))
			{
				throw new InvalidOperationException($"Only {string.Join(",", allowedExtensions)} files allowed");
			}

			string originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
			string fileName = $"resized_{originalFileName}{extension}";
			string fileNameWithPath = Path.Combine(path, fileName);
			using var stream = new FileStream(fileNameWithPath, FileMode.Create);
			await file.CopyToAsync(stream);
			return fileName;
		}
	}
}
