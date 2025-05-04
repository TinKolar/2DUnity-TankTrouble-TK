using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public enum SpawnType { Player1, Player2, AI, Any }
    public SpawnType spawnType = SpawnType.Any;
    public float spawnRadius = 1f;

    void OnDrawGizmos()
    {
        Gizmos.color = GetSpawnColor();
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, "spawn.png");
    }

    private Color GetSpawnColor()
    {
        return spawnType switch
        {
            SpawnType.Player1 => Color.green,
            SpawnType.Player2 => Color.cyan,
            SpawnType.AI => Color.red,
            _ => Color.yellow
        };
    }
}