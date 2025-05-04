using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public static List<GameObject> Players = new List<GameObject>();


    [Header("Settings")]
    public GameObject player1TankPrefab;
    public GameObject player2TankPrefab;
    public GameObject aiTankPrefab;

    [Header("Debug")]
    public bool allowSpawnOverlap = false;

    private List<SpawnPoint> allSpawnPoints = new List<SpawnPoint>();
    private bool hasInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SceneManager.SetActiveScene(gameObject.scene);

        Players.Clear();
        InitializeSpawning();

    }

    public static void RegisterPlayer(GameObject player)
    {
        if (!Players.Contains(player))
            Players.Add(player);
    }

    public static void UnregisterPlayer(GameObject player)
    {
        Players.Remove(player);
    }

    public void InitializeSpawning()
    {
        if (hasInitialized) return;

        // Only find spawn points in THIS scene
        allSpawnPoints = new List<SpawnPoint>();
        foreach (SpawnPoint sp in FindObjectsOfType<SpawnPoint>())
        {
            if (sp.gameObject.scene == gameObject.scene)
            {
                allSpawnPoints.Add(sp);
            }
        }

        // Ensure GameManager exists
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        StartGameMode(GameManager.Instance.CurrentGameMode);
        hasInitialized = true;
    }

    private void StartGameMode(GameManager.GameMode currentGameMode)
    {
        if (currentGameMode == GameManager.GameMode.SinglePlayer)
        {
            SpawnPlayer(1, player1TankPrefab);
        }
        else if (currentGameMode == GameManager.GameMode.SinglePlayerWithAI)
        {
            SpawnPlayer(1, player1TankPrefab);
            SpawnAI();
        }
        else if (currentGameMode == GameManager.GameMode.TwoPlayers)
        {
            SpawnPlayer(1, player1TankPrefab);
            SpawnPlayer(2, player2TankPrefab);
        }
        else if (currentGameMode == GameManager.GameMode.TwoPlayersWithAI)
        {
            SpawnPlayer(1, player1TankPrefab);
            SpawnPlayer(2, player2TankPrefab);
            SpawnAI();
        }
    }

    void SpawnPlayer(int playerNumber, GameObject prefab)
    {
        var spawn = GetValidSpawnPoint(SpawnPoint.SpawnType.Player1 + (playerNumber - 1));
        if (spawn != null)
        {
            Instantiate(prefab, spawn.transform.position, spawn.transform.rotation);
            //SceneManager.MoveGameObjectToScene(prefab, SceneManager.GetSceneByName(gameObject.scene.name));

            MarkSpawnUsed(spawn);
        }
    }

    void SpawnAI()
    {
        var spawn = GetValidSpawnPoint(SpawnPoint.SpawnType.AI);
        if (spawn != null)
        {
            Instantiate(aiTankPrefab, spawn.transform.position, spawn.transform.rotation);
            //SceneManager.MoveGameObjectToScene(aiTankPrefab, SceneManager.GetSceneByName(gameObject.scene.name));
            MarkSpawnUsed(spawn);
        }

    }

    SpawnPoint GetValidSpawnPoint(SpawnPoint.SpawnType type)
    {
        // Find all spawn points matching type
        var validSpawns = allSpawnPoints.FindAll(sp =>
            sp.spawnType == type || sp.spawnType == SpawnPoint.SpawnType.Any);

        // Check for empty space if needed
        if (!allowSpawnOverlap)
        {
            validSpawns.RemoveAll(sp =>
                Physics2D.OverlapCircle(sp.transform.position, sp.spawnRadius));
        }

        return validSpawns.Count > 0 ?
            validSpawns[Random.Range(0, validSpawns.Count)] : null;
    }

    void MarkSpawnUsed(SpawnPoint spawn)
    {
        if (spawn.spawnType == SpawnPoint.SpawnType.Any)
        {
            // Disable spawn point after use if it was "Any" type
            spawn.gameObject.SetActive(false);
        }
    }

}