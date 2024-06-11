// Assets/Scripts/WebRequestHandler.cs
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequestHandler : MonoBehaviour
{
    private string backendUrl = "http://localhost:3000/data";

    public IEnumerator SendDataToBackend(string json)
    {
        UnityWebRequest request = new UnityWebRequest(backendUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Data sent successfully");
        }
        else
        {
            Debug.LogError("Error sending data: " + request.error);
        }
    }

    public IEnumerator GetDataFromBackend()
    {
        UnityWebRequest request = UnityWebRequest.Get(backendUrl);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Data retrieved successfully: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error retrieving data: " + request.error);
        }
    }
}
