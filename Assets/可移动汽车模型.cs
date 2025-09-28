using UnityEngine;

public class 可移动汽车模型 : MonoBehaviour
{
    [Header("前轮转向设置")]
    public Transform frontLeftWheel;    // 左前轮视觉模型
    public Transform frontRightWheel;   // 右前轮视觉模型
    public float maxSteeringAngle = 35f; // 最大转向角度
    public float steeringSpeed = 100f;   // 转向速度

    [Header("移动设置")]
    public float moveSpeed = 8f;        // 移动速度
    public float brakeDistance = 3f;    // 刹车距离

    [Header("障碍物检测")]
    public float frontDetectionDistance = 10f; // 前方检测距离
    public float sideDetectionDistance = 4f;   // 侧方检测距离
    public LayerMask obstacleMask;             // 障碍物层级

    [Header("目的地")]
    public Transform target;             // 目标位置
    public float arrivalDistance = 2f;   // 到达判定距离
    public float disappearDelay = 0.5f;  // 到达后消失延迟时间

    [Header("消失效果")]
    public bool useFadeEffect = true;    // 是否使用淡出效果
    public float fadeDuration = 1f;      // 淡出持续时间

    // 私有变量
    private float currentSteeringAngle = 0f;
    private bool isAvoiding = false;
    private bool hasReachedTarget = false;
    private Vector3 avoidanceDirection;
    private Renderer[] carRenderers;     // 所有渲染器组件
    private float fadeTimer = 0f;        // 淡出计时器
    private bool isDisappearing = false; // 是否正在消失

    void Start()
    {
        // 获取所有渲染器组件
        carRenderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        if (hasReachedTarget)
        {
            HandleDisappearance();
            return;
        }

        if (target == null) return;

        // 检查是否到达目标
        if (CheckArrival())
        {
            StartDisappearance();
            return;
        }

        // 检测障碍物并计算转向
        HandleObstacleAvoidance();

        // 应用转向和控制移动
        ApplySteering();
        MoveCar();
    }

    bool CheckArrival()
    {
        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= arrivalDistance;
    }

    void StartDisappearance()
    {
        hasReachedTarget = true;
        isDisappearing = true;
        fadeTimer = 0f;

        // 立即停止车辆移动
        moveSpeed = 0f;
        currentSteeringAngle = 0f;
        ApplySteering();

        Debug.Log("汽车已到达目的地，开始消失过程");
    }

    void HandleDisappearance()
    {
        if (!isDisappearing) return;

        fadeTimer += Time.deltaTime;

        if (useFadeEffect && fadeTimer > disappearDelay)
        {
            // 淡出效果
            float fadeProgress = (fadeTimer - disappearDelay) / fadeDuration;
            ApplyFadeEffect(1f - fadeProgress);

            if (fadeTimer > disappearDelay + fadeDuration)
            {
                DestroyCar();
            }
        }
        else if (!useFadeEffect && fadeTimer > disappearDelay)
        {
            // 直接消失
            DestroyCar();
        }
    }

    void ApplyFadeEffect(float alpha)
    {
        foreach (Renderer renderer in carRenderers)
        {
            if (renderer != null)
            {
                Material[] materials = renderer.materials;
                foreach (Material material in materials)
                {
                    Color color = material.color;
                    color.a = alpha;
                    material.color = color;
                }
            }
        }
    }

    void DestroyCar()
    {
        Debug.Log("汽车已消失");
        Destroy(gameObject);
    }

