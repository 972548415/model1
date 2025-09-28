using UnityEngine;

public class PrometeoAutoController : MonoBehaviour
{
    [Header("自动控制设置")]
    public float maxSteeringAngle = 30f;
    public float steeringSpeed = 2f;
    public float brakeDistance = 8f;
    public float arrivalDistance = 3f;

    private PrometeoCarController carController;
    private Transform currentDestination;
    private float targetSpeed;
    private bool isAutoMoving = false;
    private bool hasReachedDestination = false;

    void Start()
    {
        carController = GetComponent<PrometeoCarController>();
        if (carController == null)
        {
            Debug.LogError("PrometeoAutoController requires PrometeoCarController component!");
            return;
        }

        // 初始状态：禁用自动控制
        StopAutoMove();
    }

    void Update()
    {
        if (!isAutoMoving || hasReachedDestination || carController == null) return;

        if (currentDestination != null)
        {
            // 计算转向和控制速度
            HandleSteering();
            HandleSpeedControl();
            CheckArrival();
        }
    }

    void HandleSteering()
    {
        Vector3 directionToTarget = (currentDestination.position - transform.position).normalized;
        float angleToTarget = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);

        // 应用转向
        if (angleToTarget > 5f)
        {
            // 向右转
            carController.TurnRight();
        }
        else if (angleToTarget < -5f)
        {
            // 向左转
            carController.TurnLeft();
        }
        else
        {
            // 重置转向
            carController.ResetSteeringAngle();
        }
    }

    void HandleSpeedControl()
    {
        // 检查前方障碍物
        RaycastHit hit;
        bool hasObstacle = Physics.Raycast(transform.position, transform.forward, out hit, brakeDistance);

        if (hasObstacle)
        {
            // 前方有障碍物，刹车或减速
            carController.Brakes();
            carController.ThrottleOff();
        }
        else
        {
            // 根据距离调整速度
            float distanceToTarget = Vector3.Distance(transform.position, currentDestination.position);
            float speedMultiplier = Mathf.Clamp01(distanceToTarget / (brakeDistance * 2f));

            // 设置油门
            if (carController.carSpeed < targetSpeed * speedMultiplier)
            {
                carController.GoForward();
            }
            else
            {
                carController.ThrottleOff();
            }
        }
    }

    void CheckArrival()
    {
        if (currentDestination == null) return;

        float distance = Vector3.Distance(transform.position, currentDestination.position);
        if (distance <= arrivalDistance)
        {
            ReachDestination();
        }
    }

    void ReachDestination()
    {
        hasReachedDestination = true;
        StopAutoMove();
        Debug.Log($"{name} 已到达目的地");
    }

    // 公共方法
    public void SetDestination(Transform destination)
    {
        currentDestination = destination;
        hasReachedDestination = false;
    }

    public void SetTargetSpeed(float speed)
    {
        targetSpeed = speed;
    }

    public void StartAutoMove()
    {
        if (carController != null && currentDestination != null)
        {
            isAutoMoving = true;
            hasReachedDestination = false;
            Debug.Log($"{name} 开始自动移动");
        }
    }

    public void StopAutoMove()
    {
        isAutoMoving = false;
        if (carController != null)
        {
            carController.ThrottleOff();
            carController.ResetSteeringAngle();
        }
    }

    public bool HasReachedDestination()
    {
        return hasReachedDestination;
    }

    void OnDrawGizmos()
    {
        if (isAutoMoving && currentDestination != null)
        {
            Gizmos.color = hasReachedDestination ? Color.green : Color.blue;
            Gizmos.DrawLine(transform.position, currentDestination.position);

            if (!hasReachedDestination)
            {
                Gizmos.DrawWireSphere(currentDestination.position, arrivalDistance);
            }

            // 绘制刹车距离
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * brakeDistance);
        }
    }
}
