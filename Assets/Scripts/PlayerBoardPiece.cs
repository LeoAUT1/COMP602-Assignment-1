using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBoardPiece : MonoBehaviour
{
    [SerializeField] protected CameraController cam;
    void Start()
    {
        cam = FindAnyObjectByType<CameraController>(); // Good enough :^)

        if (cam != null )
        {
            cam.target = this.transform;
        }
    }
}
