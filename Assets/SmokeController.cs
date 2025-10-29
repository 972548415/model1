// SmokeController.cs
// SmokeController.cs
using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class SmokeController : MonoBehaviour
{
    [Header("Visual Effect 设置")]
    [Tooltip("Visual Effect 组件，用于烟雾效果")]
    public VisualEffect smokeVisualEffect;

    [Header("烟雾控制")]
    [Tooltip("烟雾持续时间")]
    public float smokeDuration = 5.0f;

    [Tooltip("淡入时间")]
    public float fadeInDuration = 1.0f;

    [Tooltip("淡出时间")]
    public float fadeOutDuration = 2.0f;

    [Header("VFX 参数控制")]
    [Tooltip("控制烟雾生成速率的参数名")]
    public string spawnRateParameter = "SpawnRate";

    [Tooltip("控制烟雾大小的参数名")]
    public string sizeParameter = "Size";

    [Tooltip("控制烟雾颜色的参数名")]
    public string colorParameter = "Color";

    private float originalSpawnRate;
    private float originalSize;
    private Color originalColor;
    private Coroutine smokeCoroutine;

    private void Start()
    {
        // 初始化 Visual Effect
        InitializeVisualEffect();

        // 注册事件监听
        EgoEventCenter.AddListener(EventType.MainVehicleReachPosition, OnMainVehicleReachPosition);
        EgoEventCenter.AddListener(EventType.SmokeStartGenerate, OnSmokeStart);
        EgoEventCenter.AddListener(EventType.SmokeDisappear, OnSmokeDisappear);

        Debug.Log("SmokeController 已启动，使用 Visual Effect Graph");
    }

    private void OnDestroy()
    {
        // 移除事件监听
        EgoEventCenter.RemoveListener(EventType.MainVehicleReachPosition, OnMainVehicleReachPosition);
        EgoEventCenter.RemoveListener(EventType.SmokeStartGenerate, OnSmokeStart);
        EgoEventCenter.RemoveListener(EventType.SmokeDisappear, OnSmokeDisappear);

        // 停止所有协程
        if (smokeCoroutine != null)
        {
            StopCoroutine(smokeCoroutine);
        }
    }

    /// <summary>
    /// 初始化 Visual Effect 组件
    /// </summary>
    private void InitializeVisualEffect()
    {
        // 如果没有指定 Visual Effect，尝试获取
        if (smokeVisualEffect == null)
        {
            smokeVisualEffect = GetComponent<VisualEffect>();

            if (smokeVisualEffect == null)
            {
                Debug.LogError("未找到 Visual Effect 组件！请确保对象上有 Visual Effect 组件。");
                return;
            }
        }

        // 保存原始参数值
        if (smokeVisualEffect.HasFloat(spawnRateParameter))
        {
            originalSpawnRate = smokeVisualEffect.GetFloat(spawnRateParameter);
        }
        else
        {
            Debug.LogWarning($"未找到参数: {spawnRateParameter}，将使用默认控制方式");
        }

        if (smokeVisualEffect.HasFloat(sizeParameter))
        {
            originalSize = smokeVisualEffect.GetFloat(sizeParameter);
        }

        if (smokeVisualEffect.HasVector4(colorParameter))
        {
            originalColor = smokeVisualEffect.GetVector4(colorParameter);
        }

        // 初始状态：停止烟雾
        StopSmokeImmediate();

        Debug.Log($"SmokeController 初始化完成，Visual Effect: {smokeVisualEffect != null}");
    }

    /// <summary>
    /// 主车辆到达位置事件处理
    /// </summary>
    private void OnMainVehicleReachPosition()
    {
        Debug.Log("收到主车辆到达事件，开始生成烟雾");
        StartSmoke();
    }

    /// <summary>
    /// 烟雾开始生成事件处理
    /// </summary>
    private void OnSmokeStart()
    {
        Debug.Log("收到烟雾开始事件");
        StartSmoke();
    }

    /// <summary>
    /// 烟雾消失事件处理
    /// </summary>
    private void OnSmokeDisappear()
    {
        Debug.Log("收到烟雾消失事件");
        StopSmoke();
    }

    /// <summary>
    /// 开始生成烟雾
    /// </summary>
    public void StartSmoke()
    {
        if (smokeVisualEffect == null)
        {
            Debug.LogError("Visual Effect 未设置！");
            return;
        }

        // 如果已经有烟雾协程在运行，先停止它
        if (smokeCoroutine != null)
        {
            StopCoroutine(smokeCoroutine);
        }

        smokeCoroutine = StartCoroutine(SmokeSequence());
    }

    /// <summary>
    /// 停止烟雾
    /// </summary>
    public void StopSmoke()
    {
        if (smokeCoroutine != null)
        {
            StopCoroutine(smokeCoroutine);
        }

        // 开始淡出
        smokeCoroutine = StartCoroutine(FadeOutSmoke());
    }

    /// <summary>
    /// 立即停止烟雾（无淡出效果）
    /// </summary>
    public void StopSmokeImmediate()
    {
        if (smokeCoroutine != null)
        {
            StopCoroutine(smokeCoroutine);
            smokeCoroutine = null;
        }

        if (smokeVisualEffect != null)
        {
            // 停止生成新粒子
            if (smokeVisualEffect.HasFloat(spawnRateParameter))
            {
                smokeVisualEffect.SetFloat(spawnRateParameter, 0f);
            }

            // 停止效果播放
            smokeVisualEffect.Stop();

            Debug.Log("烟雾立即停止");
        }
    }

    /// <summary>
    /// 烟雾序列控制
    /// </summary>
    private IEnumerator SmokeSequence()
    {
        Debug.Log("开始烟雾序列");

        // 淡入烟雾
        yield return StartCoroutine(FadeInSmoke());

        // 保持烟雾状态
        yield return new WaitForSeconds(smokeDuration);

        // 淡出烟雾
        yield return StartCoroutine(FadeOutSmoke());

        smokeCoroutine = null;
    }

    /// <summary>
    /// 淡入烟雾
    /// </summary>
    private IEnumerator FadeInSmoke()
    {
        Debug.Log("烟雾淡入开始");

        if (smokeVisualEffect.HasFloat(spawnRateParameter))
        {
            // 开始播放效果
            smokeVisualEffect.Play();

            float timer = 0f;
            while (timer < fadeInDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / fadeInDuration;

                // 淡入生成速率
                float currentSpawnRate = Mathf.Lerp(0f, originalSpawnRate, progress);
                smokeVisualEffect.SetFloat(spawnRateParameter, currentSpawnRate);

                yield return null;
            }

            // 确保最终值正确
            smokeVisualEffect.SetFloat(spawnRateParameter, originalSpawnRate);
        }
        else
        {
            // 如果没有找到参数，直接播放
            smokeVisualEffect.Play();
        }

        Debug.Log("烟雾淡入完成");
    }

    /// <summary>
    /// 淡出烟雾
    /// </summary>
    private IEnumerator FadeOutSmoke()
    {
        Debug.Log("烟雾淡出开始");

        if (smokeVisualEffect.HasFloat(spawnRateParameter))
        {
            float startSpawnRate = smokeVisualEffect.GetFloat(spawnRateParameter);
            float timer = 0f;

            while (timer < fadeOutDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / fadeOutDuration;

                // 淡出生成速率
                float currentSpawnRate = Mathf.Lerp(startSpawnRate, 0f, progress);
                smokeVisualEffect.SetFloat(spawnRateParameter, currentSpawnRate);

                yield return null;
            }

            // 确保最终停止生成
            smokeVisualEffect.SetFloat(spawnRateParameter, 0f);
        }

        // 停止效果
        smokeVisualEffect.Stop();

        smokeCoroutine = null;
        Debug.Log("烟雾淡出完成");
    }

    /// <summary>
    /// 设置烟雾强度
    /// </summary>
    /// <param name="intensity">强度 (0-1)</param>
    public void SetSmokeIntensity(float intensity)
    {
        if (smokeVisualEffect == null) return;

        intensity = Mathf.Clamp01(intensity);

        if (smokeVisualEffect.HasFloat(spawnRateParameter))
        {
            smokeVisualEffect.SetFloat(spawnRateParameter, originalSpawnRate * intensity);
        }

        if (smokeVisualEffect.HasFloat(sizeParameter))
        {
            smokeVisualEffect.SetFloat(sizeParameter, originalSize * intensity);
        }
    }

    /// <summary>
    /// 设置烟雾颜色
    /// </summary>
    /// <param name="color">目标颜色</param>
    public void SetSmokeColor(Color color)
    {
        if (smokeVisualEffect != null && smokeVisualEffect.HasVector4(colorParameter))
        {
            smokeVisualEffect.SetVector4(colorParameter, color);
        }
    }

    /// <summary>
    /// 重置烟雾参数到原始值
    /// </summary>
    public void ResetSmokeParameters()
    {
        if (smokeVisualEffect == null) return;

        if (smokeVisualEffect.HasFloat(spawnRateParameter))
        {
            smokeVisualEffect.SetFloat(spawnRateParameter, originalSpawnRate);
        }

        if (smokeVisualEffect.HasFloat(sizeParameter))
        {
            smokeVisualEffect.SetFloat(sizeParameter, originalSize);
        }

        if (smokeVisualEffect.HasVector4(colorParameter))
        {
            smokeVisualEffect.SetVector4(colorParameter, originalColor);
        }
    }

    /// <summary>
    /// 手动测试开始烟雾（用于编辑器测试）
    /// </summary>
    [ContextMenu("测试开始烟雾")]
    public void TestStartSmoke()
    {
        Debug.Log("手动测试开始烟雾");
        StartSmoke();
    }

    /// <summary>
    /// 手动测试停止烟雾（用于编辑器测试）
    /// </summary>
    [ContextMenu("测试停止烟雾")]
    public void TestStopSmoke()
    {
        Debug.Log("手动测试停止烟雾");
        StopSmoke();
    }

    /// <summary>
    /// 检查烟雾是否正在播放
    /// </summary>
    public bool IsSmokePlaying()
    {
        return smokeVisualEffect != null && smokeVisualEffect.aliveParticleCount > 0;
    }

    // 在编辑器中显示调试信息
    private void OnGUI()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            GUILayout.BeginArea(new Rect(10, 160, 300, 120));
            GUILayout.BeginVertical("Box");

            GUILayout.Label("烟雾控制器状态 (VFX):");
            GUILayout.Label($"Visual Effect: {(smokeVisualEffect != null ? "已设置" : "未设置")}");
            GUILayout.Label($"活跃粒子: {(smokeVisualEffect != null ? smokeVisualEffect.aliveParticleCount.ToString() : "N/A")}");
            GUILayout.Label($"状态: {(IsSmokePlaying() ? "播放中" : "停止")}");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
#endif
    }
}