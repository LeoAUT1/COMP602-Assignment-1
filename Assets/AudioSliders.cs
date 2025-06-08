using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSliders : MonoBehaviour
{
    void Start()
    {
        Debug.Log("audi slde");
    }
    public void UpdateMusicVolume(float vol)
    {
        Debug.Log(vol);
        AudioManager.Instance.SetMusicVolume(vol);
    }

    public void UpdateSfxVolume(float vol)
    {
        AudioManager.Instance.SetSfxVolume(vol);
    }
}
