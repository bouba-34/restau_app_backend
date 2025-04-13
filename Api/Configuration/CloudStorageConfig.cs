namespace backend.Api.Configuration
{
    public class CloudStorageConfig
    {
        public string Endpoint { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }
        public bool UseSSL { get; set; }
        public string PublicEndpoint { get; set; }
        public bool UseLocalStorage { get; set; } = false;
        public string LocalStoragePath { get; set; } = "wwwroot/images";
        public string Region { get; set; } = "us-east-1"; // Default region
    }
}