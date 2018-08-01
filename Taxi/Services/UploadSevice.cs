using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Taxi.Services
{
    public class UploadSevice : IUploadService
    {
        private IAmazonS3 _s3;

        private const string bucketName = "taxi-storage-v1";

        public UploadSevice(IAmazonS3 amazonS3)
        {
            _s3 = amazonS3;
        }

        public async Task PutObjectToStorage(string key)
        {
            try
            {
                // 1. Put object-specify only key name for the new object.
                var putRequest1 = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    ContentBody = "sample text"
                };

                PutObjectResponse response1 = await _s3.PutObjectAsync(putRequest1);

                // 2. Put the object-set ContentType and add metadata.
                //var putRequest2 = new PutObjectRequest
                //{
                //    BucketName = bucketName,
                //    Key = keyName2,
                //    FilePath = filePath,
                //    ContentType = "text/plain"
                //};
                //putRequest2.Metadata.Add("x-amz-meta-title", "someTitle");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                        "Error encountered ***. Message:'{0}' when writing an object"
                        , e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown encountered on server. Message:'{0}' when writing an object"
                    , e.Message);
            }
        }
    }
}
