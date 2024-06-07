using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    private string apiUrl = "http://localhost:3000";

    public IEnumerator AddItem(string name, int score)
    {
        Item item = new Item { name = name, score = score };
        string json = JsonUtility.ToJson(item);

        UnityWebRequest request = new UnityWebRequest(apiUrl + "/add", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Item added: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }

    public IEnumerator GetItems()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl + "/items");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Items received: " + request.downloadHandler.text);
            ItemList items = JsonUtility.FromJson<ItemList>(request.downloadHandler.text);
            // Handle the received items
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }

    [System.Serializable]
    public class Item
    {
        public string name;
        public int score;
    }

    [System.Serializable]
    public class ItemList
    {
        public List<Item> items;
    }
}
