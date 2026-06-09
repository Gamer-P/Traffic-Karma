using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderTrigger : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;

            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}