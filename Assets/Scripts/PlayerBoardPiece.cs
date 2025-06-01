using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBoardPiece : MonoBehaviour
{
    [SerializeField] protected CameraController cam;

    [SerializeField] private GameObject[] playerModels;

    void Start()
    {
        cam = FindAnyObjectByType<CameraController>(); // Good enough :^)

        if (cam != null )
        {
            cam.target = this.transform;
        }

        SetPlayerModel();
    }

    public void SetPlayerModel()
    {

        foreach (GameObject playerModel in playerModels)
        {
            if (playerModel != null)
            {

                playerModel.SetActive(false);
            }
        }

        int level = Player.Instance.GetLevel();

        if (level > 5) //We only have models up to level 5
        {
            level = 5;
        }

        playerModels[level - 1].SetActive(true);
    }
}
