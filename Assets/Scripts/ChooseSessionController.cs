using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseSessionController : MonoBehaviour
{
    public Slider slider;
    public GameObject proceedButtonGameObject;

    void Start()
    {
        proceedButtonGameObject.GetComponent<Button>().onClick.AddListener(OnProceedButtonClick);
    }

    void OnProceedButtonClick()
    {
        int sessionid = (int)slider.value;
        StartCoroutine(GameManager.Instance.OnSessionPicked(sessionid));
    }
}
