// AudioController.cs
// AudioController.cs
using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour
{
    [Header("声音设置")]
    [Tooltip("触发时播放的声音片段")]
    public AudioClip triggerSound;

    [Tooltip("音频源组件，如果为空会自动获取")]
    public AudioSource audioSource;

    [Header("声音控制")]
    [Tooltip("声音音量")]
    [Range(0f, 1f)]
    public float volume = 1.0f;

    [Tooltip("是否循环播放")]
    public bool loop = false;

    [Tooltip("延迟播放时间（秒）")]
    public float delay = 0f;

    [Header("高级设置")]
    [Tooltip("是否在触发时停止当前播放的声音")]
    public bool stopCurrentOnTrigger = true;

    [Tooltip("淡入淡出时间（秒）")]
    public float fadeDuration = 1.0f;

    private float originalVolume;
    private Coroutine fadeCoroutine;

    private void Start()
    {
        // 初始化音频源
        InitializeAudioSource();

        // 注册事件监听
        EgoEventCenter.AddListener(EventType.MainVehicleReachPosition, OnMainVehicleReachPosition);
        EgoEventCenter.AddListener(EventType.PlayTriggerSound, OnPlayTriggerSound);

        Debug.Log("AudioController 已启动，等待事件触发");
    }

    private void OnDestroy()
    {
        // 移除事件监听
        EgoEventCenter.RemoveListener(EventType.MainVehicleReachPosition, OnMainVehicleReachPosition);
        EgoEventCenter.RemoveListener(EventType.PlayTriggerSound, OnPlayTriggerSound);

        // 停止所有协程
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }

    /// <summary>
    /// 初始化音频源组件
    /// </summary>
    private void InitializeAudioSource()
    {
        // 如果没有指定音频源，尝试获取
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();

            // 如果还没有，就创建一个
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("自动添加了 AudioSource 组件");
            }
        }

        // 设置音频源参数
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.playOnAwake = false;

        originalVolume = volume;

        Debug.Log($"AudioController 初始化完成，音频源: {audioSource != null}");
    }

    /// <summary>
    /// 主车辆到达位置事件处理
    /// </summary>
    private void OnMainVehicleReachPosition()
    {
        Debug.Log("收到主车辆到达事件，准备播放声音");
        PlayTriggerSound();
    }

    /// <summary>
    /// 播放触发声音事件处理
    /// </summary>
    private void OnPlayTriggerSound()
    {
        Debug.Log("收到播放声音事件");
        PlayTriggerSound();
    }

    /// <summary>
    /// 播放触发声音
    /// </summary>
    public void PlayTriggerSound()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource 未设置！");
            return;
        }

        if (triggerSound == null)
        {
            Debug.LogError("TriggerSound 未设置！请分配音频片段。");
            return;
        }

        // 如果有延迟，使用协程延迟播放
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
    /// 立即播放声音
    /// </summary>
    private void PlaySoundImmediate()
    {
        if (stopCurrentOnTrigger && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.clip = triggerSound;
        audioSource.volume = volume;

        // 如果有淡入效果
        if (fadeDuration > 0)
        {
            fadeCoroutine = StartCoroutine(FadeInAndPlay());
        }
        else
        {
            audioSource.Play();
            Debug.Log($"开始播放声音: {triggerSound.name}, 长度: {triggerSound.length}秒");
        }
    }

    /// <summary>
    /// 延迟播放声音
    /// </summary>
    private IEnumerator PlaySoundWithDelay()
    {
        Debug.Log($"等待 {delay} 秒后播放声音");
        yield return new WaitForSeconds(delay);
        PlaySoundImmediate();
    }

    /// <summary>
    /// 淡入并播放声音
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
        Debug.Log($"声音淡入完成，开始播放: {triggerSound.name}");
    }

    /// <summary>
    /// 停止播放声音
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
                Debug.Log("声音已停止");
            }
        }
    }

    /// <summary>
    /// 淡出并停止声音
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
        Debug.Log("声音淡出完成");
    }

    /// <summary>
    /// 暂停声音
    /// </summary>
    public void PauseSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("声音已暂停");
        }
    }

    /// <summary>
    /// 继续播放声音
    /// </summary>
    public void ResumeSound()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.UnPause();
            Debug.Log("声音已继续播放");
        }
    }

    /// <summary>
    /// 设置音量
    /// </summary>
    /// <param name="newVolume">新音量 (0-1)</param>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// 检查是否正在播放声音
    /// </summary>
    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    /// <summary>
    /// 手动触发声音（用于测试）
    /// </summary>
    [ContextMenu("测试播放声音")]
    public void TestPlaySound()
    {
        Debug.Log("手动测试播放声音");
        PlayTriggerSound();
    }

    /// <summary>
    /// 手动停止声音（用于测试）
    /// </summary>
    [ContextMenu("测试停止声音")]
    public void TestStopSound()
    {
        Debug.Log("手动测试停止声音");
        StopSound();
    }

    // 在编辑器中显示调试信息
    private void OnGUI()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.BeginVertical("Box");

            GUILayout.Label("音频控制器状态:");
            GUILayout.Label($"音频源: {(audioSource != null ? "已设置" : "未设置")}");
            GUILayout.Label($"音频片段: {(triggerSound != null ? triggerSound.name : "未设置")}");
            GUILayout.Label($"播放状态: {(IsPlaying() ? "播放中" : "停止")}");
            GUILayout.Label($"音量: {volume}");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
#endif
    }
}
