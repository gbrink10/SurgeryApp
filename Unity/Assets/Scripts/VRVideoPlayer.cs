using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using OculusPassthrough = OculusIntegration.Passthrough;

public class VRVideoPlayer : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Material skyboxMaterial;
    [SerializeField] private GameObject flatScreenPrefab;
    [SerializeField] private Transform environmentRoot;
    
    private GameObject currentScreen;
    private bool isPassthroughActive;
    private OculusPassthrough passthroughLayer;
    private VideoType currentVideoType = VideoType.Flat;

    public enum VideoType
    {
        Flat,
        SideBySide3D,
        Spherical360,
        Spherical360_3D
    }

    private void Start()
    {
        // Initialize Oculus Passthrough
        passthroughLayer = GetComponent<OculusPassthrough>();
        if (passthroughLayer != null)
        {
            passthroughLayer.enabled = false;
        }

        // Setup video player
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }
        
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        videoPlayer.errorReceived += HandleVideoError;
    }

    public void PlayVideo(string url, VideoType videoType)
    {
        StopCurrentVideo();
        currentVideoType = videoType;

        videoPlayer.url = url;
        SetupVideoDisplay(videoType);
        
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += (source) =>
        {
            source.Play();
        };
    }

    private void SetupVideoDisplay(VideoType videoType)
    {
        switch (videoType)
        {
            case VideoType.Flat:
                SetupFlatScreen();
                break;
                
            case VideoType.SideBySide3D:
                SetupSideBySide3D();
                break;
                
            case VideoType.Spherical360:
                Setup360Video(false);
                break;
                
            case VideoType.Spherical360_3D:
                Setup360Video(true);
                break;
        }
    }

    private void SetupFlatScreen()
    {
        if (currentScreen == null)
        {
            currentScreen = Instantiate(flatScreenPrefab, environmentRoot);
            currentScreen.transform.localPosition = new Vector3(0, 1.6f, 2f);
        }
        
        videoPlayer.targetMaterialRenderer = currentScreen.GetComponent<Renderer>();
        videoPlayer.targetMaterialProperty = "_MainTex";
    }

    private void SetupSideBySide3D()
    {
        SetupFlatScreen();
        // Add stereo material configuration for side-by-side 3D
        Material screenMaterial = currentScreen.GetComponent<Renderer>().material;
        screenMaterial.EnableKeyword("_STEREO_SIDE_BY_SIDE");
    }

    private void Setup360Video(bool is3D)
    {
        if (skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
            videoPlayer.targetMaterialRenderer = null;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = new RenderTexture(4096, 2048, 24);
            skyboxMaterial.mainTexture = videoPlayer.targetTexture;

            if (is3D)
            {
                skyboxMaterial.EnableKeyword("_STEREO_TOP_BOTTOM");
            }
        }
    }

    public void TogglePassthrough()
    {
        if (passthroughLayer != null)
        {
            isPassthroughActive = !isPassthroughActive;
            passthroughLayer.enabled = isPassthroughActive;
            
            // Adjust video opacity based on passthrough state
            if (currentScreen != null)
            {
                Material material = currentScreen.GetComponent<Renderer>().material;
                material.SetFloat("_Opacity", isPassthroughActive ? 0.7f : 1.0f);
            }
        }
    }

    public void StopCurrentVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        // Reset skybox if it was used
        if (currentVideoType == VideoType.Spherical360 || 
            currentVideoType == VideoType.Spherical360_3D)
        {
            RenderSettings.skybox = null;
        }
    }

    private void HandleVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"Video Player Error: {message}");
        // Implement error UI feedback here
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.errorReceived -= HandleVideoError;
        }
    }
}
