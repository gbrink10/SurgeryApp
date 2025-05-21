using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Evereal.VRVideoPlayer;

public class VideoControlPanel : MonoBehaviour
{
    public VRVideoPlayer vrPlayer;
    public Transform screenTransform;

    public Button pauseButton;
    public Button resumeButton;
    public Button resizeButton;
    public Button exitButton;

    public Slider progressSlider;

    public Transform menuToShowOnExit, thisPanelUI;

    private int sizeStep = 0;
    private float[] sizeMultipliers = new float[] { 1f, 0.7f, 1.3f, 1.5f };
    private Vector3 originalScale;

    private bool isDraggingSlider = false;
    private bool wasPlayingBeforeDrag = false;

    void Start()
    {
        if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseClicked);
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (resizeButton != null) resizeButton.onClick.AddListener(OnResizeClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
            progressSlider.onValueChanged.AddListener(OnSliderChanged);

            // Make sure the slider has EventTrigger to connect dragging events
            AddEventTrigger(progressSlider.gameObject, EventTriggerType.PointerDown, OnSliderDragStart);
            AddEventTrigger(progressSlider.gameObject, EventTriggerType.PointerUp, OnSliderDragEnd);
        }

        if (screenTransform != null)
            originalScale = screenTransform.localScale;

        thisPanelUI?.gameObject.SetActive(false);
    }

    void Update()
    {
        if (vrPlayer != null && vrPlayer.isPrepared && vrPlayer.length > 0 && !isDraggingSlider)
        {
            double current = vrPlayer.time;
            double total = vrPlayer.length;
            progressSlider.value = (float)(current / total);
        }
    }

    private void OnPauseClicked()
    {
        if (vrPlayer != null && vrPlayer.isPlaying)
            vrPlayer.Pause();
    }

    private void OnResumeClicked()
    {
        if (vrPlayer != null && !vrPlayer.isPlaying)
            vrPlayer.Play();
    }

    private void OnResizeClicked()
    {
        sizeStep = (sizeStep + 1) % sizeMultipliers.Length;
        if (screenTransform != null)
            screenTransform.localScale = originalScale * sizeMultipliers[sizeStep];
    }

    private void OnExitClicked()
    {
        if (vrPlayer != null)
            vrPlayer.Stop();

        if (menuToShowOnExit != null)
            menuToShowOnExit.gameObject.SetActive(true);

        if (thisPanelUI != null)
            thisPanelUI.gameObject.SetActive(false);
    }

    private void OnSliderChanged(float value)
    {
        if (isDraggingSlider && vrPlayer != null && vrPlayer.isPrepared && vrPlayer.length > 0)
        {
            double newTime = vrPlayer.length * value;
            vrPlayer.time = newTime;
        }
    }

    public void OnSliderDragStart(BaseEventData data)
    {
        isDraggingSlider = true;
        if (vrPlayer != null)
        {
            wasPlayingBeforeDrag = vrPlayer.isPlaying;
            if (vrPlayer.isPlaying)
                vrPlayer.Pause();
        }
    }

    public void OnSliderDragEnd(BaseEventData data)
    {
        isDraggingSlider = false;
        if (vrPlayer != null && vrPlayer.isPrepared && vrPlayer.length > 0)
        {
            double newTime = vrPlayer.length * progressSlider.value;
            vrPlayer.time = newTime;

            if (wasPlayingBeforeDrag)
                vrPlayer.Play();
        }
    }

    public void HideMainMenu()
    {
        if (menuToShowOnExit != null)
            menuToShowOnExit.gameObject.SetActive(false);

        if (thisPanelUI != null)
            thisPanelUI.gameObject.SetActive(true);
    }

    public void OnVideoStarted()
    {
        isDraggingSlider = false;
        if (progressSlider != null)
            progressSlider.value = 0f;
    }

    // Adds drag events to the slider
    private void AddEventTrigger(GameObject target, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = target.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }
}
