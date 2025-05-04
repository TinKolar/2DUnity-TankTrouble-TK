using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    [SerializeField] private Canvas PauseCanvas;
    [SerializeField] private Canvas GameCanvas;
    [SerializeField] public TextMeshProUGUI scores;

    public GameObject p1;
    public GameObject p2;
    public GameObject ai;
    private string clone = "(Clone)";


    public void Update()
    {
        scores.text = GetTextScores();
    }

    private string GetTextScores()
    {
        GameManager.GameMode gm = GameManager.Instance.CurrentGameMode;

        EnsurePlayerInScore(p1.name);
        EnsurePlayerInScore(ai.name);
        EnsurePlayerInScore(p2.name);

        switch (gm)
        {
            case GameManager.GameMode.SinglePlayer:
                return "Sandbox";
            case GameManager.GameMode.SinglePlayerWithAI:
                return $"{p1.name}({GameManager.playerScores[p1.name + clone]})\t{ai.name}({GameManager.playerScores[ai.name + clone]})";
            case GameManager.GameMode.TwoPlayers:
                return $"{p1.name} ( {GameManager.playerScores[p1.name + clone]})\t{p2.name}({GameManager.playerScores[p2.name + clone]})";
            case GameManager.GameMode.TwoPlayersWithAI:
                return $"{p1.name}({GameManager.playerScores[p1.name + clone]})\t{p2.name}({GameManager.playerScores[p2.name + clone]})\t{ai.name}({GameManager.playerScores[ai.name + clone]})";
            default:
                return "Something went wrong!";
        }
    }

    private void EnsurePlayerInScore(string name)
    {
        if (!GameManager.playerScores.ContainsKey(name + clone))
            GameManager.playerScores[name + clone] = 0;
    }

    public void OnResumeButton()
    {
        PauseCanvas.gameObject.SetActive(false);
    }

    public void OnPauseButton()
    {
        PauseCanvas.gameObject.SetActive(true);
    }

    public void OnSettingsButton()
    {

    }

    public void OnMainMenuButton()
    {

        PauseCanvas.gameObject.SetActive(false);
        FindObjectOfType<GameManager>().LoadScene("MainMenu", gameObject.scene.name);

    }

}
