using UnityEngine;

public class CarSpawning : MonoBehaviour
{
    [SerializeField] private GameObject policePrefab;
    [SerializeField] private Transform player;
    [SerializeField] private float spawnDistance = 5f;

    private bool hasSpawned = false;

    void Update()
    {
        if (hasSpawned || player == null || policePrefab == null)
            return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= spawnDistance)
        {
            SpawnPolice();
        }
    }

    void SpawnPolice()
    {
        // 🔥 Face player while spawning
        Quaternion rot = Quaternion.LookRotation(player.position - transform.position);

        Instantiate(policePrefab, transform.position, rot);

        hasSpawned = true;

        Destroy(gameObject); // optional

        Debug.Log("Police Spawned at: " + transform.position);
    }
}