using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

public class GameController : MonoBehaviour
{
    [Header("General Script References")]
    public GameObject gameDataPrefab;
    private HoleGen holeGen;
    private GameData gameData;
    private BallController ballCont;
    private InputHandler inputHandler;
    private HoleThemeHandler holeThemeHandler;
    private DefaultMats defMats;
    private PauseHandler pauseHandler;

    [Header("UI")]
    public TextMeshProUGUI holeResultText;
    public TextMeshProUGUI 
        holeNoText,
        holeParText,
        holeStrokesText;

    public GameObject sBoard9;
    public GameObject sBoard18;

    private List<TextMeshProUGUI> 
        sBoardElements = new List<TextMeshProUGUI>(),
        sBoardHoleNos = new List<TextMeshProUGUI>(),
        sBoardScores = new List<TextMeshProUGUI>(),
        sBoardPars = new List<TextMeshProUGUI>();

    public List<TextMeshProUGUI> gameUIElements;
    public Image sBoardImage;
    public Image holeIcon;
    private Image fadeImage;

    void Awake()
    {
        //Fetch Components
        ballCont = FindObjectOfType<BallController>();
        holeGen = this.GetComponent<HoleGen>();
        inputHandler = this.GetComponent<InputHandler>();
        holeThemeHandler = this.GetComponent<HoleThemeHandler>();
        defMats = this.GetComponent<DefaultMats>();
        pauseHandler = FindObjectOfType<PauseHandler>();
        fadeImage = GameObject.Find("BlackFade").GetComponent<Image>();
        holeResultText = GameObject.Find("HoleResultText").GetComponent<TextMeshProUGUI>();

        //Instantiate our GameData prefab if it doesn't already exist.
        if (!FindObjectOfType<GameData>())
        {
            gameData = Instantiate(gameDataPrefab).GetComponent<GameData>();
        }
        else
        {
            gameData = FindObjectOfType<GameData>();
        }

        //Select the relevant scoreboard
        SelectScoreboard(gameData.gameMode, gameData.noHoles);

        //Set Vsync
        QualitySettings.vSyncCount = 0;
    }

    public void Start()
    {
        StartCoroutine(StartNewHole());
    }

    //Selects and fetches the relevant scoreboard type appropriate to the current game mode selected
    private void SelectScoreboard(string gameMode, int noHoles)
    {
        switch (gameMode)
        {
            case "9Hole":
                sBoard9.SetActive(true);
                sBoardScores = sBoard9.transform.Find("sBoardScores").GetComponentsInChildren<TextMeshProUGUI>().ToList();
                sBoardPars = sBoard9.transform.Find("sBoardPars").GetComponentsInChildren<TextMeshProUGUI>().ToList();
                sBoardHoleNos = sBoard9.transform.Find("sBoardHoleNos").GetComponentsInChildren<TextMeshProUGUI>().ToList();
                break;
            case "18Hole":
                sBoard18.SetActive(true);
                sBoardScores = sBoard18.transform.Find("sBoardScores").GetComponentsInChildren<TextMeshProUGUI>().ToList();
                sBoardPars = sBoard18.transform.Find("sBoardPars").GetComponentsInChildren<TextMeshProUGUI>().ToList();
                sBoardHoleNos = sBoard18.transform.Find("sBoardHoleNos").GetComponentsInChildren<TextMeshProUGUI>().ToList();
                break;
            case "Endless":
                sBoard18.SetActive(true);
                sBoardScores = sBoard18.transform.Find("sBoardScores").GetComponentsInChildren<TextMeshProUGUI>().ToList();
                sBoardPars = sBoard18.transform.Find("sBoardPars").GetComponentsInChildren<TextMeshProUGUI>().ToList();
                sBoardHoleNos = sBoard18.transform.Find("sBoardHoleNos").GetComponentsInChildren<TextMeshProUGUI>().ToList();
                break;
        }

        foreach (TextMeshProUGUI t in sBoardScores)
        {
            sBoardElements.Add(t);
        }

        foreach (TextMeshProUGUI t in sBoardPars)
        {
            sBoardElements.Add(t);
        }

        foreach (TextMeshProUGUI t in sBoardHoleNos)
        {
            sBoardElements.Add(t);
        }

        var totalLabels = GameObject.FindGameObjectsWithTag("TotalsLabel");
        foreach(GameObject label in totalLabels)
        {
            sBoardElements.Add(label.GetComponent<TextMeshProUGUI>());
        }

        sBoardElements.Add(GameObject.FindGameObjectWithTag("ParTotalText").GetComponent<TextMeshProUGUI>());
        sBoardElements.Add(GameObject.FindGameObjectWithTag("ScoreTotalText").GetComponent<TextMeshProUGUI>());
    }

