using UnityEngine;

public class 触发点 : MonoBehaviour
{
    [Header("触发设置")]
    public float triggerRadius = 5f;
    public string mainCarTag = "Player";

    [Header("目标车辆设置")]
    public PrometeoCarController[] otherCarsToActivate;
    public Transform[] otherCarsDestinations;
    public float[] carTargetSpeeds;

    private bool hasBeenTriggered = false;
    private SphereCollider triggerCollider;

    void Start()
    {
        // 确保有碰撞器
        triggerCollider = GetComponent<SphereCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<SphereCollider>();
            Debug.Log($"为触发点 {name} 添加了 SphereCollider");
        }

        triggerCollider.isTrigger = true;
        triggerCollider.radius = triggerRadius;

        // 禁用渲染器（如果存在）
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasBeenTriggered)
        {
            Debug.Log($"触发点 {name} 已被触发过，忽略此次触发");
            return;
        }

        Debug.Log($"触发点 {name} 检测到碰撞: {other.name}, 标签: {other.tag}");

        if (other.CompareTag(mainCarTag))
        {
            Debug.Log($"主车辆 {other.name} 进入触发区域");
            TriggerOtherCars();
            hasBeenTriggered = true;

            // 可选：禁用碰撞器避免重复触发
            triggerCollider.enabled = false;
        }
    }

    public void TriggerOtherCars()
    {
        Debug.Log($"开始激活其他车辆，数量: {otherCarsToActivate?.Length}");

        // 触发事件
        TriggerEventManager.ActivateTrigger(
            TriggerType.MainCarReachedPoint,
            transform,
            gameObject
        );

        // 激活其他车辆
        ActivateAllOtherCars();
    }

    void ActivateAllOtherCars()
    {
        if (otherCarsToActivate == null || otherCarsToActivate.Length == 0)
        {
            Debug.LogWarning("没有设置其他车辆");
            return;
        }

        for (int i = 0; i < otherCarsToActivate.Length; i++)
        {
            if (otherCarsToActivate[i] != null)
            {
                Debug.Log($"正在设置车辆 {i}: {otherCarsToActivate[i].name}");

                // 确保有自动控制器
                PrometeoAutoController autoController = otherCarsToActivate[i].GetComponent<PrometeoAutoController>();
                if (autoController == null)
                {
                    autoController = otherCarsToActivate[i].gameObject.AddComponent<PrometeoAutoController>();
                    Debug.Log($"为 {otherCarsToActivate[i].name} 添加了自动控制器");
                }

                // 设置目的地
                if (otherCarsDestinations != null && i < otherCarsDestinations.Length && otherCarsDestinations[i] != null)
                {
                    autoController.SetDestination(otherCarsDestinations[i]);
                    Debug.Log($"设置目的地: {otherCarsDestinations[i].name}");
                }
                else
                {
                    Debug.LogWarning($"车辆 {i} 没有设置目的地");
                }

                // 设置速度
                if (carTargetSpeeds != null && i < carTargetSpeeds.Length)
                {
                    autoController.SetTargetSpeed(carTargetSpeeds[i]);
                    Debug.Log($"设置速度: {carTargetSpeeds[i]}");
                }

                // 开始移动
                autoController.StartAutoMove();
                Debug.Log($"开始移动车辆 {otherCarsToActivate[i].name}");
            }
            else
            {
                Debug.LogWarning($"车辆 {i} 为 null");
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = hasBeenTriggered ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);

        // 绘制连线到所有目的地
        if (otherCarsDestinations != null)
        {
            Gizmos.color = Color.blue;
            foreach (var dest in otherCarsDestinations)
            {
                if (dest != null)
                    Gizmos.DrawLine(transform.position, dest.position);
            }
        }
    }

    // 调试方法：手动触发
    [ContextMenu("手动触发")]
    public void ManualTrigger()
    {
        if (!hasBeenTriggered)
        {
            Debug.Log("手动触发");
            TriggerOtherCars();
            hasBeenTriggered = true;
        }
    }
}