using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickOneButton : MonoBehaviour
{
    public Button otherButton;
    private Button button;
    private Image buttonImage;

    private Color grayColor = Color.gray;
    private Color greenColor = new Color(0.337f, 0.698f, 0.133f); 

    void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        button.onClick.AddListener(OnButtonClick);
        
        if (otherButton != null)
        {
            otherButton.onClick.AddListener(OnOtherButtonClick);
        }
    }

    void OnButtonClick()
    {
        buttonImage.color = grayColor;
    }

    void OnOtherButtonClick()
    {
        buttonImage.color = greenColor;
    }
}
