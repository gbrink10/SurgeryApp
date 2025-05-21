using UnityEngine;
using Evereal.VRVideoPlayer;
using System.Collections;
using Amazon.S3;
using Amazon;
using System;
using System.IO;
using UnityEngine.Networking;
using TMPro;

public class VideoPlaybackManager : MonoBehaviour
{
    public VRVideoPlayer vrPlayer;
    public TextMeshProUGUI progressText; // Optional — assign in inspector

    void Start()
    {
        vrPlayer.SetSource(Evereal.VRVideoPlayer.VideoSource.ABSOLUTE_URL);
        vrPlayer.SetRenderMode(Evereal.VRVideoPlayer.RenderMode.NORMAL);
        vrPlayer.SetStereoMode(Evereal.VRVideoPlayer.StereoMode.LEFT_RIGHT);
       // vrPlayer.SetStereoMode(VideoSession.is3D ? Evereal.VRVideoPlayer.StereoMode.LEFT_RIGHT : Evereal.VRVideoPlayer.StereoMode.NONE);
    }

    // === STREAM & PLAY ===
    public void PlayVideo(string s3Key)
    {
        Debug.Log("🔐 Creating signed URL...");
        var client = new AmazonS3Client(AWSCredentialHolder.Credentials, RegionEndpoint.EUNorth1);
        string signedUrl = S3SignedUrlHelper.GeneratePreSignedURL(
            "vr-surgery-app-videos", s3Key, TimeSpan.FromMinutes(15), client
        );

        Debug.Log("🎬 Signed URL ready: " + signedUrl);
        StartCoroutine(PlayPrepared(signedUrl));
    }

    // === BASIC DOWNLOAD & PLAY ===
    public void DownloadAndPlay(string s3Key)
    {
        StartCoroutine(DownloadThenPlay(s3Key));
    }

    private IEnumerator DownloadThenPlay(string s3Key)
    {
        string localPath = GetLocalPathFor(s3Key);

        if (!File.Exists(localPath))
        {
            var client = new AmazonS3Client(AWSCredentialHolder.Credentials, RegionEndpoint.EUNorth1);
            string signedUrl = S3SignedUrlHelper.GeneratePreSignedURL(
                "vr-surgery-app-videos", s3Key, TimeSpan.FromMinutes(15), client
            );

            UnityWebRequest request = UnityWebRequest.Get(signedUrl);
            request.SendWebRequest();

            while (!request.isDone)
            {
                if (progressText != null)
                    progressText.text = $"Downloading... {(request.downloadProgress * 100f):0.0}%";
                yield return null;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Download failed: " + request.error);
                if (progressText != null)
                    progressText.text = "Download failed!";
                yield break;
            }

            File.WriteAllBytes(localPath, request.downloadHandler.data);
            Debug.Log("✅ Video saved: " + localPath);

            if (progressText != null)
            {
                progressText.text = "Downloaded ✔";
                yield return new WaitForSeconds(2f);
                progressText.text = "";
            }
        }

        StartCoroutine(PlayPrepared(localPath));
    }

    // === DOWNLOAD & PLAY WITH CALLBACKS ===
    public void DownloadAndPlay(string s3Key, Action<float> onProgress, Action onComplete)
    {
        StartCoroutine(DownloadThenPlayWithCallbacks(s3Key, onProgress, onComplete));
    }

    private IEnumerator DownloadThenPlayWithCallbacks(string s3Key, Action<float> onProgress, Action onComplete)
    {
        string localPath = GetLocalPathFor(s3Key);

        if (!File.Exists(localPath))
        {
            var client = new AmazonS3Client(AWSCredentialHolder.Credentials, RegionEndpoint.EUNorth1);
            string signedUrl = S3SignedUrlHelper.GeneratePreSignedURL(
                "vr-surgery-app-videos", s3Key, TimeSpan.FromMinutes(15), client
            );

            UnityWebRequest request = UnityWebRequest.Get(signedUrl);
            request.SendWebRequest();

            while (!request.isDone)
            {
                onProgress?.Invoke(request.downloadProgress * 100f);
                yield return null;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Download failed: " + request.error);
                onProgress?.Invoke(0);
                yield break;
            }

            File.WriteAllBytes(localPath, request.downloadHandler.data);
            Debug.Log("✅ Downloaded and saved: " + localPath);
        }

        onComplete?.Invoke();
        //StartCoroutine(PlayPrepared(localPath)); this will autoplay after download
    }

    // === PLAY FROM LOCAL ===
    public void PlayLocal(string s3Key)
    {
        string localPath = GetLocalPathFor(s3Key);
        if (File.Exists(localPath))
        {
            Debug.Log("▶️ Playing local: " + localPath);
            StartCoroutine(PlayPrepared(localPath));
        }
        else
        {
            Debug.LogWarning("⚠️ Local file not found: " + localPath);
            if (progressText != null)
                progressText.text = "Local file missing";
        }
    }

    // === DELETE LOCAL FILE ===
    public void DeleteLocalCopy(string s3Key)
    {
        string localPath = GetLocalPathFor(s3Key);
        if (File.Exists(localPath))
        {
            File.Delete(localPath);
            Debug.Log("🗑️ Deleted local file: " + localPath);
            if (progressText != null)
                progressText.text = "File deleted";
        }
        else
        {
            Debug.Log("⚠️ Nothing to delete");
            if (progressText != null)
                progressText.text = "Nothing to delete";
        }
    }

    // === CHECK LOCAL CACHE ===
    public bool IsVideoDownloaded(string s3Key)
    {
        return File.Exists(GetLocalPathFor(s3Key));
    }

    // === PLAY HELPER ===
    private IEnumerator PlayPrepared(string pathOrUrl)
    {
        vrPlayer.SetSource(Evereal.VRVideoPlayer.VideoSource.ABSOLUTE_URL);
        vrPlayer.Load(pathOrUrl, false);
        yield return new WaitUntil(() => vrPlayer.isPrepared);
        vrPlayer.Play();
    }

    // === PATH HELPER ===
    private string GetLocalPathFor(string s3Key)
    {
        string fileName = Path.GetFileName(s3Key);
        return Path.Combine(Application.persistentDataPath, fileName);
    }
}
