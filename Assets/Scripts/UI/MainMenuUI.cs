using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{

    public string currentGameScene;

    public void OnStartButton()
    {
        FindObjectOfType<GameManager>().LoadScene(GetRandomMap(), "MainMenu");
    }

    public void OnExitButton()
    {
        FindObjectOfType<GameManager>().Quit();
    }

    public void OnCreditsButton()
    {
        //do something
    }

    public string GetRandomMap()
    {
        // Generate random number between 1 and NumberOfMaps (inclusive)
        int randomMapNumber = Random.Range(1, GameManager.NumberOfMaps + 1);

        currentGameScene = $"GameScene {randomMapNumber}";
        // Return the formatted string
        return currentGameScene;
    }

}
