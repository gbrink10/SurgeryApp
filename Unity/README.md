# Surgery VR Video Player - Oculus App

A VR video player application for Oculus Quest that supports side-by-side 3D videos, 360° videos, and passthrough mixed reality.

## Features

- Stream surgical videos from AWS S3
- Support for multiple video formats:
  - Standard flat videos
  - Side-by-side 3D videos
  - 360° spherical videos
  - 360° 3D videos
- Passthrough AR support for mixed reality viewing
- VR user interface for video browsing and control
- Automatic video type detection based on filename

## Prerequisites

1. Unity 2022.3 LTS or newer
2. Oculus Integration SDK (from Asset Store)
3. Meta XR All-in-One SDK
4. AWS SDK for Unity
5. SideQuest Desktop App

## Setup Instructions

1. Create a new Unity project using the 3D Core template
2. Install required packages:
   - Window > Package Manager > Add package from git URL:
     ```
     com.unity.xr.oculus
     com.unity.xr.management
     ```
   - Import Oculus Integration from Asset Store
   - Import AWS SDK for Unity

3. Configure the project for Oculus:
   - Edit > Project Settings > XR Plug-in Management
   - Enable Oculus for Android platform
   - Set target device to Quest/Quest 2/Quest Pro

4. Copy the Scripts folder to your Assets directory
5. Set up the scene:
   - Create an empty GameObject named "VRVideoPlayer"
   - Add the VRVideoPlayer, VRUIManager, and AWSManager components
   - Set up the UI prefabs and references in the inspector

## Building for SideQuest

1. Switch platform to Android:
   - File > Build Settings > Android > Switch Platform

2. Configure Player Settings:
   - Package Name: com.yourdomain.surgeryvr
   - Minimum API Level: Android 10.0 (API level 29)
   - Target API Level: Android 10.0 (API level 29)
   - Scripting Backend: IL2CPP
   - Target Architectures: ARM64

3. Build APK:
   - File > Build Settings > Build
   - Name the APK (e.g., "SurgeryVR.apk")

4. Install via SideQuest:
   - Connect your Quest device via USB
   - Enable Developer Mode on your Quest
   - Open SideQuest
   - Drag and drop the APK file to install

## Usage

1. Launch the app on your Quest
2. Use the virtual menu to browse available videos
3. Select a video to play
4. Controls:
   - A/X button: Toggle passthrough
   - B/Y button: Refresh video list
   - Trigger: Select UI elements
   - Grip: Grab and move UI panel

## Video Naming Convention

The app automatically detects video type based on filename:
- Regular video: `surgery_procedure.mp4`
- Side-by-side 3D: `surgery_procedure_3d.mp4` or `surgery_procedure_sbs.mp4`
- 360° video: `surgery_procedure_360.mp4`
- 360° 3D video: `surgery_procedure_360_3d.mp4`

## Troubleshooting

1. AWS Connection Issues:
   - Verify internet connection
   - Check AWS credentials in AWSManager.cs
   - Ensure S3 bucket permissions are correct

2. Video Playback Issues:
   - Check video format compatibility
   - Verify video file size (max 2GB recommended)
   - Check video resolution (max 4K recommended)

3. Performance Issues:
   - Lower video resolution/bitrate
   - Check Quest storage space
   - Clear app cache if necessary

## Support

For issues and feature requests, please create an issue in the GitHub repository.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
