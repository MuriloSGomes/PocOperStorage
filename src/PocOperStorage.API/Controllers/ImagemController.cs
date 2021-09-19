using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using PocOperStorage.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PocOperStorage.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImagemController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ImagemController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Save(IFormFile file, [FromForm] Imagem command)
        {
            var imageUrl = await Upload(file);
            command.UrlImagem = imageUrl;
            return Ok(command);
        }

        private async Task<string> Upload(IFormFile file)
        {
            var accountName = _configuration["StorageConfiguration:AccountName"];
            var accountKey = _configuration["StorageConfiguration:AccountKey"];
            var containerNames = _configuration["StorageConfiguration:ContainerName"];

            var storageCredentials = new StorageCredentials(accountName, accountKey);
            var storageAccount = new CloudStorageAccount(storageCredentials, true);
            var blobAzure = storageAccount.CreateCloudBlobClient();
            var container = blobAzure.GetContainerReference(containerNames);

            var blob = container.GetBlockBlobReference(file.FileName);
            blob.Properties.ContentType = file.ContentType;
            await blob.UploadFromStreamAsync(file.OpenReadStream());

            return blob.SnapshotQualifiedStorageUri.PrimaryUri.ToString();
        }
    }
}
