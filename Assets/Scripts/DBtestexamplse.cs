// Example usage in another script
public class ExampleUsage : MonoBehaviour
{
    private WebRequestHandler webRequestHandler;

    private void Start()
    {
        webRequestHandler = gameObject.AddComponent<WebRequestHandler>();

        // Example JSON data to send
        string jsonData = "{\"field1\":\"value1\",\"field2\":42}";
        StartCoroutine(webRequestHandler.SendDataToBackend(jsonData));

        // Retrieve data from backend
        StartCoroutine(webRequestHandler.GetDataFromBackend());
    }
}
