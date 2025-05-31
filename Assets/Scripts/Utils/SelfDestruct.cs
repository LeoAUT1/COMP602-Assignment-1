using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float delayInSeconds = 2.0f;
    public bool leftClickToDestroy = true;
    void Start()
    {
        Destroy(gameObject, delayInSeconds);
    }

    private void Update()
    {
        if (leftClickToDestroy && Input.GetMouseButtonDown(0))
        {
            Destroy(gameObject);
        }
    }
}
