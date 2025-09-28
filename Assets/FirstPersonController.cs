using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f; // ʹ����ʵ����ֵ

    private CharacterController controller;
    private Camera playerCamera;
    private float verticalRotation = 0f;
    private Vector3 playerVelocity; // ���ڴ�����Ծ������

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>(); // ��ȡ�������е������

        // ������굽��Ļ���Ĳ�������
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
        // ��ȡ�������
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // ������ת����Y����ת��ұ���
        transform.Rotate(0, mouseX, 0);

        // ������ת����X����ת�������ע���Ǹ�ֵ����Ϊ���Y����������������Ҫ���Ͽ�����X������ת��
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f); // ���������ӽǷ�Χ�����ⲱ��360����ת
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    void HandleMovement()
    {
        // �����ɫ�ڵ����ϣ�����Y���ٶȣ�����Ӧ������
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // һ��СС�����µ�����ȷ����ɫ���ȵ�վ�ڵ���
        }

        // ��ȡ��������
        float moveX = Input.GetAxis("Horizontal"); // A/D ��Ӧ -1/1
        float moveZ = Input.GetAxis("Vertical");   // S/W ��Ӧ -1/1

        // ������ҵ�ǰ�ĳ���transform.forward�����ƶ�
        Vector3 moveDirection = (transform.right * moveX) + (transform.forward * moveZ);
        controller.Move(moveDirection * walkSpeed * Time.deltaTime);

        // ��Ծ�߼�
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // ����ʽ��v = sqrt(2gh)
        }

        // Ӧ������
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}