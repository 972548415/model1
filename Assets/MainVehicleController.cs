// MainVehicleController.cs
using UnityEngine;

public class MainVehicleController : MonoBehaviour
{
    [Header("触发位置设置")]
    public Transform targetPosition;
    public float triggerDistance = 2.0f;

    [Header("调试设置")]
    public bool showDebugInfo = true;

    private bool hasTriggered = false;
    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
        if (showDebugInfo)
            Debug.Log($"主车辆控制器已启动，目标位置: {targetPosition}");
    }

    void Update()
    {
        if (!hasTriggered && targetPosition != null)
        {
            float distance = Vector3.Distance(transform.position, targetPosition.position);

            if (showDebugInfo)
            {
                // 每1秒输出一次距离信息，避免日志过多
                if (Vector3.Distance(transform.position, lastPosition) > 0.1f)
                {
                    Debug.Log($"主车辆距离目标位置: {distance:F1}米 (触发距离: {triggerDistance}米)");
                    lastPosition = transform.position;
                }
            }

            if (distance <= triggerDistance)
            {
                TriggerEvents();
                hasTriggered = true;
            }
        }
    }

    void TriggerEvents()
    {
        Debug.Log("=== 主车辆到达目标位置，触发所有事件 ===");

        // 触发主事件
        EgoEventCenter.PostEvent(EventType.MainVehicleReachPosition);

        // 触发其他相关事件
        EgoEventCenter.PostEvent(EventType.VehicleStartMoving);
        EgoEventCenter.PostEvent(EventType.VehicleDisappear);
        EgoEventCenter.PostEvent(EventType.SmokeStartGenerate);
        EgoEventCenter.PostEvent(EventType.PlayTriggerSound);

        Debug.Log("所有事件已触发完成");
    }

    // 重置触发状态（用于测试）
    public void ResetTrigger()
    {
        hasTriggered = false;
        Debug.Log("主车辆触发状态已重置");
    }

    // 在Scene视图中显示触发范围
    void OnDrawGizmosSelected()
    {
        if (targetPosition != null)
        {
            Gizmos.color = hasTriggered ? Color.green : Color.red;
            Gizmos.DrawWireSphere(targetPosition.position, triggerDistance);
            Gizmos.DrawLine(transform.position, targetPosition.position);
        }
    }
}
