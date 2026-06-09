using UnityEngine;
using TMPro;

public class ScoreTrigger : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    private static int score = 0; // 🔥 static (shared across all)

    private bool collected = false;

    private void Start()
    {
        UpdateScore();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !collected)
        {
            collected = true;

            score += 1;
            UpdateScore();

            Destroy(gameObject);
        }
    }

    void UpdateScore()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }
}