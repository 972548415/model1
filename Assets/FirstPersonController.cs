using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f; // 使用真实重力值

    private CharacterController controller;
    private Camera playerCamera;
    private float verticalRotation = 0f;
    private Vector3 playerVelocity; // 用于处理跳跃和重力

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>(); // 获取子物体中的摄像机

        // 锁定鼠标到屏幕中心并隐藏它
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 左右旋转：绕Y轴旋转玩家本体
        transform.Rotate(0, mouseX, 0);

        // 上下旋转：绕X轴旋转摄像机（注意是负值，因为鼠标Y向上是正，但我们要向上看是绕X轴正旋转）
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f); // 限制上下视角范围，避免脖子360度旋转
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    void HandleMovement()
    {
        // 如果角色在地面上，重置Y轴速度，否则应用重力
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // 一个小小的向下的力，确保角色稳稳地站在地上
        }

        // 获取键盘输入
        float moveX = Input.GetAxis("Horizontal"); // A/D 对应 -1/1
        float moveZ = Input.GetAxis("Vertical");   // S/W 对应 -1/1

        // 根据玩家当前的朝向（transform.forward）来移动
        Vector3 moveDirection = (transform.right * moveX) + (transform.forward * moveZ);
        controller.Move(moveDirection * walkSpeed * Time.deltaTime);

        // 跳跃逻辑
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // 物理公式：v = sqrt(2gh)
        }

        // 应用重力
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}