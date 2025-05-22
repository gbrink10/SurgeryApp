using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;

public class VRUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VRVideoPlayer videoPlayer;
    [SerializeField] private Transform videoListContent;
    [SerializeField] private GameObject videoItemPrefab;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button passthroughToggle;
    [SerializeField] private TextMeshProUGUI statusText;
    
    [Header("UI Settings")]
    [SerializeField] private float uiDistance = 2f;
    [SerializeField] private float uiHeight = 1.6f;
    [SerializeField] private bool followCamera = true;
    
    private Camera mainCamera;
    private List<VideoInfo> currentVideos;
    private bool isLoading;

    private void Start()
    {
        mainCamera = Camera.main;
        
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshVideoList);
            
        if (passthroughToggle != null)
            passthroughToggle.onClick.AddListener(() => videoPlayer.TogglePassthrough());

        // Position UI in front of user
        PositionUI();
        
        // Initial video list load
        RefreshVideoList();
    }

    private void Update()
    {
        if (followCamera)
        {
            PositionUI();
        }

        // Handle input for UI interaction
        HandleInput();
    }

    private void PositionUI()
    {
        if (mainCamera != null)
        {
            Vector3 position = mainCamera.transform.position + 
                             mainCamera.transform.forward * uiDistance;
            position.y = mainCamera.transform.position.y + uiHeight;
            
            transform.position = position;
            transform.rotation = Quaternion.LookRotation(
                transform.position - mainCamera.transform.position
            );
        }
    }

    private void HandleInput()
    {
        // Example: Use Oculus controller buttons for UI interaction
        if (OVRInput.GetDown(OVRInput.Button.Two)) // B or Y button
        {
            RefreshVideoList();
        }
        
        if (OVRInput.GetDown(OVRInput.Button.One)) // A or X button
        {
            videoPlayer.TogglePassthrough();
        }
    }

    public async void RefreshVideoList()
    {
        if (isLoading) return;
        
        isLoading = true;
        SetStatus("Loading videos...");
        
        try
        {
            // Clear existing video items
            foreach (Transform child in videoListContent)
            {
                Destroy(child.gameObject);
            }

            // Get videos from AWS
            currentVideos = await AWSManager.Instance.ListVideos();
            
            // Create UI items for each video
            foreach (var video in currentVideos)
            {
                CreateVideoListItem(video);
            }
            
            SetStatus($"Found {currentVideos.Count} videos");
        }
        catch (System.Exception e)
        {
            SetStatus($"Error: {e.Message}");
            Debug.LogError($"Error refreshing video list: {e}");
        }
        finally
        {
            isLoading = false;
        }
    }

    private void CreateVideoListItem(VideoInfo video)
    {
        GameObject item = Instantiate(videoItemPrefab, videoListContent);
        VideoListItem listItem = item.GetComponent<VideoListItem>();
        
        if (listItem != null)
        {
            // Extract video name from key
            string videoName = System.IO.Path.GetFileNameWithoutExtension(video.Key);
            
            listItem.Initialize(videoName, video.LastModified.ToString("g"), async () =>
            {
                SetStatus($"Loading video: {videoName}");
                
                try
                {
                    string url = await AWSManager.Instance.GetVideoUrl(video.Key);
                    if (!string.IsNullOrEmpty(url))
                    {
                        // Determine video type based on filename or metadata
                        VRVideoPlayer.VideoType videoType = DetermineVideoType(video.Key);
                        videoPlayer.PlayVideo(url, videoType);
                        SetStatus($"Playing: {videoName}");
                    }
                    else
                    {
                        SetStatus("Error: Could not get video URL");
                    }
                }
                catch (System.Exception e)
                {
                    SetStatus($"Error playing video: {e.Message}");
                }
            });
        }
    }

    private VRVideoPlayer.VideoType DetermineVideoType(string filename)
    {
        filename = filename.ToLower();
        
        if (filename.Contains("3d") || filename.Contains("sbs"))
            return VRVideoPlayer.VideoType.SideBySide3D;
        else if (filename.Contains("360_3d"))
            return VRVideoPlayer.VideoType.Spherical360_3D;
        else if (filename.Contains("360"))
            return VRVideoPlayer.VideoType.Spherical360;
        else
            return VRVideoPlayer.VideoType.Flat;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"Status: {message}");
    }

    private void OnDestroy()
    {
        if (refreshButton != null)
            refreshButton.onClick.RemoveAllListeners();
            
        if (passthroughToggle != null)
            passthroughToggle.onClick.RemoveAllListeners();
    }
}

// Component for individual video list items
public class VideoListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private Button playButton;

    public void Initialize(string title, string date, UnityEngine.Events.UnityAction onClick)
    {
        if (titleText != null) titleText.text = title;
        if (dateText != null) dateText.text = date;
        if (playButton != null) playButton.onClick.AddListener(onClick);
    }

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveAllListeners();
    }
}
