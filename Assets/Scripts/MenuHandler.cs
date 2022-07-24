using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using static HSD_Utils;
using static GameStateManager;

public class MenuHandler : MonoBehaviour
{
    bool showPause;

    GameObject player;
    GameObject UIObj;

    PartyManager _pm;
    RunManager _rm;

    bool inNewGame;
    public string currSeed;
    
    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.Find("/Player");
        UIObj = GameObject.Find("/UI");
        UIObj.SetActive(false);
        _pm = player.GetComponent<PartyManager>();
        _rm = GameObject.Find("/GameControl").GetComponent<RunManager>();

        showPause = true;
        TogglePause();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && CurrGamestate != GameState.Dead) {
            TogglePause();
        }
        if(CurrGamestate == GameState.Dead) {
            transform.Find("DeathScreen").gameObject.SetActive(true);
        }
        if(Input.GetKeyDown(KeyCode.Return) && inNewGame) {
            StartNewGame();
        }
    }

    public void TogglePause() {
        showPause = !showPause;
        if (showPause) {
            Time.timeScale = 0f;
            transform.Find("PauseMenu").gameObject.SetActive(true);
            ChangeState(GameState.Paused);
        }
        else {
            transform.Find("PauseMenu").gameObject.SetActive(false);
            if (!inNewGame) {
                Time.timeScale = 1f;
                ChangeState(GameState.Running);
            }
        }
    }

    public void CloseGame() {
        Debug.Log("CLOSING GAME");
        Application.Quit();
    }

    public void ShowNewGameMenu(bool afterWin = false) {
        transform.Find("DeathScreen").gameObject.SetActive(false);

        Transform newGameMenu = transform.Find("NewGame");
        newGameMenu.gameObject.SetActive(true);

        IEnumerator SelectField() {
            yield return null;
            newGameMenu.Find("Window/SeedField").gameObject.GetComponent<TMP_InputField>().Select();
        }
        StartCoroutine(SelectField());

        inNewGame = true;
        ChangeState(GameState.Paused);
        Time.timeScale = 0f;
        if (afterWin) {
            newGameMenu.Find("Window/Label").GetComponent<TextMeshProUGUI>().text = "Congrats!\nNew Game";
        }
    }

    public void UpdateSeed(string s) { currSeed = s; }

    public void StartNewGame() {
        transform.Find("NewGame").gameObject.SetActive(false);
        inNewGame = false;

        int sentSeed;
        currSeed = currSeed.TrimStart('-');
        if (currSeed.Equals("")) {
            sentSeed = -1;
        }
        else if(currSeed.Length < 9) {
            currSeed = "100000000".Substring(0, 9 - currSeed.Length) + currSeed;
            sentSeed = int.Parse(currSeed);
        }
        else { sentSeed = int.Parse(currSeed); }

        _rm.UnloadRoom();
        foreach(Transform child in GameObject.Find("/Data/Rooms").transform) {
            Destroy(child.gameObject);
        }
        SetSeed(sentSeed, true);
        _rm.RUN_SEED = (int)LAST_KNOWN_SEED;
        _rm.StartRun();
        _pm.coinCount = 0;
        _pm.FullHealParty();
        _pm.RandomizePartyWeapons();
        player.transform.position = new Vector3(5.5f, -4.5f, 0f);
        Time.timeScale = 1f;
        ChangeState(GameState.Running);
        UIObj.SetActive(true);
    }
}
