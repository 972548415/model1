// MainVehicleController.cs
using UnityEngine;

public class MainVehicleController : MonoBehaviour
{
    [Header("����λ������")]
    public Transform targetPosition;
    public float triggerDistance = 2.0f;

    [Header("��������")]
    public bool showDebugInfo = true;

    private bool hasTriggered = false;
    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
        if (showDebugInfo)
            Debug.Log($"��������������������Ŀ��λ��: {targetPosition}");
    }

    void Update()
    {
        if (!hasTriggered && targetPosition != null)
        {
            float distance = Vector3.Distance(transform.position, targetPosition.position);

            if (showDebugInfo)
            {
                // ÿ1�����һ�ξ�����Ϣ��������־����
                if (Vector3.Distance(transform.position, lastPosition) > 0.1f)
                {
                    Debug.Log($"����������Ŀ��λ��: {distance:F1}�� (��������: {triggerDistance}��)");
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
        Debug.Log("=== ����������Ŀ��λ�ã����������¼� ===");

        // �������¼�
        EgoEventCenter.PostEvent(EventType.MainVehicleReachPosition);

        // ������������¼�
        EgoEventCenter.PostEvent(EventType.VehicleStartMoving);
        EgoEventCenter.PostEvent(EventType.VehicleDisappear);
        EgoEventCenter.PostEvent(EventType.SmokeStartGenerate);
        EgoEventCenter.PostEvent(EventType.PlayTriggerSound);

        Debug.Log("�����¼��Ѵ������");
    }

    // ���ô���״̬�����ڲ��ԣ�
    public void ResetTrigger()
    {
        hasTriggered = false;
        Debug.Log("����������״̬������");
    }

    // ��Scene��ͼ����ʾ������Χ
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
