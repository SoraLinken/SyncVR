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
        "To what extent were your movements in time with the other participant?",
        "How likable did you find the other participant in the experiment?",
        "How friendly did you find the other participant in the experiment?",
        "How dominant did you find the other participant in the experiment?",
        "Did you experience a feeling of togetherness when moving with the other participant?",
        "Did you feel your movements were coordinated with the other participant's movements?",
        "Did you feel you and the other participant were a unit during the task? ",
        "Did you feel you and the other participant cooperated during the task?",
        "How close do you currently feel to the other participant in the experiment?",
        "Did you try to ignore the other participant during the task?",
        "Did you feel you and the other participant worked together during the task?",
        "Would you be open to future cooperation with the other participant?"
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
        QuestionnaireData data = new QuestionnaireData(GameManager.uniqueId, answers, GameManager.email, SynchronizationManager.NormalizeListSize(SynchronizationManager.synchronizationHands), SynchronizationManager.synchronizationPendulum);
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
