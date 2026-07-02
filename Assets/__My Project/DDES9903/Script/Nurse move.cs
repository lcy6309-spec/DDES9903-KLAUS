using System.Collections;
using UnityEngine;

public class Nursemove : MonoBehaviour
{
    [Header("Nurse: Navigation Settings")]
    public Transform[] waypoints;
    public float moveSpeed = 2.0f;

    [Header("Nurse: Dialogue Settings")]
    public AudioSource nurseAudioSource;
    public AudioClip dialogueClip;

    private float lockedGroundY;

    private void Start()
    {
        lockedGroundY = transform.position.y;
    }

    public void InitiateMovement()
    {
        gameObject.SetActive(true);
        StartCoroutine(FollowPathSequence());
    }

    private IEnumerator FollowPathSequence()
    {
        if (waypoints == null || waypoints.Length == 0) yield break;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Vector3 startPos = transform.position;
            Vector3 endPos = new Vector3(waypoints[i].position.x, lockedGroundY, waypoints[i].position.z);

            float distance = Vector3.Distance(startPos, endPos);
            if (distance < 0.01f) continue;

            float progress = 0f;

            while (progress < 1.0f)
            {
                progress += Time.deltaTime * (moveSpeed / distance);

                Vector3 currentLerpPos = Vector3.Lerp(startPos, endPos, progress);
                transform.position = new Vector3(currentLerpPos.x, lockedGroundY, currentLerpPos.z);

                yield return null;
            }
        }

        if (nurseAudioSource != null && dialogueClip != null)
        {
            nurseAudioSource.clip = dialogueClip;
            nurseAudioSource.Play();
        }
    }
}