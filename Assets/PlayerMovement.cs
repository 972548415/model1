using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public float moveSpeed = 5f;      // �ƶ��ٶ�
    public float rotationSpeed = 2f; // �����ת�ٶ�
    public float jumpForce = 5f;     // ��Ծ������ѡ��

    private CharacterController controller;
    private float verticalRotation = 0f;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; // ������굽��Ļ����
    }

    void Update()
    {
        // --- �������ӽ� ---
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f); // ���������ӽǷ�Χ

        transform.Rotate(0, mouseX, 0); // ������ת��Y�ᣩ
        Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0); // ������ת��X�ᣩ

        // --- ���̿����ƶ� ---
        float horizontal = Input.GetAxis("Horizontal"); // A/D ����
        float vertical = Input.GetAxis("Vertical");     // W/S ǰ��

        Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;
        controller.SimpleMove(moveDirection * moveSpeed); // Ӧ���ƶ�

        // --- ��Ծ����ѡ��---
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = jumpForce;
        }

        // ����ģ��
        velocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
