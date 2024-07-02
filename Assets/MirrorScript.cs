using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorScript : MonoBehaviour
{
    private Transform trans;
    private float initialX;

    void Start()
    {
        trans = GameObject.Find("Main Camera").GetComponent<Transform>();
        initialX = transform.position.x;
    }

    void Update()
    {
        // float reflectedX = trans.position.x - initialX;
        // Vector3 newPosition = new Vector3(reflectedX/2, transform.position.y, transform.position.z);
        // transform.position = newPosition;
    }
}
