using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public GameObject agreeNumberGameObject;
    private TextMeshProUGUI agreeNumberText;
    private Slider slider;

    void Start()
    {
        agreeNumberText = agreeNumberGameObject.GetComponent<TextMeshProUGUI>();

        slider = GetComponent<Slider>();

        slider.onValueChanged.AddListener(delegate { UpdateAgreeNumber(); });

        UpdateAgreeNumber();
    }

    void UpdateAgreeNumber()
    {
        agreeNumberText.text = slider.value.ToString();
    }
}
