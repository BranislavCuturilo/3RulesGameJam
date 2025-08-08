using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{
    public AudioMixer AudioMixer;

    public void SetVolume(float volume)
    {
        AudioMixer.SetFloat("MasterVolume", volume);
    }

    public void SetQuality(int QualityIndex)
    {
        QualitySettings.SetQualityLevel(QualityIndex);
    }


}
