using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIOverlayManager : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Transform[] overlays;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            foreach (Transform t in overlays)
            {
                t.gameObject.SetActive(false);
            }
        }
    }
}
