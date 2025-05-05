using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{

    private string currentGameScene;

    [SerializeField] private Canvas ControlsCanvas;
    [SerializeField] private Canvas MainMenuCanvas;

    public TextMeshProUGUI locked;

    private void Start()
    {
        if(GameManager.NumberOfMaps==4)
        {
            locked.text = "Unlocked :)";
            locked.color = Color.green;
        }
    }

    public void OnStartButton()
    {
        FindObjectOfType<GameManager>().LoadScene(GetRandomMap(), "MainMenu");
    }

    public void OnExitButton()
    {
        FindObjectOfType<GameManager>().Quit();
    }

    public void OnControlsButton()
    {
        ControlsCanvas.gameObject.SetActive(true);
        MainMenuCanvas.gameObject.SetActive(false);
    }

    public void OnBackButton()
    {
        MainMenuCanvas.gameObject.SetActive(true);
        ControlsCanvas.gameObject.SetActive(false);
    }

    public string GetRandomMap()
    {
        // Generate random number between 1 and NumberOfMaps (inclusive)
        int randomMapNumber = Random.Range(1, GameManager.NumberOfMaps + 1);

        currentGameScene = $"GameScene {randomMapNumber}";
        // Return the formatted string
        return currentGameScene;
    }

    public void OnSpecialMapButton()
    {
        if(GameManager.NumberOfMaps==4)
        FindObjectOfType<GameManager>().LoadScene("GameScene 4", "MainMenu");
        else
        {
            locked.text = "Locked";
        }

    }

}
