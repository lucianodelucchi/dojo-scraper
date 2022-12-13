using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class AssetsManager
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AssetsManager> _logger;

        private string _imagesFolder { get; }

        private string DefaultDirectory
            => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures, Environment.SpecialFolderOption.Create);

        public AssetsManager(IConfiguration configuration, ILogger<AssetsManager> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _imagesFolder = _configuration["downloadFolder"] ?? Path.Combine(DefaultDirectory, "DojoScraper", "images");
        }

        internal void EnsureFolderExists()
        {
            _logger.LogDebug($"Creating {_imagesFolder} if not exists.");
            Directory.CreateDirectory(_imagesFolder);
        }

        internal async Task<string> SaveImageAsync(string filename, Stream imageStream, CancellationToken token = default)
        {
            var imagePath = Path.Combine(_imagesFolder, filename);

            using var newImage = File.Create(imagePath);

            await imageStream.CopyToAsync(newImage, token);

            return newImage.Name;
        }

        internal async Task<string> SaveImageAsync(string name, Stream imageStream, DateTime createdAt, CancellationToken token = default)
        {
            var imagePath = await SaveImageAsync(name, imageStream, token);
            
            File.SetCreationTimeUtc(imagePath, createdAt);
            
            return imagePath;
        }

        internal bool ImageExists(string filename)
        {
            var imagePath = Path.Combine(_imagesFolder, filename);

            return File.Exists(imagePath);
        }

        internal void ClearImages()
        {
            Directory.Delete(_imagesFolder, true);
            //foreach (var file in Directory.GetFiles(_imagesFolder))
            //{
            //    File.Delete(file);
            //}
        }
    }
}