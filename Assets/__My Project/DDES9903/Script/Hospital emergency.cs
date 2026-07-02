using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Hospitalemergency : MonoBehaviour
{
    [Header("Reference Settings")]
    public GameObject doctorGameObject;
    public Transform finalWaypoint;
    public Light directionalLight;

    [Header("Audio Settings")]
    public AudioSource emergencyAudioSource;
    public AudioClip sirenClip;

    [Header("Nurse Timing Settings")]
    public float nurseSpawnDelay = 3.0f;
    public Nursemove nurseAInstance;
    public Nursemove nurseBInstance;
    public float strobeInterval = 0.3f;

    private bool crisisTriggered = false;
    private bool doctorReachedFinal = false;
    private NavMeshAgent doctorAgent;
    private AudioSource doctorAudio;

    private void Start()
    {
        if (nurseAInstance != null) nurseAInstance.gameObject.SetActive(false);
        if (nurseBInstance != null) nurseBInstance.gameObject.SetActive(false);

        if (doctorGameObject != null)
        {
            doctorAgent = doctorGameObject.GetComponent<NavMeshAgent>();
            doctorAudio = doctorGameObject.GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (crisisTriggered || doctorGameObject == null || doctorAgent == null || doctorAudio == null || finalWaypoint == null) return;

        if (!doctorReachedFinal)
        {
            float distanceToFinal = Vector3.Distance(doctorAgent.transform.position, finalWaypoint.position);

            if (distanceToFinal <= (doctorAgent.stoppingDistance + 0.2f) && !doctorAgent.pathPending)
            {
                doctorReachedFinal = true;
            }
        }

        if (doctorReachedFinal && !crisisTriggered)
        {
            if (!doctorAudio.isPlaying)
            {
                crisisTriggered = true;
                StartCoroutine(ExecuteNarrativeChain());
            }
        }
    }

    private IEnumerator ExecuteNarrativeChain()
    {
        if (emergencyAudioSource != null && sirenClip != null)
        {
            emergencyAudioSource.clip = sirenClip;
            emergencyAudioSource.loop = true;
            emergencyAudioSource.Play();
        }

        StartCoroutine(AnimateLightStrobe());

        yield return new WaitForSeconds(nurseSpawnDelay);

        if (nurseAInstance != null) nurseAInstance.InitiateMovement();
        if (nurseBInstance != null) nurseBInstance.InitiateMovement();
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