    public IEnumerator StartNewHole()
    {
        //Lock player inputs
        inputHandler.allInputLocked = true;
        pauseHandler.isPausable = false;

        //Pause ball movement
        ballCont.PauseBallActivity();

        //Generate new hole
        StartCoroutine(holeGen.GenerateHole());
        yield return new WaitForSeconds(2f);

        //Update the hole mesh renderers
        defMats.GetHoleRenderers();

        //Fetch game data and feed it to the game UI
        UpdateGameUI();

        //Spawn the ball
        ballCont.UpdateBallSpawnPos();
        ballCont.RespawnBall(false, ballCont.ballSpawnPos);

        //Fade in from black
        FadeGameIn(1f);
        FadeGameUIIn(1f);
        yield return new WaitForSeconds(1f);

        //Free up player inputs
        inputHandler.allInputLocked = false;
        pauseHandler.isPausable = true;

        yield return null;
    }

    public IEnumerator MoveToNextHole()
    {
        //Add the player's hole score to the scores list
        gameData.AddScore(gameData.currentStrokes);

        //Lock Player Inputs
        inputHandler.allInputLocked = true;
        pauseHandler.isPausable = false;

        //Pause ball activity
        ballCont.PauseBallActivity();
        var bFollow = FindObjectsOfType<BallFollow>().First(b => b.name == "CamTarget");
        bFollow.enabled = false;

        //Show hole result
        string result = DetermineHoleResult();
        Debug.Log("Hole Result: " + result);
        ShowHoleResult(result, 2.5f);
        yield return new WaitForSeconds(.5f);

        //Fade to black
        FadeGameOut(2f);
        FadeGameUIOut(2f);
        yield return new WaitForSeconds(2f);

        //Fade result out
        FadeResultOut(1.5f);
        yield return new WaitForSeconds(.5f);

        //Determine if the player still has holes left to play and act accordingly
        if (gameData.currentHoleNo < gameData.noHoles || gameData.noHoles == -1)
        {
            //Show scoreboard    
            FadeScoreboardIn(1f);

            //Enable the click to continue text
            var contText = GameObject.Find("ClickContText");
            contText.GetComponent<AlphaPulse>().enabled = true;

            //Wait for click input to continue
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

            //Kill continue text alpha tween and hide it
            contText.GetComponent<AlphaPulse>().tween.Kill();
            contText.GetComponent<AlphaPulse>().enabled = false;
            contText.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0f);

            //Fade to black
            FadeScoreboardOut(1f);
            FadeGameToBlack(1f);
            yield return new WaitForSeconds(1f);

            //Select new hole theme and move to next hole
            holeThemeHandler.SelectNewTheme();
        }
        else
        {
            //Game End
            Debug.Log("Game Complete");

            //Show Scoreboard
            FadeScoreboardIn(1f);
            GameObject.Find("ClickContText").GetComponent<AlphaPulse>().enabled = true;
            GameObject.Find("ClickContText").GetComponent<TextMeshProUGUI>().text = "Click anywhere to return to the main menu.";
            //Wait for click input to continue
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

            //Return to main menu
            ResetGameData();
            SceneManager.LoadScene("MainMenu");
        }

