using System.Collections;
using UnityEngine;

public class HospitalEmergencyInitializer : MonoBehaviour
{
    [Header("Global Illumination")]
    public Light directionalLight;
    public float strobeInterval = 0.3f;

    [Header("System Audio Source")]
    public AudioSource emergencyAudioSource;
    public AudioClip sirenClip;

    private void Start()
    {
        if (emergencyAudioSource != null && sirenClip != null)
        {
            emergencyAudioSource.clip = sirenClip;
            emergencyAudioSource.loop = true;
            emergencyAudioSource.Play();
        }

        StartCoroutine(AnimateLightStrobe());
    }

    private IEnumerator AnimateLightStrobe()
    {
        while (true)
        {
            if (directionalLight != null)
            {
                directionalLight.color = Color.red;
                yield return new WaitForSeconds(strobeInterval);
                directionalLight.color = Color.white;
                yield return new WaitForSeconds(strobeInterval);
            }
            else
            {
                yield return null;
            }
        }
    }
}