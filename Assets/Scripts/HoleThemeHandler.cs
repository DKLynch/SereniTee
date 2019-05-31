using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HoleThemeHandler : MonoBehaviour
{
    public List<string> themes;

    //Selects a new theme from the array of strings
    public void SelectNewTheme()
    {
        int themeIndex = Random.Range(0, themes.Count);
        SceneManager.LoadScene(themes[themeIndex], LoadSceneMode.Single);
    }
}
