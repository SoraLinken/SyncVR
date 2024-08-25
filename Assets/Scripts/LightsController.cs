using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Changes the room lightning based on the syncronization level
public class LightsController : MonoBehaviour
{
    public static LightsController Instance { get; private set;}
    private Light bakedLight;

    public  void changeLightColor(Color newColor){
        if(bakedLight != null)
        {
            bakedLight.color = newColor;
        }
    }

    void Start()
    {
        Instance = this;
        bakedLight = GetComponent<Light>();
    }
}
