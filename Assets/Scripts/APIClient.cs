using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class APIClient
{
    private const string baseUrl = "http://localhost:8080/api";

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

    internal static void PostRequest(char v)
    {
        throw new NotImplementedException();
    }
}