// SmokeController.cs
// SmokeController.cs
using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class SmokeController : MonoBehaviour
{
    [Header("Visual Effect ����")]
    [Tooltip("Visual Effect �������������Ч��")]
    public VisualEffect smokeVisualEffect;

    [Header("�������")]
    [Tooltip("�������ʱ��")]
    public float smokeDuration = 5.0f;

    [Tooltip("����ʱ��")]
    public float fadeInDuration = 1.0f;

    [Tooltip("����ʱ��")]
    public float fadeOutDuration = 2.0f;

    [Header("VFX ��������")]
    [Tooltip("���������������ʵĲ�����")]
    public string spawnRateParameter = "SpawnRate";

    [Tooltip("���������С�Ĳ�����")]
    public string sizeParameter = "Size";

    [Tooltip("����������ɫ�Ĳ�����")]
    public string colorParameter = "Color";

    private float originalSpawnRate;
    private float originalSize;
    private Color originalColor;
    private Coroutine smokeCoroutine;

    private void Start()
    {
        // ��ʼ�� Visual Effect
        InitializeVisualEffect();

        // ע���¼�����
        EgoEventCenter.AddListener(EventType.MainVehicleReachPosition, OnMainVehicleReachPosition);
        EgoEventCenter.AddListener(EventType.SmokeStartGenerate, OnSmokeStart);
        EgoEventCenter.AddListener(EventType.SmokeDisappear, OnSmokeDisappear);

        Debug.Log("SmokeController ��������ʹ�� Visual Effect Graph");
    }

    private void OnDestroy()
    {
        // �Ƴ��¼�����
        EgoEventCenter.RemoveListener(EventType.MainVehicleReachPosition, OnMainVehicleReachPosition);
        EgoEventCenter.RemoveListener(EventType.SmokeStartGenerate, OnSmokeStart);
        EgoEventCenter.RemoveListener(EventType.SmokeDisappear, OnSmokeDisappear);

        // ֹͣ����Э��
        if (smokeCoroutine != null)
        {
            StopCoroutine(smokeCoroutine);
        }
    }

    /// <summary>
    /// ��ʼ�� Visual Effect ���
    /// </summary>
    private void InitializeVisualEffect()
    {
        // ���û��ָ�� Visual Effect�����Ի�ȡ
        if (smokeVisualEffect == null)
        {
            smokeVisualEffect = GetComponent<VisualEffect>();

            if (smokeVisualEffect == null)
            {
                Debug.LogError("δ�ҵ� Visual Effect �������ȷ���������� Visual Effect �����");
                return;
            }
        }

        // ����ԭʼ����ֵ
        if (smokeVisualEffect.HasFloat(spawnRateParameter))
        {
            originalSpawnRate = smokeVisualEffect.GetFloat(spawnRateParameter);
        }
        else
        {
            Debug.LogWarning($"δ�ҵ�����: {spawnRateParameter}����ʹ��Ĭ�Ͽ��Ʒ�ʽ");
        }

        if (smokeVisualEffect.HasFloat(sizeParameter))
        {
            originalSize = smokeVisualEffect.GetFloat(sizeParameter);
        }

        if (smokeVisualEffect.HasVector4(colorParameter))
        {
            originalColor = smokeVisualEffect.GetVector4(colorParameter);
        }

        // ��ʼ״̬��ֹͣ����
        StopSmokeImmediate();

        Debug.Log($"SmokeController ��ʼ����ɣ�Visual Effect: {smokeVisualEffect != null}");
    }

    /// <summary>
    /// ����������λ���¼�����
    /// </summary>
    private void OnMainVehicleReachPosition()
    {
        Debug.Log("�յ������������¼�����ʼ��������");
        StartSmoke();
    }

    /// <summary>
    /// ����ʼ�����¼�����
    /// </summary>
    private void OnSmokeStart()
    {
        Debug.Log("�յ�����ʼ�¼�");
        StartSmoke();
    }

    /// <summary>
    /// ������ʧ�¼�����
    /// </summary>
    private void OnSmokeDisappear()
    {
        Debug.Log("�յ�������ʧ�¼�");
        StopSmoke();
    }

    /// <summary>
    /// ��ʼ��������
    /// </summary>
    public void StartSmoke()
    {
        if (smokeVisualEffect == null)
        {
            Debug.LogError("Visual Effect δ���ã�");
            return;
        }

        // ����Ѿ�������Э�������У���ֹͣ��
        if (smokeCoroutine != null)
        {
            StopCoroutine(smokeCoroutine);
        }

        smokeCoroutine = StartCoroutine(SmokeSequence());
    }

    /// <summary>
    /// ֹͣ����
    /// </summary>
    public void StopSmoke()
    {
        if (smokeCoroutine != null)
        {
            StopCoroutine(smokeCoroutine);
        }

        // ��ʼ����
        smokeCoroutine = StartCoroutine(FadeOutSmoke());
    }

    /// <summary>
    /// ����ֹͣ�����޵���Ч����
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
            // ֹͣ����������
            if (smokeVisualEffect.HasFloat(spawnRateParameter))
            {
                smokeVisualEffect.SetFloat(spawnRateParameter, 0f);
            }

            // ֹͣЧ������
            smokeVisualEffect.Stop();

            Debug.Log("��������ֹͣ");
        }
    }

    /// <summary>
    /// �������п���
    /// </summary>
    private IEnumerator SmokeSequence()
    {
        Debug.Log("��ʼ��������");

        // ��������
        yield return StartCoroutine(FadeInSmoke());

        // ��������״̬
        yield return new WaitForSeconds(smokeDuration);

        // ��������
        yield return StartCoroutine(FadeOutSmoke());

        smokeCoroutine = null;
    }

    /// <summary>
    /// ��������
    /// </summary>
    private IEnumerator FadeInSmoke()
    {
        Debug.Log("�����뿪ʼ");

        if (smokeVisualEffect.HasFloat(spawnRateParameter))
        {
            // ��ʼ����Ч��
            smokeVisualEffect.Play();

            float timer = 0f;
            while (timer < fadeInDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / fadeInDuration;

                // ������������
                float currentSpawnRate = Mathf.Lerp(0f, originalSpawnRate, progress);
                smokeVisualEffect.SetFloat(spawnRateParameter, currentSpawnRate);

                yield return null;
            }

            // ȷ������ֵ��ȷ
            smokeVisualEffect.SetFloat(spawnRateParameter, originalSpawnRate);
        }
        else
        {
            // ���û���ҵ�������ֱ�Ӳ���
            smokeVisualEffect.Play();
        }

        Debug.Log("���������");
    }

    /// <summary>
    /// ��������
    /// </summary>
    private IEnumerator FadeOutSmoke()
    {
        Debug.Log("��������ʼ");

        if (smokeVisualEffect.HasFloat(spawnRateParameter))
        {
            float startSpawnRate = smokeVisualEffect.GetFloat(spawnRateParameter);
            float timer = 0f;

            while (timer < fadeOutDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / fadeOutDuration;

                // ������������
                float currentSpawnRate = Mathf.Lerp(startSpawnRate, 0f, progress);
                smokeVisualEffect.SetFloat(spawnRateParameter, currentSpawnRate);

                yield return null;
            }

            // ȷ������ֹͣ����
            smokeVisualEffect.SetFloat(spawnRateParameter, 0f);
        }

        // ֹͣЧ��
        smokeVisualEffect.Stop();

        smokeCoroutine = null;
        Debug.Log("���������");
    }

    /// <summary>
    /// ��������ǿ��
    /// </summary>
    /// <param name="intensity">ǿ�� (0-1)</param>
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
    /// ����������ɫ
    /// </summary>
    /// <param name="color">Ŀ����ɫ</param>
    public void SetSmokeColor(Color color)
    {
        if (smokeVisualEffect != null && smokeVisualEffect.HasVector4(colorParameter))
        {
            smokeVisualEffect.SetVector4(colorParameter, color);
        }
    }

    /// <summary>
    /// �������������ԭʼֵ
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
    /// �ֶ����Կ�ʼ�������ڱ༭�����ԣ�
    /// </summary>
    [ContextMenu("���Կ�ʼ����")]
    public void TestStartSmoke()
    {
        Debug.Log("�ֶ����Կ�ʼ����");
        StartSmoke();
    }

    /// <summary>
    /// �ֶ�����ֹͣ�������ڱ༭�����ԣ�
    /// </summary>
    [ContextMenu("����ֹͣ����")]
    public void TestStopSmoke()
    {
        Debug.Log("�ֶ�����ֹͣ����");
        StopSmoke();
    }

    /// <summary>
    /// ��������Ƿ����ڲ���
    /// </summary>
    public bool IsSmokePlaying()
    {
        return smokeVisualEffect != null && smokeVisualEffect.aliveParticleCount > 0;
    }

    // �ڱ༭������ʾ������Ϣ
    private void OnGUI()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            GUILayout.BeginArea(new Rect(10, 160, 300, 120));
            GUILayout.BeginVertical("Box");

            GUILayout.Label("���������״̬ (VFX):");
            GUILayout.Label($"Visual Effect: {(smokeVisualEffect != null ? "������" : "δ����")}");
            GUILayout.Label($"��Ծ����: {(smokeVisualEffect != null ? smokeVisualEffect.aliveParticleCount.ToString() : "N/A")}");
            GUILayout.Label($"״̬: {(IsSmokePlaying() ? "������" : "ֹͣ")}");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
#endif
    }
}