        //Destroy the hole end collider that we deparented in order to prevent the coroutine from being stopped too soon.
        if (this.GetComponentInChildren<HoleCheck>())
        {
            Destroy(this.GetComponentInChildren<HoleCheck>().gameObject);
        }
        yield return null;
    }

    //Determine the player's score on the hole relative to it's par
    private string DetermineHoleResult()
    {
        string result = "";
        int strokes = gameData.currentStrokes;
        int par = gameData.currentHolePar;

        if(strokes == 1)
        {
            result = "Hole In One!";
        }
        else if(strokes == par - 4)
        {
            result = "Condor!";
        } else if (strokes == par - 3)
        {
            result = "Albatross!";
        } else if (strokes == par - 2)
        {
            result = "Eagle!";
        } else if (strokes == par - 1)
        {
            result = "Birdie!";
        } else if (strokes == par)
        {
            result = "Par!";
        } else if (strokes == par + 1)
        {
            result = "Bogey!";
        } else if (strokes == par + 2)
        {
            result = "Double Bogey!";
        } else if (strokes == par + 3)
        {
            result = "Triple Bogey!";
        } else
        {
            bool isUnderPar;
            int score = (strokes - par);

            isUnderPar = (score < 0) ? true : false;

            if (!isUnderPar)
            {
                result = ("+" + score.ToString());
            }

        }

        return result;
    }

    //Update the game's UI with the relevant game data
    public void UpdateGameUI()
    {
        holeNoText.text = gameData.currentHoleNo.ToString();
        holeParText.text = gameData.currentHolePar.ToString();
        holeStrokesText.text = gameData.currentStrokes.ToString();
    }

    //Disable scoreboard parallax when not visible to prevent unnecessary Update calls.
    public void ToggleScoreboardParallaxEnable()
    {
        var sBoardContPar = FindObjectOfType<MenuParallax>();
        bool sBoardActive = sBoardContPar.enabled;
        sBoardContPar.enabled = (!sBoardActive);
    }

    //Updates the scoreboard at the end of the hole
    public void UpdateScoreboard()
    {     
        if (gameData.gameMode.Equals("Endless"))
        {
            //Slightly convoluted solution to the endless mode running over the 18 holes
            //Basically checks if we've passed the 18 hole mark and increments the counter of how many times we've passed a multiple of 18 
            //This helps in calculating where in the holepars/scores list we should start to display from
            var holeNo = gameData.currentHoleNo;
            if (holeNo % 18 == 1 && holeNo != 1) gameData.scoreboardResets ++;
            for (int i = 0; i < sBoardScores.Count; i++)
            {
                sBoardHoleNos[i].text = "-";
                if (gameData.scoreboardResets > 0)
                {
                    if((gameData.holeScores.Count - (gameData.scoreboardResets * 18)) > i)
                    {
                        sBoardHoleNos[i].text = (((gameData.scoreboardResets * 18) + i + 1)).ToString();
                        sBoardPars[i].text = gameData.holePars[(gameData.scoreboardResets * 18) + i].ToString();
                        sBoardScores[i].text = gameData.holeScores[(gameData.scoreboardResets * 18) + i].ToString();
                    } 
                }
                else
                {
                    if(gameData.holeScores.Count > i)
                    {
                        sBoardHoleNos[i].text = (i+1).ToString();
                        sBoardPars[i].text = gameData.holePars[i].ToString();
                        sBoardScores[i].text = gameData.holeScores[i].ToString();
                    }
                }
            }
        } else
        {
            for (int i = 0; i < sBoardScores.Count; i++)
            {
                if (gameData.holeScores.Count > i)
                {
                    sBoardScores[i].text = gameData.holeScores[i].ToString();
                }
            }

            for (int i = 0; i < sBoardPars.Count; i++)
            {
                if (gameData.holePars.Count > i)
                {
                    sBoardPars[i].text = gameData.holePars[i].ToString();
                }
            }
        }

        GameObject.FindGameObjectWithTag("ParTotalText").GetComponent<TextMeshProUGUI>().text = gameData.totalPar.ToString();
        GameObject.FindGameObjectWithTag("ScoreTotalText").GetComponent<TextMeshProUGUI>().text = gameData.totalStrokes.ToString();
    }

    //Destroys and instantiates a new gameData prefab
    public void ResetGameData()
    {
        Destroy(gameData.gameObject);
        Instantiate(gameDataPrefab);
    }

    //UI Alpha Fade Methods
    private void FadeGameIn(float time)
    {
        fadeImage.DOFade(0f, time);
    }

    private void FadeGameOut(float time)
    {
        fadeImage.DOFade(.75f, time);
    }

    private void FadeGameToBlack(float time)
    {
        fadeImage.DOFade(1f, time);
    }

    private void FadeGameUIIn(float time)
    {
        float alphaval = 200 / 255f;
        holeIcon.DOFade(alphaval, time);

        foreach (TextMeshProUGUI t in gameUIElements)
        {
            t.DOFade(alphaval, time);
        }
    }

    private void FadeGameUIOut(float time)
    {
        holeIcon.DOFade(0f, time);

        foreach (TextMeshProUGUI t in gameUIElements)
        {
            t.DOFade(0f, time);
        }
    }

    private void FadeScoreboardOut(float time)
    {
        sBoardImage.DOFade(0f, time);

        foreach (TextMeshProUGUI t in sBoardElements)
        {
            t.DOFade(0f, time);
        }
    }

    private void FadeScoreboardIn(float time)
    {
        ToggleScoreboardParallaxEnable();
        UpdateScoreboard();

        sBoardImage.DOFade(1f, time);

        foreach (TextMeshProUGUI t in sBoardElements)
        {
            t.DOFade(1f, time);
        }
    }

    private void ShowHoleResult(string result, float time)
    {
        holeResultText.text = result;
        holeResultText.DOFade(1f, time);
    }

    private void FadeResultOut(float time)
    {
        holeResultText.DOFade(0f, time);
    }
}
