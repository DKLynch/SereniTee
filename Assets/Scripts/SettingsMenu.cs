using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Slider redSlider, greenSlider, blueSlider;
    public Image colorPreviewImg;
    private BallController ballCont;
    //private AudioMixer masterVolume;

    private void Start()
    {
        ballCont = FindObjectOfType<BallController>();
    }

    public void UpdateVolume(float volume)
    {
        //masterVolume.SetFloat("volume", volume);
    }

    public void UpdateBallColour()
    {
        var r = redSlider.value;
        var g = greenSlider.value;
        var b = blueSlider.value;
        Color newColor = new Color(r/255f, g/255f, b/255f, 1f);

        //Update the preview image and ball materials
        colorPreviewImg.color = newColor;
        ballCont.ballMat.color = newColor;
        ballCont.ballEmissiveMat.color = newColor;
        ballCont.ballEmissiveMat.SetColor("_EmissionColor", newColor);
        ballCont.ballLight.color = newColor;
    }
    
    public void UpdateMaxFPS(float maxFps)
    {
        Application.targetFrameRate = Mathf.RoundToInt(maxFps);
    }

    public void UpdateFullscreen(bool useFullscreenMode)
    {
        if (useFullscreenMode)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        } else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }

    public void UpdateQualitySetting(int qualIndex)
    {
        QualitySettings.SetQualityLevel(qualIndex);
    }
}
