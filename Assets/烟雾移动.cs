// ZAxisMovementController.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZAxisMovementController : MonoBehaviour
{
    [Header("移动物体设置")]
    public List<GameObject> movingObjects = new List<GameObject>();
    public float moveSpeed = 5.0f;
    public bool movePositive = true; // true: 正Z方向, false: 负Z方向

    [Header("移动范围限制")]
    public bool useBoundary = false;
    public float maxZPosition = 50f;
    public float minZPosition = -50f;

    [Header("消失设置")]
    public bool fadeOnBoundary = false;
    public float fadeDuration = 2.0f;
    public bool destroyAfterFade = true;

    [Header("启动设置")]
    public float startMoveDelay = 0.5f;
    public float perObjectDelay = 0.1f;
    public bool autoStartOnTrigger = true;

    private List<Vector3> originalPositions = new List<Vector3>();
    private List<Renderer> objectRenderers = new List<Renderer>();
    private List<Material[]> originalMaterials = new List<Material[]>();
    private bool isMoving = false;
    private Coroutine moveCoroutine;

    void Start()
    {
        InitializeObjects();

        // 注册事件监听
        if (autoStartOnTrigger)
        {
            EgoEventCenter.AddListener(EventType.MainVehicleReachPosition, OnTriggerActivated);
            EgoEventCenter.AddListener(EventType.VehicleStartMoving, OnTriggerActivated);
        }

        Debug.Log($"Z轴移动控制器已启动，共管理 {movingObjects.Count} 个物体");
    }

    void OnDestroy()
    {
        if (autoStartOnTrigger)
        {
            EgoEventCenter.RemoveListener(EventType.MainVehicleReachPosition, OnTriggerActivated);
            EgoEventCenter.RemoveListener(EventType.VehicleStartMoving, OnTriggerActivated);
        }

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
    }

    void InitializeObjects()
    {
        originalPositions.Clear();
        objectRenderers.Clear();
        originalMaterials.Clear();

        foreach (var obj in movingObjects)
        {
            if (obj != null)
            {
                // 保存原始位置
                originalPositions.Add(obj.transform.position);

                // 获取渲染器和材质
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    objectRenderers.Add(renderer);
                    originalMaterials.Add(renderer.materials);
                }
                else
                {
                    objectRenderers.Add(null);
                    originalMaterials.Add(null);
                }

                Debug.Log($"初始化物体: {obj.name} 位置: {obj.transform.position}");
            }
        }
    }

    void OnTriggerActivated()
    {
        Debug.Log("收到触发事件，开始沿Z轴移动");
        StartZAxisMovement();
    }

    public void StartZAxisMovement()
    {
        if (isMoving) return;

        Debug.Log($"开始沿Z轴移动 {movingObjects.Count} 个物体");
        isMoving = true;

        // 开始移动协程
        moveCoroutine = StartCoroutine(MoveAllObjectsAlongZ());
    }

    public void StopZAxisMovement()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
        isMoving = false;
        Debug.Log("停止Z轴移动");
    }

    IEnumerator MoveAllObjectsAlongZ()
    {
        Debug.Log($"开始移动 {movingObjects.Count} 个物体沿Z轴，速度: {moveSpeed}");

        // 初始延迟
        yield return new WaitForSeconds(startMoveDelay);

        // 为每个物体启动移动协程
        for (int i = 0; i < movingObjects.Count; i++)
        {
            if (movingObjects[i] != null)
            {
                StartCoroutine(MoveSingleObjectAlongZ(i));
                yield return new WaitForSeconds(perObjectDelay);
            }
        }
    }

    IEnumerator MoveSingleObjectAlongZ(int objectIndex)
    {
        GameObject obj = movingObjects[objectIndex];
        if (obj == null) yield break;

        string objectName = obj.name;
        Debug.Log($"开始移动物体: {objectName} 沿Z轴");

        Vector3 startPosition = obj.transform.position;
        float direction = movePositive ? 1f : -1f;

        while (obj != null && IsWithinBoundary(obj.transform.position.z))
        {
            // 计算移动距离
            float moveDistance = moveSpeed * Time.deltaTime * direction;
            Vector3 newPosition = obj.transform.position + new Vector3(0, 0, moveDistance);

            // 应用移动
            obj.transform.position = newPosition;

            // 检查是否超出边界
            if (useBoundary && !IsWithinBoundary(newPosition.z))
            {
                HandleBoundaryReached(objectIndex);
                break;
            }

            yield return null;
        }

        if (obj != null)
        {
            Debug.Log($"物体 {objectName} 移动完成或到达边界");
        }
    }

    bool IsWithinBoundary(float currentZ)
    {
        if (!useBoundary) return true;

        return currentZ >= minZPosition && currentZ <= maxZPosition;
    }

    void HandleBoundaryReached(int objectIndex)
    {
        GameObject obj = movingObjects[objectIndex];
        if (obj == null) return;

        if (fadeOnBoundary)
        {
            StartCoroutine(FadeOutObject(objectIndex));
        }
        else if (destroyAfterFade)
        {
            Destroy(obj);
            Debug.Log($"销毁到达边界的物体: {obj.name}");
        }
        else
        {
            obj.SetActive(false);
            Debug.Log($"隐藏到达边界的物体: {obj.name}");
        }
    }

    IEnumerator FadeOutObject(int objectIndex)
    {
        GameObject obj = movingObjects[objectIndex];
        Renderer renderer = objectRenderers[objectIndex];

        if (obj == null || renderer == null) yield break;

        Debug.Log($"开始淡出物体: {obj.name}");

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration && obj != null)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            SetObjectAlpha(renderer, alpha);
            yield return null;
        }

        if (obj != null)
        {
            if (destroyAfterFade)
            {
                Destroy(obj);
            }
            else
            {
                obj.SetActive(false);
            }
            Debug.Log($"物体 {obj.name} 淡出完成");
        }
    }

    void SetObjectAlpha(Renderer renderer, float alpha)
    {
        Material[] materials = renderer.materials;
        for (int i = 0; i < materials.Length; i++)
        {
            Color color = materials[i].color;
            color.a = alpha;
            materials[i].color = color;
        }
    }

    // 公共方法用于外部控制
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
        Debug.Log($"移动速度设置为: {moveSpeed}");
    }

    public void SetDirection(bool positiveDirection)
    {
        movePositive = positiveDirection;
        Debug.Log($"移动方向设置为: {(movePositive ? "正Z轴" : "负Z轴")}");
    }

    public void ToggleDirection()
    {
        movePositive = !movePositive;
        Debug.Log($"切换移动方向为: {(movePositive ? "正Z轴" : "负Z轴")}");
    }

    // 重置所有物体状态
    public void ResetObjects()
    {
        StopZAxisMovement();
        isMoving = false;

        for (int i = 0; i < movingObjects.Count; i++)
        {
            if (movingObjects[i] != null)
            {
                // 重置位置
                if (i < originalPositions.Count)
                {
                    movingObjects[i].transform.position = originalPositions[i];
                }

                // 重置材质透明度
                if (i < objectRenderers.Count && objectRenderers[i] != null)
                {
                    SetObjectAlpha(objectRenderers[i], 1f);
                }

                // 确保物体可见
                movingObjects[i].SetActive(true);
            }
        }

        Debug.Log("所有物体状态已重置");
    }

    // 添加物体到控制器
    public void AddObject(GameObject newObject)
    {
        if (newObject != null && !movingObjects.Contains(newObject))
        {
            movingObjects.Add(newObject);
            InitializeObjects(); // 重新初始化以包含新物体
            Debug.Log($"添加物体: {newObject.name}");
        }
    }

    // 移除物体从控制器
    public void RemoveObject(GameObject objectToRemove)
    {
        if (movingObjects.Contains(objectToRemove))
        {
            movingObjects.Remove(objectToRemove);
            InitializeObjects(); // 重新初始化
            Debug.Log($"移除物体: {objectToRemove.name}");
        }
    }

    // 在Scene视图中显示调试信息
    void OnDrawGizmosSelected()
    {
        if (useBoundary)
        {
            Gizmos.color = Color.cyan;

            // 绘制边界线
            Vector3 maxZLineStart = new Vector3(-10, 0, maxZPosition);
            Vector3 maxZLineEnd = new Vector3(10, 0, maxZPosition);
            Vector3 minZLineStart = new Vector3(-10, 0, minZPosition);
            Vector3 minZLineEnd = new Vector3(10, 0, minZPosition);

            Gizmos.DrawLine(maxZLineStart, maxZLineEnd);
            Gizmos.DrawLine(minZLineStart, minZLineEnd);

            // 绘制边界文字
#if UNITY_EDITOR
            UnityEditor.Handles.Label(new Vector3(0, 2, maxZPosition), "最大Z边界");
            UnityEditor.Handles.Label(new Vector3(0, 2, minZPosition), "最小Z边界");
#endif
        }

        // 绘制移动方向指示器
        Gizmos.color = movePositive ? Color.green : Color.red;
        foreach (var obj in movingObjects)
        {
            if (obj != null)
            {
                Vector3 direction = movePositive ? Vector3.forward : Vector3.back;
                Gizmos.DrawRay(obj.transform.position, direction * 3f);
            }
        }
    }

    [ContextMenu("测试Z轴移动")]
    public void TestZAxisMovement()
    {
        Debug.Log("手动测试Z轴移动");
        StartZAxisMovement();
    }

    [ContextMenu("停止移动")]
    public void TestStopMovement()
    {
        Debug.Log("手动停止移动");
        StopZAxisMovement();
    }
}