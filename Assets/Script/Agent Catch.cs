using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AgentCatch : MonoBehaviour
{
    public Transform player;
    [SerializeField] private GameManager gameManager;

    private NavMeshAgent agent;
    private bool isGameOver = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (isGameOver) return;

        // Check if agent reached player
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Extra safety (optional but good)
            if (Vector3.Distance(transform.position, player.position) <= agent.stoppingDistance + 1f)
            {
                isGameOver = true;
                agent.isStopped = true;

                gameManager.GameOver();
            }
        }
    }
}
