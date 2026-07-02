using UnityEngine;
using System.Collections;

public class HospitalNurseTrigger : MonoBehaviour
{
    public enum TriggerTarget { DetectDoctor, DetectPlayer }

    [Header("Trigger Settings")]
    public TriggerTarget targetRole;
    public string playerTag = "Player";
    public string doctorTag = "Doctor";
    public float detectDistance = 2.5f;

    [Header("Audio Settings")]
    public AudioSource nurseAudioSource;
    public AudioClip nurseClip;

    [Header("Sequence Settings (For Nurse A Only)")]
    public AudioSource doctorAudioSource;
    public AudioClip doctorReplyClip;
    public Transform operatingRoomTarget;

    private bool hasTriggered = false;
    private GameObject cachedDoctor;

    private void Start()
    {
        if (nurseAudioSource != null) nurseAudioSource.playOnAwake = false;
        if (doctorAudioSource != null) doctorAudioSource.playOnAwake = false;

        if (targetRole == TriggerTarget.DetectDoctor)
        {
            cachedDoctor = GameObject.FindGameObjectWithTag(doctorTag);
        }
    }

    private void Update()
    {
        if (hasTriggered || targetRole != TriggerTarget.DetectDoctor) return;

        if (cachedDoctor != null)
        {
            float distance = Vector3.Distance(transform.position, cachedDoctor.transform.position);
            if (distance <= detectDistance)
            {
                StartCoroutine(TimelineSequenceRoutine());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || targetRole != TriggerTarget.DetectPlayer) return;

        if (other.CompareTag(playerTag))
        {
            hasTriggered = true;
            if (nurseAudioSource != null && nurseClip != null)
            {
                nurseAudioSource.clip = nurseClip;
                nurseAudioSource.Play();
            }
        }
    }

    private IEnumerator TimelineSequenceRoutine()
    {
        hasTriggered = true;

        // 1. 强制关闭护士自己身上的旧移动脚本，把控制权拿过来
        MonoBehaviour nurseMove = GetComponent("NurseMove") as MonoBehaviour;
        if (nurseMove != null) nurseMove.enabled = false;

        // 2. 播放护士语音
        if (nurseAudioSource != null && nurseClip != null)
        {
            nurseAudioSource.clip = nurseClip;
            nurseAudioSource.Play();
            yield return new WaitForSeconds(nurseClip.length + 0.5f);
        }

        // 3. 强制关闭医生身上的旧移动脚本（不管它叫什么名字，全部禁用）
        if (cachedDoctor != null)
        {
            MonoBehaviour[] docScripts = cachedDoctor.GetComponents<MonoBehaviour>();
            foreach (var script in docScripts)
            {
                if (script != this && !(script is Animator))
                {
                    script.enabled = false;
                }
            }
        }

        // 4. 播放医生回话
        if (doctorAudioSource != null && doctorReplyClip != null)
        {
            doctorAudioSource.clip = doctorReplyClip;
            doctorAudioSource.Play();
            yield return new WaitForSeconds(doctorReplyClip.length + 0.5f);
        }

        // 5. 走去手术室
        if (operatingRoomTarget != null)
        {
            StartCoroutine(MoveToOperatingRoomRoutine());
        }
    }

    private IEnumerator MoveToOperatingRoomRoutine()
    {
        float speed = 3.0f;

        if (cachedDoctor != null)
        {
            Animator docAnim = cachedDoctor.GetComponent<Animator>();
            if (docAnim != null) docAnim.SetBool("isWalking", true);
        }

        while (operatingRoomTarget != null)
        {
            Vector3 nursePos = transform.position;
            Vector3 targetPos = operatingRoomTarget.position;

            transform.position = Vector3.MoveTowards(nursePos, targetPos, speed * Time.deltaTime);

            if (cachedDoctor != null)
            {
                Vector3 docPos = cachedDoctor.transform.position;
                Vector3 followPos = transform.position - transform.forward * 1.5f;
                cachedDoctor.transform.position = Vector3.MoveTowards(docPos, followPos, (speed * 0.95f) * Time.deltaTime);

                Vector3 lookDirection = targetPos - docPos;
                if (lookDirection != Vector3.zero)
                {
                    cachedDoctor.transform.rotation = Quaternion.Slerp(cachedDoctor.transform.rotation, Quaternion.LookRotation(lookDirection), 5f * Time.deltaTime);
                }
            }

            Vector3 lookDirNurse = targetPos - transform.position;
            if (lookDirNurse != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirNurse), 5f * Time.deltaTime);
            }

            if (Vector3.Distance(transform.position, targetPos) < 0.2f)
            {
                if (cachedDoctor != null)
                {
                    Animator docAnim = cachedDoctor.GetComponent<Animator>();
                    if (docAnim != null) docAnim.SetBool("isWalking", false);
                }
                break;
            }

            yield return null;
        }
    }
}