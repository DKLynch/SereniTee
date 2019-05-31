using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MenuHandler : MonoBehaviour
{
    private HoleThemeHandler themeHandler;
    private GameData gameData;
    public GameObject gameDataPrefab;
    public Image fadeImg;

    private void Awake()
    {
        //Instantiate our GameData prefab if it doesn't already exist.
        if (!FindObjectOfType<GameData>())
        {
            gameData = Instantiate(gameDataPrefab).GetComponent<GameData>();
        }
        else
        {
            gameData = FindObjectOfType<GameData>();
        }
    }

    void Start()
    {
        themeHandler = FindObjectOfType<HoleThemeHandler>();

        //Set FPS & Vsync
        QualitySettings.vSyncCount = 0;

        //Fade From Black
        FadeIn();
    }

    public void Start9HoleCourse()
    {
        FadeOut();
        gameData.gameMode = "9Hole";
        gameData.noHoles = 9;
        Invoke("SwitchScene", 1.5f);
    }

    public void Start18HoleCourse()
    {
        FadeOut();
        gameData.gameMode = "18Hole";
        gameData.noHoles = 18;
        Invoke("SwitchScene", 1.5f);
    }

    public void StartEndlessCourse()
    {
        FadeOut();
        gameData.gameMode = "Endless";
        gameData.noHoles = -1;
        Invoke("SwitchScene", 1.5f);
    }

    public void SwitchScene()
    {
        themeHandler.SelectNewTheme();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void FadeIn()
    {
        fadeImg.enabled = true;
        fadeImg.DOFade(0f, 1.5f).OnComplete(DisableImg);
    }

    private void FadeOut()
    {
        fadeImg.enabled = true;
        fadeImg.DOFade(1f, 1.5f);
    }

    public void DisableImg()
    {
        fadeImg.enabled = false;
    }
}
