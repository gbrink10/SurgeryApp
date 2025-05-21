using UnityEngine;
using UnityEngine.UI;
using Evereal.VRVideoPlayer;

public class VideoButton : MonoBehaviour
{
    public Text titleText;
    public Text uploaderText;

    public Button streamPlayButton;
    public Button downloadButton;
    public Button playDownloadedButton;
    public Button deleteButton;

    public Text downloadStatusText; // optional for showing % or status

    private VideoMeta videoMeta;
    private VideoPlaybackManager playbackManager;

    public void Setup(VideoMeta meta)
    {
        videoMeta = meta;
        titleText.text = System.IO.Path.GetFileName(meta.videoUrl);
        uploaderText.text = "";

        playbackManager = FindAnyObjectByType<VideoPlaybackManager>();

        if (playbackManager == null)
        {
            Debug.LogError("❌ VideoPlaybackManager not found.");
            return;
        }

        streamPlayButton.onClick.AddListener(OnStreamPlayClicked);
        downloadButton.onClick.AddListener(OnDownloadClicked);
        playDownloadedButton.onClick.AddListener(OnPlayDownloadedClicked);
        deleteButton.onClick.AddListener(OnDeleteClicked);

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        bool isDownloaded = playbackManager.IsVideoDownloaded(videoMeta.videoUrl);

        streamPlayButton.gameObject.SetActive(true); // Always visible

        downloadButton.gameObject.SetActive(!isDownloaded); // Hide if downloaded
        playDownloadedButton.gameObject.SetActive(isDownloaded); // Show only if downloaded
        deleteButton.gameObject.SetActive(isDownloaded); // Show only if downloaded
    }

    private void OnStreamPlayClicked()
    {
        playbackManager.PlayVideo(videoMeta.videoUrl);
        CloseMenu();
    }

    private void OnDownloadClicked()
    {
        playbackManager.DownloadAndPlay(videoMeta.videoUrl, OnDownloadProgress, OnDownloadComplete);
        downloadStatusText.text = "Starting download...";
    }

    private void OnPlayDownloadedClicked()
    {
        playbackManager.PlayLocal(videoMeta.videoUrl);
        CloseMenu();
    }

    private void OnDeleteClicked()
    {
        playbackManager.DeleteLocalCopy(videoMeta.videoUrl);
        UpdateButtons();
        if (downloadStatusText != null)
            downloadStatusText.text = "Deleted";
    }

    private void OnDownloadProgress(float percent)
    {
        if (downloadStatusText != null)
            downloadStatusText.text = $"Downloading... {percent:0.0}%";
    }

    private void OnDownloadComplete()
    {
        if (downloadStatusText != null)
            downloadStatusText.text = "Downloaded ✔";
        UpdateButtons();
    }

    private void CloseMenu()
    {
        var videoControlPanel = FindAnyObjectByType<VideoControlPanel>();
        if (videoControlPanel != null)
        {
            videoControlPanel.HideMainMenu();
        }
        else
        {
            Debug.LogError("❌ VideoControlPanel not found.");
        }
    }
}
