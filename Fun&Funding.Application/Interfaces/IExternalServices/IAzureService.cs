using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Application.Interfaces.IExternalServices
{
    public interface IAzureService
    {
        public Task<List<BlobContentInfo>> UploadBlobFiles(List<IFormFile> files);
        public Task<List<BlobContentInfo>> UploadFiles(List<IFormFile> files);
        public Task<List<string>> UploadUrlBlobFiles(List<IFormFile> files);
        public Task<string> UploadUrlSingleFiles(IFormFile file);

        public Task<List<BlobItem>> GetUploadedItems();
    }
}
