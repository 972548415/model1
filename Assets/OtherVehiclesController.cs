// OtherVehiclesController.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OtherVehiclesController : MonoBehaviour
{
    [Header("其他车辆设置")]
    public List<GameObject> otherVehicles = new List<GameObject>();
    public Transform destination;
    public float moveSpeed = 3.0f;
    public float rotationSpeed = 2.0f;

    [Header("消失设置")]
    public float fadeDuration = 3.0f;
    public bool destroyAfterFade = false;

    [Header("移动设置")]
    public float startMoveDelay = 0.5f;
    public float perVehicleDelay = 0.2f;

    private List<Vector3> originalPositions = new List<Vector3>();
    private List<Renderer> vehicleRenderers = new List<Renderer>();
    private List<Material[]> originalMaterials = new List<Material[]>();
    private bool isMoving = false;

    void Start()
    {
        InitializeVehicles();

        // 注册事件监听
        EgoEventCenter.AddListener(EventType.MainVehicleReachPosition, OnMainVehicleReach);
        EgoEventCenter.AddListener(EventType.VehicleStartMoving, OnVehicleStartMoving);

        Debug.Log($"其他车辆控制器已启动，共管理 {otherVehicles.Count} 辆车");
    }

    void OnDestroy()
    {
        EgoEventCenter.RemoveListener(EventType.MainVehicleReachPosition, OnMainVehicleReach);
        EgoEventCenter.RemoveListener(EventType.VehicleStartMoving, OnVehicleStartMoving);
    }

    void InitializeVehicles()
    {
        originalPositions.Clear();
        vehicleRenderers.Clear();
        originalMaterials.Clear();

        foreach (var vehicle in otherVehicles)
        {
            if (vehicle != null)
            {
                // 保存原始位置
                originalPositions.Add(vehicle.transform.position);

                // 获取渲染器和材质
                Renderer renderer = vehicle.GetComponent<Renderer>();
                if (renderer != null)
                {
                    vehicleRenderers.Add(renderer);
                    originalMaterials.Add(renderer.materials);
                }
                else
                {
                    vehicleRenderers.Add(null);
                    originalMaterials.Add(null);
                }

                Debug.Log($"初始化车辆: {vehicle.name} 位置: {vehicle.transform.position}");
            }
        }
    }

    void OnMainVehicleReach()
    {
        Debug.Log("收到主车辆到达事件");
        StartVehicleActions();
    }

    void OnVehicleStartMoving()
    {
        Debug.Log("收到车辆开始移动事件");
        StartVehicleActions();
    }

    void StartVehicleActions()
    {
        if (isMoving) return;

        Debug.Log("开始执行其他车辆动作");
        isMoving = true;

        // 开始移动车辆
        StartCoroutine(MoveAllVehicles());

        // 开始淡出效果
        StartCoroutine(FadeOutAllVehicles());
    }

    IEnumerator MoveAllVehicles()
    {
        if (destination == null)
        {
            Debug.LogError("目的地未设置！其他车辆无法移动");
            yield break;
        }

        Debug.Log($"开始移动 {otherVehicles.Count} 辆车到目的地: {destination.position}");

        // 初始延迟
        yield return new WaitForSeconds(startMoveDelay);

        // 为每辆车启动移动协程
        for (int i = 0; i < otherVehicles.Count; i++)
        {
            if (otherVehicles[i] != null)
            {
                StartCoroutine(MoveSingleVehicle(i));
                yield return new WaitForSeconds(perVehicleDelay);
            }
        }
    }

    IEnumerator MoveSingleVehicle(int vehicleIndex)
    {
        GameObject vehicle = otherVehicles[vehicleIndex];
        if (vehicle == null) yield break;

        string vehicleName = vehicle.name;
        Debug.Log($"开始移动车辆: {vehicleName}");

        Vector3 startPosition = vehicle.transform.position;
        float distance = Vector3.Distance(startPosition, destination.position);
        float moveTime = distance / moveSpeed;

        float elapsedTime = 0f;

        while (elapsedTime < moveTime && vehicle != null)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / moveTime;

            // 移动位置
            vehicle.transform.position = Vector3.Lerp(startPosition, destination.position, progress);

            // 旋转朝向目的地
            if (rotationSpeed > 0)
            {
                Vector3 direction = (destination.position - vehicle.transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    vehicle.transform.rotation = Quaternion.Slerp(
                        vehicle.transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
                }
            }

            yield return null;
        }

        // 确保最终位置正确
        if (vehicle != null)
        {
            vehicle.transform.position = destination.position;
            Debug.Log($"车辆 {vehicleName} 已到达目的地");
        }
    }

    IEnumerator FadeOutAllVehicles()
    {
        Debug.Log($"开始淡出 {vehicleRenderers.Count} 辆车，持续时间: {fadeDuration}秒");

        // 等待一段时间后开始淡出
        yield return new WaitForSeconds(startMoveDelay);

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            for (int i = 0; i < vehicleRenderers.Count; i++)
            {
                if (vehicleRenderers[i] != null && otherVehicles[i] != null)
                {
                    SetVehicleAlpha(vehicleRenderers[i], alpha);
                }
            }

            yield return null;
        }

        // 最终处理
        for (int i = 0; i < otherVehicles.Count; i++)
        {
            if (otherVehicles[i] != null)
            {
                if (destroyAfterFade)
                {
                    Destroy(otherVehicles[i]);
                    Debug.Log($"销毁车辆: {otherVehicles[i].name}");
                }
                else
                {
                    otherVehicles[i].SetActive(false);
                    Debug.Log($"隐藏车辆: {otherVehicles[i].name}");
                }
            }
        }

        Debug.Log("所有车辆淡出完成");
    }

    void SetVehicleAlpha(Renderer renderer, float alpha)
    {
        Material[] materials = renderer.materials;
        for (int i = 0; i < materials.Length; i++)
        {
            Color color = materials[i].color;
            color.a = alpha;
            materials[i].color = color;
        }
    }

    // 重置所有车辆状态（用于测试）
    public void ResetVehicles()
    {
        StopAllCoroutines();
        isMoving = false;

        for (int i = 0; i < otherVehicles.Count; i++)
        {
            if (otherVehicles[i] != null)
            {
                // 重置位置
                if (i < originalPositions.Count)
                {
                    otherVehicles[i].transform.position = originalPositions[i];
                }

                // 重置材质透明度
                if (i < vehicleRenderers.Count && vehicleRenderers[i] != null)
                {
                    SetVehicleAlpha(vehicleRenderers[i], 1f);
                }

                // 确保车辆可见
                otherVehicles[i].SetActive(true);
            }
        }

        Debug.Log("所有车辆状态已重置");
    }

    // 在Scene视图中显示调试信息
    void OnDrawGizmosSelected()
    {
        if (destination != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(destination.position, 1f);

            // 绘制从每辆车到目的地的线
            Gizmos.color = Color.blue;
            foreach (var vehicle in otherVehicles)
            {
                if (vehicle != null && destination != null)
                {
                    Gizmos.DrawLine(vehicle.transform.position, destination.position);
                }
            }
        }
    }

    [ContextMenu("测试移动车辆")]
    public void TestMoveVehicles()
    {
        Debug.Log("手动测试移动车辆");
        StartVehicleActions();
    }
}
