// Modified PlayerPickManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameManager;

public class PlayerPickManager : MonoBehaviour
{
    void Start()
    {
        // Ensure GameManager exists when this scene loads
        if (GameManager.Instance == null)
        {
            Instantiate(Resources.Load<GameObject>("GameManager")); // Create from prefab
            // OR: new GameObject("GameManager").AddComponent<GameManager>();
        }
    }

    public void Select1Player() => SetGameModeAndContinue(GameMode.SinglePlayer);
    public void Select1PlayerWithAI() => SetGameModeAndContinue(GameMode.SinglePlayerWithAI);
    public void Select2Players() => SetGameModeAndContinue(GameMode.TwoPlayers);
    public void Select2PlayersWithAI() => SetGameModeAndContinue(GameMode.TwoPlayersWithAI);

    private void SetGameModeAndContinue(GameMode mode)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager still not initialized!");
            return;
        }

        GameManager.Instance.CurrentGameMode = mode;
        GameManager.Instance.LoadScene("MainMenu", "PlayerPickScene");
    }
}