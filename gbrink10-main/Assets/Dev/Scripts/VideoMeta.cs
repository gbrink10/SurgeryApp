
[System.Serializable]
public class VideoMeta
{
    public string title;
    public string uploader;
    public string videoUrl;
    public VideoType videoType; // Ensure VideoType is defined below or imported
    public StereoFormat stereoFormat; // Ensure StereoFormat is defined below
}

    public enum VideoType 
    {
        Normal = 0,
        OneEighty = 180,
        ThreeSixty = 360
    }

    public enum StereoFormat 
    {
        None = 0,
        TopBottom = 1,
        LeftRight = 2
    }
