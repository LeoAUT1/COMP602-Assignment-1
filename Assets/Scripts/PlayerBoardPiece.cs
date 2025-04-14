using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBoardPiece : MonoBehaviour
{
    [SerializeField] protected CameraController cam;
    void Start()
    {
        if (cam == null)
        {
            cam = FindAnyObjectByType<CameraController>(); // Good enough :^)
            cam.target = this.transform;
        }
    }
}
