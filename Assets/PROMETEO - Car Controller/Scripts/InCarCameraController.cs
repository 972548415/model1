using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class InCarCameraController : MonoBehaviour
{
    [Header("视角设置")]
    public Transform vehicle;                  // 车辆变换组件
    public Vector3 cameraPositionOffset = new Vector3(0, 1.2f, 0.3f); // 摄像机相对于车辆的位置偏移
    public float mouseSensitivity = 2f;        // 鼠标灵敏度
    public float maxVerticalAngle = 60f;       // 最大垂直视角角度
    public float minVerticalAngle = -30f;      // 最小垂直视角角度

    [Header("移动平滑度")]
    public float positionSmoothTime = 0.1f;    // 位置平滑时间
    public float rotationSmoothTime = 0.1f;    // 旋转平滑时间

    [Header("碰撞检测")]
    public bool enableCollisionDetection = true; // 是否启用碰撞检测
    public float collisionRadius = 0.3f;       // 碰撞检测半径
    public LayerMask collisionMask;            // 碰撞层掩码
    public float collisionOffset = 0.1f;       // 碰撞偏移量

    private float xRotation = 0f;              // X轴旋转角度
    private float yRotation = 0f;              // Y轴旋转角度
    private Vector3 positionVelocity;          // 位置平滑速度
    private float rotationVelocity;            // 旋转平滑速度
    private Vector3 targetPosition;            // 目标位置
    private Quaternion targetRotation;         // 目标旋转

    private void Start()
    {
        // 初始化旋转角度为车辆当前角度
        Vector3 initialEuler = vehicle.rotation.eulerAngles;
        xRotation = initialEuler.x;
        yRotation = initialEuler.y;

        // 锁定并隐藏鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // 鼠标输入处理
        HandleMouseInput();
    }

    private void LateUpdate()
    {
        // 更新摄像机位置和旋转
        UpdateCameraTransform();
    }

    private void HandleMouseInput()
    {
        // 获取鼠标移动输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 计算新的旋转角度
        yRotation += mouseX;
        xRotation -= mouseY;

        // 限制垂直视角角度
        xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);
    }

    private void UpdateCameraTransform()
    {
        // 计算目标位置（车辆位置 + 偏移量）
        targetPosition = vehicle.TransformPoint(cameraPositionOffset);

        // 碰撞检测
        if (enableCollisionDetection)
        {
            HandleCameraCollision();
        }

        // 平滑移动摄像机到目标位置
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref positionVelocity,
            positionSmoothTime
        );

        // 计算目标旋转（基于车辆旋转 + 鼠标控制的额外旋转）
        Quaternion vehicleRotation = vehicle.rotation;
        targetRotation = vehicleRotation * Quaternion.Euler(xRotation, yRotation, 0);

        // 平滑旋转摄像机
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSmoothTime * Time.deltaTime * 10f
        );
    }

    private void HandleCameraCollision()
    {
        // 从车辆位置向摄像机目标位置发射球体检测
        Vector3 dir = targetPosition - vehicle.position;
        float distance = dir.magnitude;

        if (Physics.SphereCast(
            vehicle.position,
            collisionRadius,
            dir.normalized,
            out RaycastHit hit,
            distance,
            collisionMask))
        {
            // 如果检测到碰撞，调整摄像机位置
            targetPosition = hit.point + hit.normal * collisionOffset;
        }
    }

    // 在编辑器中绘制调试信息
    private void OnDrawGizmosSelected()
    {
        if (vehicle != null)
        {
            // 绘制摄像机目标位置
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(vehicle.TransformPoint(cameraPositionOffset), 0.1f);

            // 绘制碰撞检测范围
            if (enableCollisionDetection)
            {
                Gizmos.color = Color.yellow;
                Vector3 dir = vehicle.TransformPoint(cameraPositionOffset) - vehicle.position;
                Gizmos.DrawWireSphere(vehicle.position + dir.normalized * collisionRadius, collisionRadius);
            }
        }
    }
}
