using UnityEngine;

public class PrometeoAutoController : MonoBehaviour
{
    [Header("�Զ���������")]
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

        // ��ʼ״̬�������Զ�����
        StopAutoMove();
    }

    void Update()
    {
        if (!isAutoMoving || hasReachedDestination || carController == null) return;

        if (currentDestination != null)
        {
            // ����ת��Ϳ����ٶ�
            HandleSteering();
            HandleSpeedControl();
            CheckArrival();
        }
    }

    void HandleSteering()
    {
        Vector3 directionToTarget = (currentDestination.position - transform.position).normalized;
        float angleToTarget = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);

        // Ӧ��ת��
        if (angleToTarget > 5f)
        {
            // ����ת
            carController.TurnRight();
        }
        else if (angleToTarget < -5f)
        {
            // ����ת
            carController.TurnLeft();
        }
        else
        {
            // ����ת��
            carController.ResetSteeringAngle();
        }
    }

    void HandleSpeedControl()
    {
        // ���ǰ���ϰ���
        RaycastHit hit;
        bool hasObstacle = Physics.Raycast(transform.position, transform.forward, out hit, brakeDistance);

        if (hasObstacle)
        {
            // ǰ�����ϰ��ɲ�������
            carController.Brakes();
            carController.ThrottleOff();
        }
        else
        {
            // ���ݾ�������ٶ�
            float distanceToTarget = Vector3.Distance(transform.position, currentDestination.position);
            float speedMultiplier = Mathf.Clamp01(distanceToTarget / (brakeDistance * 2f));

            // ��������
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
        Debug.Log($"{name} �ѵ���Ŀ�ĵ�");
    }

    // ��������
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
            Debug.Log($"{name} ��ʼ�Զ��ƶ�");
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

            // ����ɲ������
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * brakeDistance);
        }
    }
}
