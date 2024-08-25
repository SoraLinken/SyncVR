using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// Generic class to conduct API requests to the SyncVR server.
public static class APIClient
{
    // The Sync VR API base URL.
    private const string baseUrl = "https://syncvr-1295f7818a49.herokuapp.com/api";
    
   
    public static IEnumerator GetRequest(string endpoint, System.Action<string> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(baseUrl + endpoint))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(webRequest.downloadHandler.text);
            }
            else
            {
                onError?.Invoke(webRequest.error);
            }
        }
    }


    public static IEnumerator PostRequest(string endpoint, string jsonData, System.Action<string> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(baseUrl + endpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(webRequest.downloadHandler.text);
            }
            else
            {
                onError?.Invoke(webRequest.error);
            }
        }
    }

    public static IEnumerator PutRequest(string endpoint, string jsonData, System.Action<string> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(baseUrl + endpoint, "PUT"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(webRequest.downloadHandler.text);
            }
            else
            {
                onError?.Invoke(webRequest.error);
            }
        }
    }

    public static IEnumerator DeleteRequest(string endpoint, System.Action<string> onSuccess, System.Action<string> onError)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Delete(baseUrl + endpoint))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(webRequest.downloadHandler.text);
            }
            else
            {
                onError?.Invoke(webRequest.error);
            }
        }
    }
}
