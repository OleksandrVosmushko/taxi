﻿using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Taxi.Models.Drivers;

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

        public async Task PutObjectToStorage(string key, string filePath)
        {
            try
            {
                // 1. Put object-specify only key name for the new object.
                var fileTransferUtility =
                    new TransferUtility(_s3);

                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucketName,
                    FilePath = filePath,
                    Key = key
                };
                await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);

                //     PutObjectResponse response1 = await _s3.PutObjectAsync(putRequest1);
                // 2. Put the object-set ContentType and add metadata.
                //var putRequest = new PutObjectRequest
                //{
                //    BucketName = bucketName,
                //    Key = key,
                //    FilePath = filePath,
                //};
                //PutObjectResponse response = await _s3.PutObjectAsync(putRequest);

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

        public async Task<FileDto> GetObjectAsync(string key)
        {
    
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = key
                };
                MemoryStream file = new MemoryStream();
                using (GetObjectResponse response = await _s3.GetObjectAsync(request))
                {
                    try
                    {
                        BufferedStream stream2 = new BufferedStream(response.ResponseStream);
                        byte[] buffer = new byte[0x2000];
                        int count = 0;
                        while ((count = stream2.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            file.Write(buffer, 0, count);
                        }
                    }
                    finally
                    {
                    }
                    string contentType = response.Headers["Content-Type"];
                    return new FileDto
                    {
                        ContentType = contentType,
                        Stream = file
                    }; 
                    
                }
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered ***. Message:'{0}' when writing an object", e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
                return null;
            }
        }
    }
}
