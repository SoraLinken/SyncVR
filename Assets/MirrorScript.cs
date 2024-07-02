using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorScript : MonoBehaviour
{
    private Transform trans;
    private Vector3 offset;
    void Start()
    {
        trans = GameObject.Find("Main Camera").GetComponent<Transform>();
        offset = trans.rotation.eulerAngles - transform.rotation.eulerAngles;
    }


    void Update()
    {
        Quaternion rot = Quaternion.Euler(trans.rotation.eulerAngles - offset * -1f);
        gameObject.transform.rotation = rot;
    }
}
