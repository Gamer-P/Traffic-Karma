using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public AudioSource backgroundMusic; // drag your music source
    public Slider volumeSlider;

    void Start()
    {
        // Set slider value to current volume
        if (backgroundMusic != null)
        {
            volumeSlider.value = backgroundMusic.volume;
        }

        // Add listener
        volumeSlider.onValueChanged.AddListener(SetVolume);  
    }

    public void SetVolume(float value)
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = value;
        }
    }
}
