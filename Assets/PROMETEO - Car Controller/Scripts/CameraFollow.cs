using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform carTransform;          // 汽车对象
    public Vector3 inCarCameraOffset = new Vector3(0, 1.5f, 0.5f); // 车内摄像机偏移（相对于车）

    [Range(1, 10)]
    public float followSpeed = 2;          // 跟随速度
    [Range(1, 10)]
    public float lookSpeed = 5;            // 注视速度
    [Range(0.1f, 5)]
    public float mouseSensitivity = 2f;    // 鼠标灵敏度

    private Vector3 initialCameraPosition; // 初始摄像机位置
    private Vector3 initialCarPosition;    // 初始汽车位置
    private Vector3 absoluteInitCameraPosition;
    private bool isInCarView = false;      // 是否处于车内视角
    private float xRotation = 0;           // 鼠标控制的X轴旋转
    private float yRotation = 0;           // 鼠标控制的Y轴旋转

    void Start()
    {
        initialCameraPosition = transform.position;
        initialCarPosition = carTransform.position;
        absoluteInitCameraPosition = initialCameraPosition - initialCarPosition;

        // 锁定鼠标到屏幕中心并隐藏
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 按V键切换视角
        if (Input.GetKeyDown(KeyCode.V))
        {
            isInCarView = !isInCarView;
        }

        if (isInCarView)
        {
            // 车内视角：摄像机固定在车内，通过鼠标控制旋转
            HandleInCarView();
        }
    }

    void FixedUpdate()
    {
        if (!isInCarView)
        {
            // 原跟随逻辑
            HandleFollowView();
        }
    }

    // 原跟随视角逻辑
    void HandleFollowView()
    {
        // 注视汽车
        Vector3 lookDirection = carTransform.position - transform.position;
        Quaternion rot = Quaternion.LookRotation(lookDirection, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, lookSpeed * Time.deltaTime);

        // 移动到汽车
        Vector3 targetPos = absoluteInitCameraPosition + carTransform.position;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }

    // 车内视角逻辑
    void HandleInCarView()
    {
        // 摄像机固定在车内位置
        transform.position = carTransform.TransformPoint(inCarCameraOffset);

        // 鼠标控制旋转
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -30f, 30f); // 限制上下旋转角度

        // 应用旋转（相对于车的方向）
        Quaternion carRotation = carTransform.rotation;
        transform.rotation = carRotation * Quaternion.Euler(xRotation, yRotation, 0);
    }
}
