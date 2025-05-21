using System.Collections.Generic;
using System;
using UnityEngine;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Threading.Tasks;
using System.Linq;

public class VideoListLoader : MonoBehaviour
{
    public List<VideoMeta> videos = new();

    private async void Awake()
    {
        videos = await LoadVideosFromS3();
    }

    private async Task<List<VideoMeta>> LoadVideosFromS3()
    {
        // 🕒 Wait until user has logged in and AWS credentials are available
        int waitMs = 0;
        while (AWSCredentialHolder.Credentials == null && waitMs < 10000)
        {
            await Task.Delay(200);
            waitMs += 200;
        }

        if (AWSCredentialHolder.Credentials == null)
        {
            Debug.LogError("❌ AWS credentials not available. Make sure user is logged in first.");
            return new List<VideoMeta>();
        }

        Debug.Log("[VideoListLoader] Using AWS credentials: " + AWSCredentialHolder.Credentials?.GetType().Name);

        var s3Client = new AmazonS3Client(AWSCredentialHolder.Credentials, RegionEndpoint.EUNorth1);

        var request = new ListObjectsV2Request
        {
            BucketName = "vr-surgery-app-videos"
        };

        var response = await s3Client.ListObjectsV2Async(request);
        var result = new List<VideoMeta>();

        foreach (var s3Object in response.S3Objects)
        {
            if (!s3Object.Key.EndsWith(".mp4"))
                continue;

            var metadataRequest = new GetObjectMetadataRequest
            {
                BucketName = "vr-surgery-app-videos",
                Key = s3Object.Key
            };

            var metadata = await s3Client.GetObjectMetadataAsync(metadataRequest);

            string GetMeta(string key) =>
                metadata.Metadata.Keys.Contains(key) ? metadata.Metadata[key] : null;

            string title = GetMeta("x-amz-meta-title") ?? s3Object.Key;
            string uploader = GetMeta("x-amz-meta-uploader") ?? "Unknown";
            string videoTypeStr = GetMeta("x-amz-meta-videotype");
            string stereoFormatStr = GetMeta("x-amz-meta-stereoformat");

            // 🌟 Proper string-to-enum parsing (case-insensitive)
            Enum.TryParse(videoTypeStr, true, out VideoType videoType);
            Enum.TryParse(stereoFormatStr, true, out StereoFormat stereoFormat);

            Debug.Log($"Metadata for {s3Object.Key} => Type: {videoTypeStr}, Format: {stereoFormatStr}");

            var video = new VideoMeta
            {
                title = title,
                uploader = uploader,
                videoUrl = s3Object.Key, // Just the object path/key
                videoType = videoType,
                stereoFormat = stereoFormat
            };

            result.Add(video);
        }

        return result;
    }

    /*
    // Legacy JSON loader, keep as backup
    // public List<VideoMeta> videos;
    // void Awake()
    // {
    //     TextAsset jsonFile = Resources.Load<TextAsset>("video_list");
    //     string wrappedJson = "{\"items\":" + jsonFile.text + "}";
    //     videos = JsonUtility.FromJson<VideoMetaList>(wrappedJson).items;
    // }
    */
}
