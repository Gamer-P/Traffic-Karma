using UnityEngine;
using System.Collections;

public class Collectables : MonoBehaviour
{
    public float effectDuration = 5f;

    [Header("UI")]
    public GameObject uncontrolText;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        CarMechanics car = other.GetComponentInParent<CarMechanics>();

        if (car != null)
        {
            // 🔥 Start coroutine on player (important)
            car.StartCoroutine(UncontrolEffect(car));
        }

        // destroy collectable
        Destroy(gameObject);
    }

    IEnumerator UncontrolEffect(CarMechanics car)
    {
        // 🔥 SHOW TEXT
        if (uncontrolText != null)
            uncontrolText.SetActive(true);

        // 🔥 ENABLE REVERSE CONTROL
        car.reverseControls = true;

        yield return new WaitForSeconds(effectDuration);

        // 🔥 RESET
        car.reverseControls = false;

        // 🔥 HIDE TEXT
        if (uncontrolText != null)
            uncontrolText.SetActive(false);
    }
}