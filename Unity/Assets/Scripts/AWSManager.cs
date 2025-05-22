using UnityEngine;
using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using System.Collections.Generic;

public class AWSManager : MonoBehaviour
{
    private static AWSManager instance;
    private AmazonS3Client s3Client;
    private const string bucketName = "vr-surgery-app-videos";
    private const string region = "eu-west-1";

    public static AWSManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("AWSManager");
                instance = go.AddComponent<AWSManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAWS();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAWS()
    {
        var credentials = new BasicAWSCredentials(
            "AKIA3Y4S6JNGBPSQRIVP",
            "N4y/4G4PGgNeNXdnHXrzOP7mIHaoq4e/nBcQcCYW"
        );

        var config = new AmazonS3Config
        {
            RegionEndpoint = RegionEndpoint.EUWest1
        };

        s3Client = new AmazonS3Client(credentials, config);
    }

    public async Task<List<VideoInfo>> ListVideos()
    {
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = "videos/"
            };

            var response = await s3Client.ListObjectsV2Async(request);
            var videos = new List<VideoInfo>();

            foreach (var obj in response.S3Objects)
            {
                videos.Add(new VideoInfo
                {
                    Key = obj.Key,
                    LastModified = obj.LastModified,
                    Size = obj.Size,
                    Url = $"https://s3.{region}.amazonaws.com/{bucketName}/{obj.Key}"
                });
            }

            return videos;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error listing videos: {e.Message}");
            return new List<VideoInfo>();
        }
    }

    public async Task<string> GetVideoUrl(string key)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddHours(1)
            };

            return s3Client.GetPreSignedURL(request);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error getting video URL: {e.Message}");
            return null;
        }
    }
}

[System.Serializable]
public class VideoInfo
{
    public string Key;
    public DateTime LastModified;
    public long Size;
    public string Url;
}
