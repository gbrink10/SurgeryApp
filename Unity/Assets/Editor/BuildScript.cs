using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using System;
using System.IO;

public class BuildScript
{
    [MenuItem("Build/Build for Oculus Quest")]
    public static void BuildForQuest()
    {
        try
        {
            // Set up the build settings
            EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Android,
                BuildTarget.Android
            );

            // Configure player settings
            PlayerSettings.SetApplicationIdentifier(
                BuildTargetGroup.Android,
                "com.surgeryvr.app"
            );
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            
            // VR Settings
            PlayerSettings.virtualRealitySupported = true;
            PlayerSettings.SetVirtualRealitySDKs(
                BuildTargetGroup.Android,
                new string[] { "Oculus" }
            );

            // Set scripting backend
            PlayerSettings.SetScriptingBackend(
                BuildTargetGroup.Android,
                ScriptingImplementation.IL2CPP
            );

            // Set target architectures
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // Create build directory if it doesn't exist
            string buildPath = "Builds/Android";
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }

            // Build the APK
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(buildPath, "SurgeryVR.apk"),
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {summary.totalSize} bytes");
                
                // Open build folder
                EditorUtility.RevealInFinder(buildPath);
            }
            else
            {
                Debug.LogError($"Build failed: {summary.result}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during build: {e.Message}\n{e.StackTrace}");
        }
    }

    [MenuItem("Build/Build Development APK")]
    public static void BuildDevelopmentAPK()
    {
        BuildForQuest();
    }

    [MenuItem("Build/Build Release APK")]
    public static void BuildReleaseAPK()
    {
        try
        {
            // Set release configuration
            EditorUserBuildSettings.development = false;
            PlayerSettings.Android.useCustomKeystore = true;
            
            // Build the APK
            BuildForQuest();
        }
        finally
        {
            // Reset development settings
            EditorUserBuildSettings.development = true;
        }
    }

    [MenuItem("Build/Clean Build Folder")]
    public static void CleanBuildFolder()
    {
        string buildPath = "Builds/Android";
        if (Directory.Exists(buildPath))
        {
            Directory.Delete(buildPath, true);
            Debug.Log("Build folder cleaned successfully");
        }
    }
}
