using UnityEngine;
using System.Collections;
using System.Text;
using System;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.S3;
using Amazon.Runtime;
using UnityEngine.Networking;

public static class CognitoAuth
{
    private const string userPoolId = "eu-north-1_TZH48qVWt";
    private const string clientId = "4c2v0drmi4bchla62f6ohk09ib";
    private const string identityPoolId = "eu-north-1:6ae58465-57c4-4310-9747-a076d51a5bc9";
    private const string region = "eu-north-1";

    public static CognitoAWSCredentials CurrentCredentials { get; private set; }

    public static IEnumerator RegisterUser(string email, string password, Action<string> onSuccess, Action<string> onError)
    {
        string url = $"https://cognito-idp.{region}.amazonaws.com/";
        string jsonBody = "{\"ClientId\":\"" + clientId + "\",\"Username\":\"" + email + "\",\"Password\":\"" + password + "\"}";
        byte[] postData = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");
            request.SetRequestHeader("X-Amz-Target", "AWSCognitoIdentityProviderService.SignUp");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(request.downloadHandler.text);
            else
                onError?.Invoke(request.downloadHandler.text);
        }
    }

    public static IEnumerator ConfirmUser(string email, string code, Action<string> onSuccess, Action<string> onError)
    {
        string url = $"https://cognito-idp.{region}.amazonaws.com/";
        string jsonBody = "{\"ClientId\":\"" + clientId + "\",\"Username\":\"" + email + "\",\"ConfirmationCode\":\"" + code + "\"}";
        byte[] postData = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");
            request.SetRequestHeader("X-Amz-Target", "AWSCognitoIdentityProviderService.ConfirmSignUp");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(request.downloadHandler.text);
            else
                onError?.Invoke(request.downloadHandler.text);
        }
    }

    public static IEnumerator ResendConfirmationCode(string email, Action<string> onSuccess, Action<string> onError)
    {
        string url = $"https://cognito-idp.{region}.amazonaws.com/";
        string jsonBody = "{\"ClientId\":\"" + clientId + "\",\"Username\":\"" + email + "\"}";
        byte[] postData = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");
            request.SetRequestHeader("X-Amz-Target", "AWSCognitoIdentityProviderService.ResendConfirmationCode");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                onSuccess?.Invoke(request.downloadHandler.text);
            else
                onError?.Invoke(request.downloadHandler.text);
        }
    }

    public static IEnumerator LoginUser(string email, string password, Action<string> onSuccess, Action<string> onError)
{
    string url = $"https://cognito-idp.{region}.amazonaws.com/";
    string jsonBody = "{\"AuthParameters\":{\"USERNAME\":\"" + email + "\",\"PASSWORD\":\"" + password + "\"},\"AuthFlow\":\"USER_PASSWORD_AUTH\",\"ClientId\":\"" + clientId + "\"}";
    byte[] postData = Encoding.UTF8.GetBytes(jsonBody);

    using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
    {
        request.uploadHandler = new UploadHandlerRaw(postData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/x-amz-json-1.1");
        request.SetRequestHeader("X-Amz-Target", "AWSCognitoIdentityProviderService.InitiateAuth");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("Login success:\n" + json);

            string idToken = ExtractIdToken(json);

            if (!string.IsNullOrEmpty(idToken))
            {
                var credentials = new CognitoAWSCredentials(identityPoolId, RegionEndpoint.EUNorth1);
                credentials.AddLogin($"cognito-idp.{region}.amazonaws.com/{userPoolId}", idToken);

                bool resolved = false;
                Exception error = null;

                // 🔁 Force credential resolution using async Task
                var task = credentials.GetCredentialsAsync();
                while (!task.IsCompleted) yield return null;

                if (task.IsFaulted)
                {
                    error = task.Exception;
                    Debug.LogError("❌ Failed to resolve AWS credentials: " + error?.Message);
                    onError?.Invoke("Failed to resolve AWS credentials.");
                    yield break;
                }

                // ✅ Store for global access
                AWSCredentialHolder.Credentials = credentials;
                CurrentCredentials = credentials;

                Debug.Log("✅ AWS credentials resolved and stored.");
                onSuccess?.Invoke(json);
            }
            else
            {
                Debug.LogError("❌ Failed to extract ID token.");
                onError?.Invoke("Failed to extract ID token.");
            }
        }
        else
        {
            Debug.LogError("Login failed:\n" + request.downloadHandler.text);
            onError?.Invoke(request.downloadHandler.text);
        }
    }
}


    private static string ExtractIdToken(string json)
    {
        try
        {
            var wrapper = JsonUtility.FromJson<AuthResponseWrapper>(json);
            return wrapper?.AuthenticationResult?.IdToken;
        }
        catch (Exception e)
        {
            Debug.LogError("Token extraction error: " + e.Message);
            return null;
        }
    }

    [Serializable]
    private class AuthResponseWrapper
    {
        public AuthResult AuthenticationResult;
    }

    [Serializable]
    private class AuthResult
    {
        public string IdToken;
        public string AccessToken;
        public string RefreshToken;
    }
}
