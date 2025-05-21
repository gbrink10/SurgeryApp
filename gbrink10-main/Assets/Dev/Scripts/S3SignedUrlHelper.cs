using Amazon.S3;
using Amazon.S3.Model;
using System;

public static class S3SignedUrlHelper
{
    public static string GeneratePreSignedURL(string bucket, string key, TimeSpan expiry, AmazonS3Client client)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiry),
            Verb = HttpVerb.GET
        };

        return client.GetPreSignedURL(request);
    }
}
