using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoosePlayerController : MonoBehaviour
{
    static public ChoosePlayerController Instance { get; private set; }
    public GameObject player1Button;
    public GameObject player2Button;

    public TextMeshProUGUI player1Text;
    public TextMeshProUGUI player2Text;

    public GameObject startSessionButton;

    Button startSessionButtonComponent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        player1Button.GetComponent<Button>().onClick.AddListener(OnPlayer1ButtonClick);
        player2Button.GetComponent<Button>().onClick.AddListener(OnPlayer2ButtonClick);
        startSessionButtonComponent = startSessionButton.GetComponent<Button>();
        startSessionButtonComponent.enabled = false;
        startSessionButtonComponent.onClick.AddListener(OnStartSessionButtonClick);
    }

    public void initialize()
    {
        player1Text.text = GameManager.selectedParticipants[0].name;
        player2Text.text = GameManager.selectedParticipants[1].name;
    }

    void OnPlayer1ButtonClick()
    {
        GameManager.email = GameManager.selectedParticipants[0].email;
        startSessionButtonComponent.enabled = true;
    }

    void OnPlayer2ButtonClick()
    {
        GameManager.email = GameManager.selectedParticipants[1].email;
        startSessionButtonComponent.enabled = true;
    }

    void OnStartSessionButtonClick()
    {
        GameManager.Instance.OnPlayerChosen();
    }
}
