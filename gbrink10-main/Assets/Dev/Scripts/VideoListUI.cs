using UnityEngine;

public class VideoListUI : MonoBehaviour
{
    public VideoListLoader loader;
    public VideoButton cardPrefab;
    public Transform cardParent;

    private async void Start()
    {
        // Wait until the videos are actually loaded
        while (loader.videos == null || loader.videos.Count == 0)
        {
            await System.Threading.Tasks.Task.Delay(200);
        }

        foreach (var video in loader.videos)
        {
            Debug.Log($"[UI] Adding video card: {video.title}");
            var ui = Instantiate(cardPrefab, cardParent);
            ui.Setup(video);
        }
    }
}
