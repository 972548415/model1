using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacingCameraController : MonoBehaviour
{
    [Header("����Ŀ��")]
    public Transform target; // Ҫ����ĳ���

    [Header("λ������")]
    public Vector3 offset = new Vector3(0, 2, -5); // ��������Ŀ���ƫ��
    public float distance = 5.0f; // Ĭ�Ͼ���
    public float height = 2.0f; // Ĭ�ϸ߶�

    [Header("ƽ������")]
    public float positionDamping = 3.0f; // λ��ƽ��ϵ��
    public float rotationDamping = 3.0f; // ��תƽ��ϵ��

    [Header("��ײ����")]
    public float collisionOffset = 0.2f; // ���������ǽ��ƫ��
    public LayerMask collisionLayers = -1; // ��ײ���Ĳ�

    [Header("����Ч��")]
    public float fovChangeSpeed = 10f; // ��Ұ�仯�ٶ�
    public float normalFOV = 60f; // ������Ұ
    public float maxFOV = 75f; // �����Ұ(����ʱ)
    public float speedThreshold = 30f; // ��ʼ�ı���Ұ���ٶ���ֵ

    private Vector3 wantedPosition; // �������λ��
    private Camera cam; // ������
    private Rigidbody targetRigidbody; // Ŀ��ĸ���

    void Start()
    {
        cam = GetComponent<Camera>();
        if (target != null)
        {
            targetRigidbody = target.GetComponent<Rigidbody>();
        }
    }

    void LateUpdate()
    {
        if (!target)
            return;

        // �������λ��
        wantedPosition = target.TransformPoint(offset);

        // ��ײ���
        CheckCameraCollision(ref wantedPosition);

        // ƽ���ƶ����λ��
        transform.position = Vector3.Lerp(transform.position, wantedPosition,
                                        positionDamping * Time.deltaTime);

        // ���㿴��Ŀ��ķ���(��΢����ǰ��)
        Vector3 lookAtPosition = target.position + target.forward * 3f;

        // ƽ����ת���
        Quaternion wantedRotation = Quaternion.LookRotation(lookAtPosition - transform.position,
                                                          target.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, wantedRotation,
                                            rotationDamping * Time.deltaTime);

        // �����ٶȵ�����Ұ
        AdjustFOVBasedOnSpeed();
    }

    void CheckCameraCollision(ref Vector3 targetPos)
    {
        RaycastHit hit;
        Vector3 dir = targetPos - target.position;

        if (Physics.SphereCast(target.position, 0.3f, dir.normalized, out hit,
                             dir.magnitude + collisionOffset, collisionLayers))
        {
            targetPos = hit.point - dir.normalized * collisionOffset;
        }
    }

    void AdjustFOVBasedOnSpeed()
    {
        if (targetRigidbody == null || cam == null)
            return;

        float currentSpeed = targetRigidbody.velocity.magnitude;
        float targetFOV = normalFOV;

        if (currentSpeed > speedThreshold)
        {
            // �����ٶȱ�������FOV
            float speedRatio = Mathf.Clamp01((currentSpeed - speedThreshold) / speedThreshold);
            targetFOV = Mathf.Lerp(normalFOV, maxFOV, speedRatio);
        }

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
    }

    // �ڱ༭���п��ӻ����λ��
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(target.position, target.TransformPoint(offset));
            Gizmos.DrawWireSphere(target.TransformPoint(offset), 0.2f);
        }
    }
}