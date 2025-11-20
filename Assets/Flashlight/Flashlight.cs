using UnityEngine;

public class Flashlight : MonoBehaviour
{
    private Light _light;
    private AudioSource _audioSource;

    void Start()
    {
        _light = GetComponentInChildren<Light>();
        _audioSource = GetComponent<AudioSource>();
        _light.enabled = false;
    }

    public void LightOn() 
    { 
        _audioSource.Play();
        _light.enabled = true;
    }

    public void LightOff()
    {
        _audioSource.Play();
        _light.enabled = false;
    }
}
