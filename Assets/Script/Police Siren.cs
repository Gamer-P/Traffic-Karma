using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class PoliceSiren : MonoBehaviour
{
    public Transform player;
    public float maxDistance = 50f;

    [Header("Fade Settings")]
    public float fadeSpeed = 2f; // how fast volume changes

    private AudioSource audioSource;
    private NavMeshAgent agent;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();

        audioSource.loop = true;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.volume = 0f;

        audioSource.Play();
    }

    void Update()
    {
        if (player == null || agent == null) return;

        // 🔹 Distance-based volume
        float distance = Vector3.Distance(transform.position, player.position);
        float minVolume = 0.1f; // 🔥 minimum sound level (adjust if needed)

        float distanceFactor = Mathf.Clamp01(1 - (distance / maxDistance));
        float distanceVolume = Mathf.Lerp(minVolume, 1f, distanceFactor);

        // 🔹 Movement check (is chasing or stopped)
        bool isMoving = agent.velocity.magnitude > 0.5f;

        float targetVolume = isMoving ? distanceVolume : 0f;

        // 🔥 Smooth fade
        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * fadeSpeed);
    }
}