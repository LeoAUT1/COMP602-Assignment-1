using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowSettings : MonoBehaviour
{

    public GameObject settings;
    public GameObject main;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Settings(bool b)
    {
        settings.SetActive(b);
        main.SetActive(!b);
    }
}
