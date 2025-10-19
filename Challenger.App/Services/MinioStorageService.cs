using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Challenger.App.Services
{
    public class MinioStorageService : IStorageService
    {
        private readonly IMinioClient _client;
        private readonly string _bucket;

        public MinioStorageService(IConfiguration config)
        {
            var endpoint = config["Storage:Minio:Endpoint"] ?? "localhost:9000";
            var accessKey = config["Storage:Minio:AccessKey"] ?? "minioadmin";
            var secretKey = config["Storage:Minio:SecretKey"] ?? "minioadmin";
            var withSSL = bool.TryParse(config["Storage:Minio:WithSSL"], out var ssl) && ssl;
            _bucket = config["Storage:Minio:Bucket"] ?? "challenger";

            _client = new MinioClient().WithEndpoint(endpoint)
                                       .WithCredentials(accessKey, secretKey)
                                       .WithSSL(withSSL)
                                       .Build();
        }

        public async Task<string> SaveAsync(Stream content, string contentType, string folder, string fileName)
        {
            var beArgs = new BucketExistsArgs().WithBucket(_bucket);
            bool found = await _client.BucketExistsAsync(beArgs);
            if (!found)
            {
                await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucket));
            }

            var ext = Path.GetExtension(fileName);
            var objectName = $"{folder.Trim('/')}/{Guid.NewGuid():N}{ext}";

            await _client.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_bucket)
                .WithObject(objectName)
                .WithStreamData(content)
                .WithObjectSize(content.Length)
                .WithContentType(contentType));

            return $"{_bucket}/{objectName}";
        }
    }
}
