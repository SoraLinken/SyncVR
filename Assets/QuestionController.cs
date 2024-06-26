using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class QuestionnaireData
{
    public string uniqueId;
    public int[] answers;
    public string email;
    public SynchronizationDatum[] synchronizationHands;
    public SynchronizationDatum[] synchronizationPendulum;

    public QuestionnaireData(string uniqueId, List<int> answers, string email, List<SynchronizationDatum> synchronizationHands, List<SynchronizationDatum> synchronizationPendulum)
    {
        this.uniqueId = uniqueId;
        this.answers = answers.ToArray();
        this.email = email;
        this.synchronizationHands = synchronizationHands.ToArray();
        this.synchronizationPendulum = synchronizationPendulum.ToArray();
    }
}


public class QuestionController : MonoBehaviour
{
    public Slider slider;
    public GameObject questionTextGameObject;
    public GameObject pageTextGameObject;
    public GameObject nextButtonGameObject;
    public GameObject nextButtonTextGameObject;

    private TextMeshProUGUI nextButtonText;
    private TextMeshProUGUI pageText;
    private TextMeshProUGUI questionText;

    private int questionIndex = 0;

    private string[] questions = new string[]
    {
        "To what extent did you feel that your movements matched those of your partner?",
        "To what extent did you feel that you and your partner cooperated?",
        "Was the cooperation with your partner good?",
        "Did you feel that you and your partner were synchronized in your movements?",
        "Did you feel a \"sense of togetherness\" when moving in sync with your partner?",
        "Did you feel that there was a similarity between you and your partner?",
        "To what extent would you be open to further cooperation with your partner in a follow-up experiment?",
        "In a follow-up experiment, would you prefer to continue with this partner over a new partner?",
        "Did you notice any issues with the communication network during the activity?"
    };

    public List<int> answers = new List<int>();

    void Start()
    {
        questionText = questionTextGameObject.GetComponent<TextMeshProUGUI>();
        pageText = pageTextGameObject.GetComponent<TextMeshProUGUI>();
        nextButtonText = nextButtonTextGameObject.GetComponentInChildren<TextMeshProUGUI>();

        questionText.text = questions[questionIndex];
        pageText.text = $"{questionIndex + 1}/{questions.Length}";
        nextButtonText.text = "Next";

        nextButtonGameObject.GetComponent<Button>().onClick.AddListener(OnNextButtonClick);
    }

    void OnQuestionnaireFinished()
    {
        QuestionnaireData data = new QuestionnaireData(GameManager.uniqueId, answers, GameManager.email, SynchronizationManager.synchronizationHands, SynchronizationManager.synchronizationPendulum);
        string jsonData = JsonUtility.ToJson(data);

        StartCoroutine(APIClient.PutRequest("/on-game-finish", jsonData, (response) =>
        {
            Debug.Log("Questionnaire finished. Answers: " + string.Join(", ", answers));
        }, (error) =>
        {
            Debug.LogError("Error submitting questionnaire: " + error);
        }));

        GameManager.questionsCanvas.SetActive(false);
    }

    void OnNextButtonClick()
    {
        answers.Add((int)slider.value);

        questionIndex++;

        if (questionIndex < questions.Length)
        {
            questionText.text = questions[questionIndex];
            pageText.text = $"{questionIndex + 1}/{questions.Length}";

            if (questionIndex == questions.Length - 1)
            {
                nextButtonText.text = "Finish";
            }
        }
        else
        {
            OnQuestionnaireFinished();
        }
    }
}
