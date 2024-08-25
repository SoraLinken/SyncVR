using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A static class for making API requests (GET, POST, PUT, DELETE) to a specified base URL.
/// </summary>
public static class APIClient
{
    // The base URL for the API requests.
    private const string baseUrl = "https://syncvr-1295f7818a49.herokuapp.com/api";
    
    /// <summary>
    /// Sends a GET request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The API endpoint to send the GET request to.</param>
    /// <param name="onSuccess">Action to be called on a successful response, passing the response text as a parameter.</param>
    /// <param name="onError">Action to be called if the request fails, passing the error message as a parameter.</param>
    /// <returns>An IEnumerator that can be used to run the request as a coroutine.</returns>
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

    /// <summary>
    /// Sends a POST request to the specified endpoint with JSON data.
    /// </summary>
    /// <param name="endpoint">The API endpoint to send the POST request to.</param>
    /// <param name="jsonData">The JSON data to be sent in the body of the request.</param>
    /// <param name="onSuccess">Action to be called on a successful response, passing the response text as a parameter.</param>
    /// <param name="onError">Action to be called if the request fails, passing the error message as a parameter.</param>
    /// <returns>An IEnumerator that can be used to run the request as a coroutine.</returns>
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

    /// <summary>
    /// Sends a PUT request to the specified endpoint with JSON data.
    /// </summary>
    /// <param name="endpoint">The API endpoint to send the PUT request to.</param>
    /// <param name="jsonData">The JSON data to be sent in the body of the request.</param>
    /// <param name="onSuccess">Action to be called on a successful response, passing the response text as a parameter.</param>
    /// <param name="onError">Action to be called if the request fails, passing the error message as a parameter.</param>
    /// <returns>An IEnumerator that can be used to run the request as a coroutine.</returns>
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

    /// <summary>
    /// Sends a DELETE request to the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The API endpoint to send the DELETE request to.</param>
    /// <param name="onSuccess">Action to be called on a successful response, passing the response text as a parameter.</param>
    /// <param name="onError">Action to be called if the request fails, passing the error message as a parameter.</param>
    /// <returns>An IEnumerator that can be used to run the request as a coroutine.</returns>
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
