namespace Asm.Server.Helpers
{
	static public class ImageHelper
	{
		public static async Task<string?> UploadImageAsync(IWebHostEnvironment env, IFormFile? file, string folderName)
		{
			if (file == null) return null;

			var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
			if (!allowedTypes.Contains(file.ContentType))
				throw new ArgumentException("Chỉ chấp nhận file ảnh (jpg, png, gif).");

			var folder = Path.Combine(env.WebRootPath, folderName);
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);

			string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
			string filePath = Path.Combine(folder, fileName);

			using var stream = new FileStream(filePath, FileMode.Create);
			await file.CopyToAsync(stream);

			return $"/{folderName}/{fileName}";
		}

	}
}
