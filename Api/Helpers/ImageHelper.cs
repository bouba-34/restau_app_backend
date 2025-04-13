using backend.Api.Configuration;
using Microsoft.AspNetCore.Http;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace backend.Api.Helpers
{
    public class ImageHelper
    {
        private readonly CloudStorageConfig _storageConfig;
        private readonly IWebHostEnvironment _environment;
        private readonly IMinioClient _minioClient;

        public ImageHelper(CloudStorageConfig storageConfig, IWebHostEnvironment environment)
        {
            _storageConfig = storageConfig;
            _environment = environment;
            
            if (!_storageConfig.UseLocalStorage)
            {
                // Initialize MinIO client with the latest API
                _minioClient = new MinioClient()
                    .WithEndpoint(_storageConfig.Endpoint)
                    .WithCredentials(_storageConfig.AccessKey, _storageConfig.SecretKey)
                    .WithRegion(_storageConfig.Region)
                    .WithSSL(_storageConfig.UseSSL)
                    .Build();
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder = "general")
        {
            if (file == null || file.Length == 0)
                return null;
                
            // Check if the file is an image
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".gif")
                throw new ArgumentException("Invalid image format");
                
            if (_storageConfig.UseLocalStorage)
            {
                return await SaveToLocalStorageAsync(file, folder);
            }

            return await SaveToMinioAsync(file, folder);
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;
                
            if (_storageConfig.UseLocalStorage)
            {
                return DeleteFromLocalStorage(imageUrl);
            }
            else
            {
                return await DeleteFromMinioAsync(imageUrl);
            }
        }

        private async Task<string> SaveToLocalStorageAsync(IFormFile file, string folder)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", folder);
            
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);
                
            // Create a unique file name
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            
            return $"/images/{folder}/{uniqueFileName}";
        }

        private bool DeleteFromLocalStorage(string imageUrl)
        {
            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> SaveToMinioAsync(IFormFile file, string folder)
        {
            // Create a unique file name
            var uniqueFileName = $"{folder}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            
            try
            {
                Console.WriteLine($"MinIO upload: Connecting to {_storageConfig.Endpoint} with bucket {_storageConfig.BucketName}");
                
                // Check if bucket exists, if not create it
                bool bucketExists = await _minioClient.BucketExistsAsync(
                    new BucketExistsArgs().WithBucket(_storageConfig.BucketName)
                ).ConfigureAwait(false);
                
                Console.WriteLine($"MinIO upload: Bucket exists? {bucketExists}");
                
                if (!bucketExists)
                {
                    Console.WriteLine($"MinIO upload: Creating bucket {_storageConfig.BucketName}");
                    await _minioClient.MakeBucketAsync(
                        new MakeBucketArgs().WithBucket(_storageConfig.BucketName)
                    ).ConfigureAwait(false);
                    
                    // Set bucket policy to public (optional - if you want images to be publicly accessible)
                    var policy = $@"{{
                        ""Version"": ""2012-10-17"",
                        ""Statement"": [
                            {{
                                ""Effect"": ""Allow"",
                                ""Principal"": {{""AWS"": [""*""]}},
                                ""Action"": [""s3:GetObject""],
                                ""Resource"": [""arn:aws:s3:::{_storageConfig.BucketName}/*""]
                            }}
                        ]
                    }}";
                    
                    Console.WriteLine("MinIO upload: Setting bucket policy to public");
                    
                    await _minioClient.SetPolicyAsync(
                        new SetPolicyArgs().WithBucket(_storageConfig.BucketName).WithPolicy(policy)
                    ).ConfigureAwait(false);
                }
                
                // Upload file to MinIO
                Console.WriteLine($"MinIO upload: Uploading file {uniqueFileName} with size {file.Length} bytes");
                using (var stream = file.OpenReadStream())
                {
                    var putObjectArgs = new PutObjectArgs()
                        .WithBucket(_storageConfig.BucketName)
                        .WithObject(uniqueFileName)
                        .WithStreamData(stream)
                        .WithObjectSize(file.Length)
                        .WithContentType(file.ContentType);
                        
                    await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                }
                
                // Construct and return URL
                string scheme = _storageConfig.UseSSL ? "https" : "http";
                string endpoint = string.IsNullOrEmpty(_storageConfig.PublicEndpoint) 
                    ? _storageConfig.Endpoint
                    : _storageConfig.PublicEndpoint;
                var url = $"{scheme}://{endpoint}/{_storageConfig.BucketName}/{uniqueFileName}";
                Console.WriteLine($"MinIO upload: Successfully uploaded. URL: {url}");
                return url;
            }
            catch (MinioException ex)
            {
                Console.WriteLine($"MinIO upload error: {ex.Message}");
                Console.WriteLine($"MinIO upload error details: {ex}");
                throw new Exception($"Error uploading to MinIO: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error during upload: {ex.Message}");
                Console.WriteLine($"Error details: {ex}");
                throw;
            }
        }

        private async Task<bool> DeleteFromMinioAsync(string imageUrl)
        {
            try
            {
                // Extract object name from URL
                Uri uri = new Uri(imageUrl);
                string pathAndQuery = uri.PathAndQuery;
                string[] segments = pathAndQuery.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                
                // The first segment should be the bucket name, remaining segments form the object name
                if (segments.Length <= 1)
                    return false;
                
                string objectName = string.Join("/", segments.Skip(1));
                
                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(_storageConfig.BucketName)
                    .WithObject(objectName);
                    
                await _minioClient.RemoveObjectAsync(removeObjectArgs).ConfigureAwait(false);
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}