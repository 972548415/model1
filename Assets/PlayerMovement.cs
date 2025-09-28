using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public float moveSpeed = 5f;      // 移动速度
    public float rotationSpeed = 2f; // 鼠标旋转速度
    public float jumpForce = 5f;     // 跳跃力（可选）

    private CharacterController controller;
    private float verticalRotation = 0f;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; // 锁定鼠标到屏幕中心
    }

    void Update()
    {
        // --- 鼠标控制视角 ---
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f); // 限制上下视角范围

        transform.Rotate(0, mouseX, 0); // 左右旋转（Y轴）
        Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0); // 上下旋转（X轴）

        // --- 键盘控制移动 ---
        float horizontal = Input.GetAxis("Horizontal"); // A/D 左右
        float vertical = Input.GetAxis("Vertical");     // W/S 前后

        Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;
        controller.SimpleMove(moveDirection * moveSpeed); // 应用移动

        // --- 跳跃（可选）---
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = jumpForce;
        }

        // 重力模拟
        velocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
