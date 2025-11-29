using UnityEngine;

public class RemoteSoundTrigger : MonoBehaviour
{
    [Header("=== 触发设置 ===")]
    [Tooltip("要检测哪个对象的位置（通常是玩家或主车）")]
    public Transform triggerPosition;

    [Tooltip("触发距离")]
    public float triggerDistance = 3f;

    [Tooltip("是否只触发一次")]
    public bool playOnce = true;

    [Header("=== 声音设置 ===")]
    [Tooltip("要播放的声音文件")]
    public AudioClip soundClip;

    [Tooltip("声音播放的位置（如果为空，会在世界原点播放）")]
    public Transform soundSourcePosition;

    [Tooltip("声音音量")]
    [Range(0f, 5f)]
    public float soundVolume = 5f;

    [Tooltip("是否启用3D音效")]
    public bool enable3DSound = true;

    [Header("=== 3D音效设置 ===")]
    public float minDistance = 1f;
    public float maxDistance = 50f;

    [Header("=== 调试选项 ===")]
    public bool showDebugInfo = true;
    public bool drawGizmos = true;

    // 私有变量
    private AudioSource remoteAudioSource;
    private bool hasPlayed = false;
    private GameObject soundEmitterObject;

    void Start()
    {
        InitializeSoundSystem();

        // 如果没有指定触发位置，尝试自动找到玩家
        if (triggerPosition == null)
        {
            triggerPosition = FindPlayerTransform();
        }
    }

    void Update()
    {
        // 检查是否应该触发
        CheckForTrigger();
    }

    /// <summary>
    /// 初始化声音系统
    /// </summary>
    void InitializeSoundSystem()
    {
        // 创建或获取声音发射器
        if (soundSourcePosition != null)
        {
            // 使用指定的声音发射位置
            soundEmitterObject = soundSourcePosition.gameObject;
        }
        else
        {
            // 创建新的声音发射器对象
            soundEmitterObject = new GameObject("RemoteSoundEmitter");
            soundEmitterObject.transform.position = Vector3.zero;
        }

        // 确保有AudioSource组件
        remoteAudioSource = soundEmitterObject.GetComponent<AudioSource>();
        if (remoteAudioSource == null)
        {
            remoteAudioSource = soundEmitterObject.AddComponent<AudioSource>();
        }

        // 配置AudioSource
        ConfigureAudioSource();

        if (showDebugInfo)
        {
            Debug.Log($"声音系统初始化完成 - 发射器位置: {soundEmitterObject.transform.position}");
        }
    }

    /// <summary>
    /// 配置AudioSource组件
    /// </summary>
    void ConfigureAudioSource()
    {
        remoteAudioSource.clip = soundClip;
        remoteAudioSource.volume = soundVolume;
        remoteAudioSource.playOnAwake = false;

        if (enable3DSound)
        {
            remoteAudioSource.spatialBlend = 1.0f; // 完全3D音效
            remoteAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            remoteAudioSource.minDistance = minDistance;
            remoteAudioSource.maxDistance = maxDistance;
            remoteAudioSource.dopplerLevel = 0f;
        }
        else
        {
            remoteAudioSource.spatialBlend = 0f; // 完全2D音效
        }
    }

    /// <summary>
    /// 检查触发条件
    /// </summary>
    void CheckForTrigger()
    {
        // 检查条件：是否已经播放过、是否有触发位置、声音是否配置正确
        if ((playOnce && hasPlayed) || triggerPosition == null || soundClip == null)
            return;

        // 计算距离
        float distance = Vector3.Distance(transform.position, triggerPosition.position);

        // 检查是否进入触发范围
        if (distance <= triggerDistance)
        {
            PlayRemoteSound();
        }
    }

    /// <summary>
    /// 播放远程声音
    /// </summary>
    void PlayRemoteSound()
    {
        if (remoteAudioSource != null && !remoteAudioSource.isPlaying)
        {
            remoteAudioSource.Play();
            hasPlayed = true;

            if (showDebugInfo)
            {
                Debug.Log($"触发远程声音播放！");
                Debug.Log($"触发位置: {transform.position}");
                Debug.Log($"声音发射位置: {soundEmitterObject.transform.position}");
                Debug.Log($"目标对象位置: {triggerPosition.position}");
                Debug.Log($"播放声音: {soundClip.name}");
            }

            // 如果只需要播放一次，可以禁用更新检查
            if (playOnce)
            {
                enabled = false;
            }
        }
    }

    /// <summary>
    /// 自动查找玩家变换组件
    /// </summary>
    Transform FindPlayerTransform()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (showDebugInfo)
                Debug.Log("自动找到玩家对象作为触发目标");
            return player.transform;
        }

        // 如果没有找到玩家，尝试其他常见标签
        player = GameObject.FindGameObjectWithTag("MainCar");
        if (player != null)
        {
            if (showDebugInfo)
                Debug.Log("自动找到主车对象作为触发目标");
            return player.transform;
        }

        Debug.LogWarning("没有找到触发目标，请在Inspector中手动分配");
        return null;
    }

    /// <summary>
    /// 手动触发声音（可以从其他脚本调用）
    /// </summary>
    public void ManualTriggerSound()
    {
        PlayRemoteSound();
    }

    /// <summary>
    /// 重置触发状态
    /// </summary>
    public void ResetTrigger()
    {
        hasPlayed = false;
        enabled = true;

        if (showDebugInfo)
            Debug.Log("触发状态已重置");
    }

    /// <summary>
    /// 更改声音发射位置
    /// </summary>
    public void ChangeSoundEmissionPoint(Transform newEmissionPoint)
    {
        soundSourcePosition = newEmissionPoint;
        InitializeSoundSystem(); // 重新初始化
    }

    // ========== 调试可视化 ==========
    void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // 绘制触发范围
        Gizmos.color = hasPlayed ? Color.gray : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);

        // 绘制触发位置图标
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position, Vector3.one * 0.3f);

        // 绘制声音发射位置
        if (soundEmitterObject != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(soundEmitterObject.transform.position, 0.5f);

            // 绘制从触发位置到声音位置的连线
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, soundEmitterObject.transform.position);
        }
        else if (soundSourcePosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(soundSourcePosition.position, 0.5f);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, soundSourcePosition.position);
        }

        // 绘制到目标对象的连线
        if (triggerPosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, triggerPosition.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        // 选中时显示更详细的信息
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);

#if UNITY_EDITOR
        // 显示距离信息
        if (triggerPosition != null)
        {
            float distance = Vector3.Distance(transform.position, triggerPosition.position);
            string status = hasPlayed ? "已触发" : "等待触发";
            string info = $"距离: {distance:F1}\n状态: {status}";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2, info);
        }
#endif
    }
}