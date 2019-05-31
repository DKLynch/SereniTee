using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseHandler : MonoBehaviour
{
    private Scene menuScene;
    public bool isPaused = false;
    public bool isPausable;
    public bool subMenuOpen = false;
    public Canvas pauseMenuCanv;
    public Canvas quitPromptCanv;
    public Canvas settingsMenuCanv;
    public Image pauseTint;
    private InputHandler inputHandler;
    private GameController gameCont;

    private void Awake()
    {
        inputHandler = FindObjectOfType<InputHandler>();
        gameCont = FindObjectOfType<GameController>();

        pauseMenuCanv.enabled = false;
        quitPromptCanv.enabled = false;
        settingsMenuCanv.enabled = false;
    }

    public void PauseGame()
    {
        //Lock Input
        isPaused = true;
        inputHandler.allInputLocked = true;

        //Show Pause UI
        pauseTint.enabled = true;
        pauseMenuCanv.enabled = true;

        //Pause Time
        Time.timeScale = 0f;
    }

    public void UnpauseGame()
    {
        //Unpause Time
        Time.timeScale = 1f;

        //Hide Pause UI
        pauseMenuCanv.enabled = false;
        pauseTint.enabled = false;

        //Unlock Input
        isPaused = false;
        inputHandler.allInputLocked = false;
    }

    public void TogglePause()
    {
        if (!subMenuOpen)
        {
            if (!isPaused)
            {
                PauseGame();
            }
            else
            {
                UnpauseGame();
            }
        } else
        {
            ReturnToPauseMenu();
        }
      
    }

    public void OpenOptions()
    {
        subMenuOpen = true;
        settingsMenuCanv.enabled = true;
        pauseMenuCanv.enabled = false;

        //The same text update fix I applied in the quit prompt method
        var textObjects = settingsMenuCanv.GetComponentsInChildren<TextMeshProUGUI>();
        foreach(TextMeshProUGUI text in textObjects)
        {
            text.ForceMeshUpdate();
        }
    }

    public void ReturnToPauseMenu()
    {
        subMenuOpen = false;
        quitPromptCanv.enabled = false;
        settingsMenuCanv.enabled = false;
        pauseMenuCanv.enabled = true;
    }

    public void QuitToMenu()
    {
        //Unpause time
        Time.timeScale = 1f;

        //Clear Game Data
        gameCont.ResetGameData();

        //Load Menu
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenQuitPrompt()
    {
        subMenuOpen = true;
        quitPromptCanv.enabled = true;
        pauseMenuCanv.enabled = false;

        //For some reason, upon opening the quit prompt, text was somewhat distorted until something prompted an update either through the inspector or script
        //After a touch of tinkering, I found this method and it seems to do what I needed it to, and fixes the bug.
        var textObjects = quitPromptCanv.GetComponentsInChildren<TextMeshProUGUI>();
        foreach(TextMeshProUGUI text in textObjects)
        {
           text.ForceMeshUpdate();
        }
    }

    public void ExitToDesktop()
    {
        Application.Quit();
    }
}
