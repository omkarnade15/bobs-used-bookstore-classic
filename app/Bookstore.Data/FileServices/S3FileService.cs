using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Bookstore.Domain;
using BobsBookstoreClassic.Data;

namespace Bookstore.Data.FileServices
{
    public class S3FileService : IFileService
    {
        private readonly TransferUtility _transferUtility;

        public S3FileService(IAmazonS3 s3Client)
        {
            _transferUtility = new TransferUtility(s3Client);
        }

        public async Task DeleteAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;

            var bucketName = BookstoreConfiguration.GetSetting("Files/BucketName");
            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = Path.GetFileName(filePath)
            };

            await _transferUtility.S3Client.DeleteObjectAsync(request);
        }

        public async Task<string?> SaveAsync(Stream contents, string filename)
        {
            if (contents == null) return null;

            var bucketName = BookstoreConfiguration.GetSetting("Files/BucketName");
            var uniqueFilename = $"{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}{Path.GetExtension(filename)}";
            var cloudFrontDomain = BookstoreConfiguration.GetSetting("Files/CloudFrontDomain");

            var request = new TransferUtilityUploadRequest
            {
                BucketName = bucketName,
                InputStream = contents,
                Key = uniqueFilename
            };

            await _transferUtility.UploadAsync(request);

            return $"{cloudFrontDomain}/{uniqueFilename}";
        }
    }
}
