using UnityEngine;
using System.Collections.Generic;
using UnityEditor.SearchService;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Reflection;

public class GameFinishManager : MonoBehaviour
{
    [Header("Settings")]
    public float checkInterval = 5f;

    private static List<GameObject> livingTanks = new List<GameObject>();
    private float checkTimer;


    void Update()
    {
        
            checkTimer += Time.deltaTime;
            if (checkTimer >= checkInterval)
            {
                checkTimer = 0f;
                CheckLivingTanks();
            }
    }

    public static void RegisterTank(GameObject tank)
    {
        if (!livingTanks.Contains(tank))
            livingTanks.Add(tank);
    }

    public static void UnregisterTank(GameObject tank)
    {
        if (livingTanks.Contains(tank))
            livingTanks.Remove(tank);
    }

    private void CheckLivingTanks()
    {

        if (livingTanks.Count <= 1 && GameManager.Instance.CurrentGameMode != GameManager.GameMode.SinglePlayer) // 0 or 1 tank left
        {

            GameObject winner = livingTanks.Count == 1 ? livingTanks[0] : null;
            UnityEngine.SceneManagement.Scene scene = SceneManager.GetActiveScene();
            GameManager.Instance.HandleRoundEnd(winner,scene);
        }
        else if (livingTanks.Count == 0)

        {
            UnityEngine.SceneManagement.Scene scene = SceneManager.GetActiveScene();
            GameManager.Instance.HandleRoundEnd(null, scene);
        }
    }


}