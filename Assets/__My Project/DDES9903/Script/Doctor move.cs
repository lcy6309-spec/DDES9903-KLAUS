using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class NPCMultiStageGuide : MonoBehaviour
{
    [Header("Navigation Settings")]
    public Transform[] waypoints;
    public float stopDistance = 0.5f;
    public int currentWaypointIndex = 0;

    [Header("Audio Settings")]
    public AudioClip[] waypointVoices;

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private bool isTalking = false;
    private int lastSpokenWaypoint = -1;
    private bool hasTriggeredFirstMeet = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();

        if (agent != null)
        {
            agent.stoppingDistance = stopDistance;
            agent.isStopped = true;
        }
    }

    void Update()
    {
        if (!hasTriggeredFirstMeet || isTalking || waypoints.Length == 0 || agent == null) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            int arrivedIndex = currentWaypointIndex - 1;

            if (arrivedIndex >= 0 && arrivedIndex < waypoints.Length && arrivedIndex != lastSpokenWaypoint)
            {
                if (arrivedIndex < waypointVoices.Length && waypointVoices[arrivedIndex] != null)
                {
                    lastSpokenWaypoint = arrivedIndex;
                    StartCoroutine(PauseAndNarrate(arrivedIndex));
                }
                else
                {
                    ProceedToNextBeat();
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggeredFirstMeet)
        {
            hasTriggeredFirstMeet = true;
            StartCoroutine(FirstMeetSequence());
        }
    }

    IEnumerator FirstMeetSequence()
    {
        isTalking = true;

        if (audioSource != null && waypointVoices.Length > 0 && waypointVoices[0] != null)
        {
            audioSource.clip = waypointVoices[0];
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);
        }
        else
        {
            yield return new WaitForSeconds(2.0f);
        }

        isTalking = false;
        lastSpokenWaypoint = 0;
        currentWaypointIndex = 1;

        if (agent != null)
        {
            agent.isStopped = false;
        }

        ProceedToNextBeat();
    }

    IEnumerator PauseAndNarrate(int voiceIndex)
    {
        isTalking = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (audioSource != null && waypointVoices[voiceIndex] != null)
        {
            audioSource.clip = waypointVoices[voiceIndex];
            audioSource.Play();

            yield return new WaitForSeconds(audioSource.clip.length);
        }

        agent.isStopped = false;
        isTalking = false;

        ProceedToNextBeat();
    }

    void ProceedToNextBeat()
    {
        if (currentWaypointIndex >= waypoints.Length)
        {
            agent.isStopped = true;
        }
        else
        {
            MoveToNextWaypoint();
        }
    }

    void MoveToNextWaypoint()
    {
        if (currentWaypointIndex < waypoints.Length && agent != null)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
            currentWaypointIndex++;
        }
    }
}