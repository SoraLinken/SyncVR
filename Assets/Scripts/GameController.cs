public class GameController : MonoBehaviour
{
    public APIManager apiManager;

    void Start()
    {
        StartCoroutine(apiManager.GetItems());
    }

    public void AddNewItem(string name, int score)
    {
        StartCoroutine(apiManager.AddItem(name, score));
    }
}
