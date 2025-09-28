using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacingCameraController : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target; // 要跟随的车辆

    [Header("位置设置")]
    public Vector3 offset = new Vector3(0, 2, -5); // 相机相对于目标的偏移
    public float distance = 5.0f; // 默认距离
    public float height = 2.0f; // 默认高度

    [Header("平滑设置")]
    public float positionDamping = 3.0f; // 位置平滑系数
    public float rotationDamping = 3.0f; // 旋转平滑系数

    [Header("碰撞避免")]
    public float collisionOffset = 0.2f; // 避免相机穿墙的偏移
    public LayerMask collisionLayers = -1; // 碰撞检测的层

    [Header("高速效果")]
    public float fovChangeSpeed = 10f; // 视野变化速度
    public float normalFOV = 60f; // 正常视野
    public float maxFOV = 75f; // 最大视野(高速时)
    public float speedThreshold = 30f; // 开始改变视野的速度阈值

    private Vector3 wantedPosition; // 相机期望位置
    private Camera cam; // 相机组件
    private Rigidbody targetRigidbody; // 目标的刚体

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

        // 计算基础位置
        wantedPosition = target.TransformPoint(offset);

        // 碰撞检测
        CheckCameraCollision(ref wantedPosition);

        // 平滑移动相机位置
        transform.position = Vector3.Lerp(transform.position, wantedPosition,
                                        positionDamping * Time.deltaTime);

        // 计算看向目标的方向(稍微看向前方)
        Vector3 lookAtPosition = target.position + target.forward * 3f;

        // 平滑旋转相机
        Quaternion wantedRotation = Quaternion.LookRotation(lookAtPosition - transform.position,
                                                          target.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, wantedRotation,
                                            rotationDamping * Time.deltaTime);

        // 根据速度调整视野
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
            // 根据速度比例计算FOV
            float speedRatio = Mathf.Clamp01((currentSpeed - speedThreshold) / speedThreshold);
            targetFOV = Mathf.Lerp(normalFOV, maxFOV, speedRatio);
        }

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
    }

    // 在编辑器中可视化相机位置
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