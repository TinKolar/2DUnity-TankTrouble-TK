using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    public static int NumberOfMaps = 3;


    public static Dictionary<string, int> playerScores = new Dictionary<string, int>();
    public int winPointsRequired = 3;

    public enum GameMode
    {
        SinglePlayer,
        SinglePlayerWithAI,
        TwoPlayers,
        TwoPlayersWithAI
    }
    public GameMode CurrentGameMode { get; set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetGameMode(GameMode mode)
    {
        // Store the selected game mode
        CurrentGameMode = mode;

        // Load the game scene
        LoadScene("MainMenu", "PlayerPickScene"); // Assuming you have these scenes
    }

    void Start()
    {
        LoadScene("PlayerPickScene"/*,unloadSceneName:"UnloadScene"*/);
    }

    public void LoadScene(string sceneName, string unloadSceneName = null)
    {
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        if (unloadSceneName != null)
        {
            SceneManager.UnloadSceneAsync(unloadSceneName);
        }
    }

    public void HandleRoundEnd(GameObject winningTank, Scene scene)
    {
        // Award point if there's a winner
        if (winningTank != null)
        {
            if (!playerScores.ContainsKey(winningTank.name))
            {
                playerScores[winningTank.name] = 0;
            }
            playerScores[winningTank.name]++;

        }

        // Check for match win
        foreach (var score in playerScores)
        {
            if (score.Value >= winPointsRequired)
            {
                //DO wining celebration

                GameFinishManager.UnregisterTank(winningTank);
                playerScores = new Dictionary<string, int>();
                LoadScene("MainMenu", scene.name);
                return;
            }
        }
        GameFinishManager.UnregisterTank(winningTank);
        LoadNextRound("WaitingScene", scene.name);
    }

    private void LoadNextRound(string v, string name)
    {
        LoadScene("WaitingScene");
        SceneManager.UnloadSceneAsync(name).completed += OnSceneUnloaded;

    }

    private void OnSceneUnloaded(AsyncOperation operation)
    {
        LoadScene(GetRandomMap(), "WaitingScene");

    }

    public string GetRandomMap()
    {
        // Generate random number between 1 and NumberOfMaps (inclusive)
        int randomMapNumber = Random.Range(1, GameManager.NumberOfMaps + 1);

        string currentGameScene = $"GameScene {randomMapNumber}";
        // Return the formatted string
        return currentGameScene;
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); //works only when we make a build for a game
#endif
    }
}


