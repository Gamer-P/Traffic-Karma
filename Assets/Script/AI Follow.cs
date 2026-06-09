using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AIFollow : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent agent;

    public float avoidDistance = 8f;
    public float avoidStrength = 6f;


    private bool isGameOver = false;
    private GameManager gameManager;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        gameManager = FindAnyObjectByType<GameManager>();

    }

    void Update()
    {
        Vector3 target = player.position;

        target += GetAvoidanceVector();

        agent.SetDestination(target);

        RotateTowards(agent.velocity);
    }

    Vector3 GetAvoidanceVector()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        RaycastHit hit;
        Vector3 avoid = Vector3.zero;

        if (Physics.Raycast(origin, transform.forward, out hit, avoidDistance))
        {
            if (!hit.collider.isTrigger)
            {
                float strength = (avoidDistance - hit.distance) / avoidDistance;
                avoid += hit.normal * strength * avoidStrength;
            }
        }

        if (Physics.Raycast(origin, transform.TransformDirection(-0.7f, 0, 1), out hit, avoidDistance))
        {
            avoid += Vector3.right * avoidStrength;
        }

        if (Physics.Raycast(origin, transform.TransformDirection(0.7f, 0, 1), out hit, avoidDistance))
        {
            avoid += Vector3.left * avoidStrength;
        }

        return avoid;
    }

    void RotateTowards(Vector3 velocity)
    {
        if (velocity.magnitude > 0.1f)
        {
            Quaternion rot = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 6f * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Police") && !isGameOver)
        {
            isGameOver = true;
            Debug.Log("Under Arrest");

            StartCoroutine(GameOverDelay());

        }
    }

    IEnumerator GameOverDelay()
    {
        yield return new WaitForSeconds(3f);
        gameManager.GameOver();

        Debug.Log("Game Over!");
    }
}