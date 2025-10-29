// AudioController.cs
// AudioController.cs
using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour
{
    [Header("��������")]
    [Tooltip("����ʱ���ŵ�����Ƭ��")]
    public AudioClip triggerSound;

    [Tooltip("��ƵԴ��������Ϊ�ջ��Զ���ȡ")]
    public AudioSource audioSource;

    [Header("��������")]
    [Tooltip("��������")]
    [Range(0f, 1f)]
    public float volume = 1.0f;

    [Tooltip("�Ƿ�ѭ������")]
    public bool loop = false;

    [Tooltip("�ӳٲ���ʱ�䣨�룩")]
    public float delay = 0f;

    [Header("�߼�����")]
    [Tooltip("�Ƿ��ڴ���ʱֹͣ��ǰ���ŵ�����")]
    public bool stopCurrentOnTrigger = true;

    [Tooltip("���뵭��ʱ�䣨�룩")]
    public float fadeDuration = 1.0f;

    private float originalVolume;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        // ��ʼ����ƵԴ
        InitializeAudioSource();

        // ע���¼�����
        EgoEventCenter.AddListener(EventType.MainVehicleReachPosition, OnMainVehicleReachPosition);
        EgoEventCenter.AddListener(EventType.PlayTriggerSound, OnPlayTriggerSound);

        Debug.Log("AudioController ���������ȴ��¼�����");
    }

    private void OnDestroy()
    {
        // �Ƴ��¼�����
        EgoEventCenter.RemoveListener(EventType.MainVehicleReachPosition, OnMainVehicleReachPosition);
        EgoEventCenter.RemoveListener(EventType.PlayTriggerSound, OnPlayTriggerSound);

        // ֹͣ����Э��
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }

    /// <summary>
    /// ��ʼ����ƵԴ���
    /// </summary>
    private void InitializeAudioSource()
    {
        // ���û��ָ����ƵԴ�����Ի�ȡ
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();

            // �����û�У��ʹ���һ��
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("�Զ������ AudioSource ���");
            }
        }

        // ������ƵԴ����
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.playOnAwake = false;

        originalVolume = volume;

        Debug.Log($"AudioController ��ʼ����ɣ���ƵԴ: {audioSource != null}");
    }

    /// <summary>
    /// ����������λ���¼�����
    /// </summary>
    private void OnMainVehicleReachPosition()
    {
        Debug.Log("�յ������������¼���׼����������");
        PlayTriggerSound();
    }

    /// <summary>
    /// ���Ŵ��������¼�����
    /// </summary>
    private void OnPlayTriggerSound()
    {
        Debug.Log("�յ����������¼�");
        PlayTriggerSound();
    }

    /// <summary>
    /// ���Ŵ�������
    /// </summary>
    public void PlayTriggerSound()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource δ���ã�");
            return;
        }

        if (triggerSound == null)
        {
            Debug.LogError("TriggerSound δ���ã��������ƵƬ�Ρ�");
            return;
        }

        // ������ӳ٣�ʹ��Э���ӳٲ���
        if (delay > 0)
        {
            StartCoroutine(PlaySoundWithDelay());
        }
        else
        {
            PlaySoundImmediate();
        }
    }

    /// <summary>
    /// ������������
    /// </summary>
    private void PlaySoundImmediate()
    {
        if (stopCurrentOnTrigger && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.clip = triggerSound;
        audioSource.volume = volume;

        // ����е���Ч��
        if (fadeDuration > 0)
        {
            fadeCoroutine = StartCoroutine(FadeInAndPlay());
        }
        else
        {
            audioSource.Play();
            Debug.Log($"��ʼ��������: {triggerSound.name}, ����: {triggerSound.length}��");
        }
    }

    /// <summary>
    /// �ӳٲ�������
    /// </summary>
    private IEnumerator PlaySoundWithDelay()
    {
        Debug.Log($"�ȴ� {delay} ��󲥷�����");
        yield return new WaitForSeconds(delay);
        PlaySoundImmediate();
    }

    /// <summary>
    /// ���벢��������
    /// </summary>
    private IEnumerator FadeInAndPlay()
    {
        audioSource.volume = 0f;
        audioSource.Play();

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, timer / fadeDuration);
            yield return null;
        }

        audioSource.volume = volume;
        Debug.Log($"����������ɣ���ʼ����: {triggerSound.name}");
    }

    /// <summary>
    /// ֹͣ��������
    /// </summary>
    public void StopSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            if (fadeDuration > 0)
            {
                fadeCoroutine = StartCoroutine(FadeOutAndStop());
            }
            else
            {
                audioSource.Stop();
                Debug.Log("������ֹͣ");
            }
        }
    }

    /// <summary>
    /// ������ֹͣ����
    /// </summary>
    private IEnumerator FadeOutAndStop()
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = originalVolume;
        Debug.Log("�����������");
    }

    /// <summary>
    /// ��ͣ����
    /// </summary>
    public void PauseSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("��������ͣ");
        }
    }

    /// <summary>
    /// ������������
    /// </summary>
    public void ResumeSound()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.UnPause();
            Debug.Log("�����Ѽ�������");
        }
    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="newVolume">������ (0-1)</param>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// ����Ƿ����ڲ�������
    /// </summary>
    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    /// <summary>
    /// �ֶ��������������ڲ��ԣ�
    /// </summary>
    [ContextMenu("���Բ�������")]
    public void TestPlaySound()
    {
        Debug.Log("�ֶ����Բ�������");
        PlayTriggerSound();
    }

    /// <summary>
    /// �ֶ�ֹͣ���������ڲ��ԣ�
    /// </summary>
    [ContextMenu("����ֹͣ����")]
    public void TestStopSound()
    {
        Debug.Log("�ֶ�����ֹͣ����");
        StopSound();
    }

    // �ڱ༭������ʾ������Ϣ
    private void OnGUI()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.BeginVertical("Box");

            GUILayout.Label("��Ƶ������״̬:");
            GUILayout.Label($"��ƵԴ: {(audioSource != null ? "������" : "δ����")}");
            GUILayout.Label($"��ƵƬ��: {(triggerSound != null ? triggerSound.name : "δ����")}");
            GUILayout.Label($"����״̬: {(IsPlaying() ? "������" : "ֹͣ")}");
            GUILayout.Label($"����: {volume}");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
#endif
    }
}