    void HandleObstacleAvoidance()
    {
        isAvoiding = false;
        avoidanceDirection = transform.forward;

        // 前方障碍物检测
        RaycastHit frontHit;
        if (Physics.Raycast(transform.position, transform.forward, out frontHit, frontDetectionDistance, obstacleMask))
        {
            isAvoiding = true;

            // 计算避障方向（根据障碍物位置决定转向方向）
            Vector3 hitLocalPos = transform.InverseTransformPoint(frontHit.point);
            float avoidanceSteering = hitLocalPos.x > 0 ? -1f : 1f;

            // 根据距离调整转向强度
            float distanceFactor = 1f - (frontHit.distance / frontDetectionDistance);
            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle,
                                            avoidanceSteering * maxSteeringAngle * distanceFactor,
                                            Time.deltaTime * steeringSpeed);
        }
        else
        {
            // 没有障碍物时朝向目标
            Vector3 targetDirection = (target.position - transform.position).normalized;
            float targetAngle = Vector3.SignedAngle(transform.forward, targetDirection, Vector3.up);

            // 平滑转向
            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle,
                                            Mathf.Clamp(targetAngle, -maxSteeringAngle, maxSteeringAngle),
                                            Time.deltaTime * steeringSpeed);
        }

        // 侧面障碍物检测（辅助避障）
        CheckSideObstacles();
    }

    void CheckSideObstacles()
    {
        RaycastHit leftHit, rightHit;
        bool leftObstacle = Physics.Raycast(transform.position, -transform.right, out leftHit, sideDetectionDistance, obstacleMask);
        bool rightObstacle = Physics.Raycast(transform.position, transform.right, out rightHit, sideDetectionDistance, obstacleMask);

        if (leftObstacle)
        {
            // 左侧有障碍物，向右转向
            float avoidance = (1f - (leftHit.distance / sideDetectionDistance)) * 0.7f;
            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, maxSteeringAngle * avoidance, Time.deltaTime * steeringSpeed);
            isAvoiding = true;
        }
        else if (rightObstacle)
        {
            // 右侧有障碍物，向左转向
            float avoidance = (1f - (rightHit.distance / sideDetectionDistance)) * -0.7f;
            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, maxSteeringAngle * avoidance, Time.deltaTime * steeringSpeed);
            isAvoiding = true;
        }
    }

    void ApplySteering()
    {
        // 应用转向到整个车辆
        transform.Rotate(0, currentSteeringAngle * Time.deltaTime * 0.5f, 0);

        // 应用转向到前轮视觉模型
        if (frontLeftWheel != null)
        {
            frontLeftWheel.localRotation = Quaternion.Euler(0, currentSteeringAngle, 0);
        }
        if (frontRightWheel != null)
        {
            frontRightWheel.localRotation = Quaternion.Euler(0, currentSteeringAngle, 0);
        }
    }

    void MoveCar()
    {
        // 根据前方障碍物距离调整速度
        RaycastHit frontHit;
        float speedMultiplier = 1f;

        if (Physics.Raycast(transform.position, transform.forward, out frontHit, brakeDistance, obstacleMask))
        {
            // 前方有障碍物时减速
            speedMultiplier = Mathf.Clamp01(frontHit.distance / brakeDistance);
        }

        // 移动车辆
        transform.Translate(Vector3.forward * moveSpeed * speedMultiplier * Time.deltaTime);
    }

    void StopCar()
    {
        // 停止车辆并重置转向
        currentSteeringAngle = 0f;
        moveSpeed = 0f;
        ApplySteering();
    }

    // 设置新的目标
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        hasReachedTarget = false;
        isDisappearing = false;

        // 重置透明度（如果之前正在消失）
        if (useFadeEffect)
        {
            ApplyFadeEffect(1f);
        }
    }

    // 检查是否已经到达目标
    public bool HasReachedTarget()
    {
        return hasReachedTarget;
    }

    // 检查是否正在消失
    public bool IsDisappearing()
    {
        return isDisappearing;
    }

    // 立即消失（无需等待）
    public void DisappearImmediately()
    {
        DestroyCar();
    }

    // 可视化调试
    void OnDrawGizmos()
    {
        // 绘制前方检测线
        Gizmos.color = isAvoiding ? Color.red : Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * frontDetectionDistance);

        // 绘制侧方检测线
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + -transform.right * sideDetectionDistance);
        Gizmos.DrawLine(transform.position, transform.position + transform.right * sideDetectionDistance);

        // 绘制刹车距离
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * brakeDistance);

        // 绘制目标方向
        if (target != null)
        {
            Gizmos.color = hasReachedTarget ? Color.gray : Color.green;
            Gizmos.DrawLine(transform.position, target.position);
            Gizmos.DrawWireSphere(target.position, arrivalDistance);
        }

        // 绘制当前转向方向
        Gizmos.color = Color.magenta;
        Vector3 steeringDir = Quaternion.Euler(0, currentSteeringAngle, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, steeringDir * 3f);
    }
}