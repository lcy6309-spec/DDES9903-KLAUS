using UnityEngine;
using System.Collections;

public class HospitalCrisisController : MonoBehaviour
{
    [Header("Environment")]
    public Light directionalLight;

    [Header("Patient Monitor")]
    public AudioSource monitorAudioSource;

    [Header("Treatment UI")]
    public GameObject treatmentButton;

    [Header("Emergency Audio")]
    public AudioClip flatlineClip;
    public AudioClip shockClip;
    public AudioClip normalHeartbeatClip;

    [Header("Leader Dialogue")]
    public AudioSource leaderAudioSource;
    public AudioClip leaderPraiseClip;

    private bool isCrisisActive = false;
    private Coroutine lightFlashCoroutine;
    private Color originalLightColor;

    private void Start()
    {
        if (directionalLight != null)
        {
            originalLightColor = directionalLight.color;
        }

        if (treatmentButton != null)
        {
            treatmentButton.SetActive(false);
        }

        // Trigger emergency after 2 seconds
        Invoke(nameof(StartCrisis), 2f);
    }

    //====================================
    // Start Emergency
    //====================================
    public void StartCrisis()
    {
        if (isCrisisActive)
            return;

        isCrisisActive = true;

        if (treatmentButton != null)
        {
            treatmentButton.SetActive(true);
        }

        if (monitorAudioSource != null && flatlineClip != null)
        {
            monitorAudioSource.clip = flatlineClip;
            monitorAudioSource.loop = true;
            monitorAudioSource.Play();
        }

        if (lightFlashCoroutine != null)
        {
            StopCoroutine(lightFlashCoroutine);
        }

        lightFlashCoroutine = StartCoroutine(FlashEmergencyLight());
    }

    //====================================
    // Flash Emergency Light
    //====================================
    private IEnumerator FlashEmergencyLight()
    {
        bool isRed = false;

        while (isCrisisActive)
        {
            if (directionalLight != null)
            {
                directionalLight.color = isRed ? originalLightColor : Color.red;
                isRed = !isRed;
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    //====================================
    // Called by Button
    //====================================
    public void TreatPatient()
    {
        if (!isCrisisActive)
            return;

        if (treatmentButton != null)
        {
            treatmentButton.SetActive(false);
        }

        if (monitorAudioSource != null)
        {
            monitorAudioSource.Stop();

            if (shockClip != null)
            {
                monitorAudioSource.PlayOneShot(shockClip);
            }
        }

        StartCoroutine(FinishTreatment());
    }

    //====================================
    // Finish Treatment
    //====================================
    private IEnumerator FinishTreatment()
    {
        // Wait for shock sound
        if (shockClip != null)
        {
            yield return new WaitForSeconds(shockClip.length);
        }

        // Stop emergency
        isCrisisActive = false;

        if (lightFlashCoroutine != null)
        {
            StopCoroutine(lightFlashCoroutine);
            lightFlashCoroutine = null;
        }

        // Restore light
        RestoreLighting();

        // Small delay
        yield return new WaitForSeconds(0.5f);

        // Resume heartbeat
        if (monitorAudioSource != null && normalHeartbeatClip != null)
        {
            monitorAudioSource.clip = normalHeartbeatClip;
            monitorAudioSource.loop = true;
            monitorAudioSource.Play();
        }

        // Wait a moment before the leader speaks
        yield return new WaitForSeconds(1f);

        // Leader praises the player
        if (leaderAudioSource != null && leaderPraiseClip != null)
        {
            leaderAudioSource.PlayOneShot(leaderPraiseClip);
        }
    }

    //====================================
    // Restore Lighting
    //====================================
    private void RestoreLighting()
    {
        if (directionalLight != null)
        {
            directionalLight.color = originalLightColor;
        }
    }
}