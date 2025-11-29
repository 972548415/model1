using UnityEngine;
using Valve.VR;

public class VehiclePlayerController : MonoBehaviour
{
    [Header("VR Player References")]
    public Transform playerHead; // SteamVR Camera (head)
    public Transform vrRigRoot; // 整个SteamVR Player根对象 [SteamVR]

    [Header("Vehicle References")]
    public Transform driverSeat; // 驾驶座位置标记（空对象）
    public PrometeoCarController carController; // 原有的车辆控制脚本

    [Header("VR Position Settings")]
    public bool useSmoothMovement = true;
    public float smoothTime = 0.1f;
    public bool maintainWorldUp = true; // 保持世界坐标系上方向

    [Header("SteamVR Input Settings")]
    public SteamVR_Action_Vector2 moveAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("Move");
    public SteamVR_Action_Boolean brakeAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Brake");
    public SteamVR_Action_Boolean handbrakeAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Handbrake");
    public SteamVR_Action_Boolean resetPositionAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("ResetPosition");

    [Header("VR Input Sensitivity")]
    public float steeringSensitivity = 1.0f;
    public float throttleSensitivity = 1.0f;
    public float deadZone = 0.1f;

    // 私有变量
    private Vector3 lastVehiclePosition;
    private Quaternion lastVehicleRotation;
    private Vector3 velocity = Vector3.zero;
    private bool isInitialized = false;

    // 输入状态
    private float vrThrottleInput = 0f;
    private float vrSteeringInput = 0f;
    private bool vrBrakeInput = false;
    private bool vrHandbrakeInput = false;

    void Start()
    {
        InitializeVRPlayer();
    }

    void InitializeVRPlayer()
    {
        // 自动查找缺失的引用
        AutoFindReferences();

        // 验证必要的组件
        if (playerHead == null)
        {
            Debug.LogError("Player Head reference is missing! Please assign SteamVR Camera.");
            return;
        }

        if (vrRigRoot == null)
        {
            Debug.LogError("VR Rig Root reference is missing! Please assign [SteamVR] object.");
            return;
        }

        if (carController == null)
        {
            Debug.LogError("PrometeoCarController reference is missing! Please assign the car controller.");
            return;
        }

        // 禁用原有的输入方式
        carController.useGamepad = false;
        carController.useTouchControls = false;

        // 初始化位置
        InitializePlayerPosition();

        // 记录初始状态
        lastVehiclePosition = transform.position;
        lastVehicleRotation = transform.rotation;

        isInitialized = true;
        Debug.Log("Vehicle VR Player Controller initialized successfully.");
    }

    void AutoFindReferences()
    {
        // 自动查找SteamVR组件
        if (playerHead == null)
        {
            GameObject cameraHead = GameObject.Find("Camera (head)");
            if (cameraHead != null) playerHead = cameraHead.transform;
        }

        if (vrRigRoot == null)
        {
            GameObject steamVR = GameObject.Find("[SteamVR]");
            if (steamVR != null) vrRigRoot = steamVR.transform;
        }

        // 自动查找车辆控制脚本
        if (carController == null)
        {
            carController = GetComponent<PrometeoCarController>();
            if (carController == null)
            {
                carController = GetComponentInChildren<PrometeoCarController>();
            }
        }

        // 自动查找驾驶座
        if (driverSeat == null)
        {
            driverSeat = transform.Find("DriverSeat");
            if (driverSeat == null)
            {
                // 创建默认驾驶座
                GameObject seat = new GameObject("DriverSeat");
                driverSeat = seat.transform;
                driverSeat.SetParent(transform);
                driverSeat.localPosition = new Vector3(0, 1f, 0.5f);
                driverSeat.localRotation = Quaternion.identity;
                Debug.Log("Created default DriverSeat object. Please adjust its position in the inspector.");
            }
        }
    }

    void InitializePlayerPosition()
    {
        if (vrRigRoot != null && driverSeat != null)
        {
            // 将整个VR Rig移动到驾驶座位置
            vrRigRoot.position = driverSeat.position;
            vrRigRoot.rotation = driverSeat.rotation;
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        // 处理VR输入
        HandleVRInput();

        // 处理重置位置输入
        if (resetPositionAction != null && resetPositionAction.GetStateDown(SteamVR_Input_Sources.Any))
        {
            ResetPlayerPosition();
        }

        // 将VR输入传递给车辆控制器
        ApplyVRInputToCar();
    }

    void LateUpdate()
    {
        if (!isInitialized) return;

        // 在LateUpdate中更新位置，确保在车辆移动之后执行
        UpdatePlayerPosition();
    }

    void HandleVRInput()
    {
        // 处理移动输入（摇杆）
        if (moveAction != null)
        {
            Vector2 input = moveAction.GetAxis(SteamVR_Input_Sources.Any);

            // 应用死区
            if (Mathf.Abs(input.x) < deadZone) input.x = 0f;
            if (Mathf.Abs(input.y) < deadZone) input.y = 0f;

            // 应用灵敏度
            vrSteeringInput = Mathf.Clamp(input.x * steeringSensitivity, -1f, 1f);
            vrThrottleInput = Mathf.Clamp(input.y * throttleSensitivity, -1f, 1f);
        }

        // 处理刹车输入
        if (brakeAction != null)
        {
            vrBrakeInput = brakeAction.GetState(SteamVR_Input_Sources.Any);
        }

        // 处理手刹输入
        if (handbrakeAction != null)
        {
            vrHandbrakeInput = handbrakeAction.GetState(SteamVR_Input_Sources.Any);
        }
    }

    void ApplyVRInputToCar()
    {
        if (carController == null) return;

        // 根据VR输入调用PrometeoCarController的方法
        if (vrThrottleInput > 0.1f)
        {
            // 前进
            carController.CancelInvoke("DecelerateCar");
            carController.ThrottleOff(); // 先停止当前输入
            SimulateKeyPress(KeyCode.W); // 模拟W键按下
        }
        else if (vrThrottleInput < -0.1f)
        {
            // 后退
            carController.CancelInvoke("DecelerateCar");
            carController.ThrottleOff(); // 先停止当前输入
            SimulateKeyPress(KeyCode.S); // 模拟S键按下
        }
        else
        {
            // 无油门输入
            carController.ThrottleOff();
        }

        // 转向输入
        if (vrSteeringInput < -0.1f)
        {
            carController.TurnLeft();
        }
        else if (vrSteeringInput > 0.1f)
        {
            carController.TurnRight();
        }
        else
        {
            if (Mathf.Abs(carController.frontLeftCollider.steerAngle) > 1f)
            {
                carController.ResetSteeringAngle();
            }
        }

        // 刹车输入
        if (vrBrakeInput)
        {
            carController.Brakes();
        }

        // 手刹输入
        if (vrHandbrakeInput)
        {
            carController.CancelInvoke("DecelerateCar");
            carController.Handbrake();
        }
        else
        {
            carController.RecoverTraction();
        }

        // 自动减速（当没有输入时）
        if (Mathf.Abs(vrThrottleInput) < 0.1f && !vrBrakeInput && !vrHandbrakeInput)
        {
            carController.InvokeRepeating("DecelerateCar", 0f, 0.1f);
        }
    }

    void SimulateKeyPress(KeyCode key)
    {
        // 由于PrometeoCarController依赖于Input.GetKey，我们需要直接调用对应的方法
        switch (key)
        {
            case KeyCode.W:
                carController.GoForward();
                break;
            case KeyCode.S:
                carController.GoReverse();
                break;
        }
    }

    void UpdatePlayerPosition()
    {
        if (playerHead == null) return;

        // 计算车辆的运动增量
        Vector3 positionDelta = transform.position - lastVehiclePosition;
        Quaternion rotationDelta = transform.rotation * Quaternion.Inverse(lastVehicleRotation);

        if (useSmoothMovement)
        {
            // 平滑移动VR Rig
            Vector3 targetPosition = vrRigRoot.position + positionDelta;
            vrRigRoot.position = Vector3.SmoothDamp(vrRigRoot.position, targetPosition, ref velocity, smoothTime);

            // 旋转处理 - 只旋转VR Rig，不旋转头部
            if (!maintainWorldUp)
            {
                vrRigRoot.rotation = rotationDelta * vrRigRoot.rotation;
            }
        }
        else
        {
            // 直接应用位置变化到VR Rig
            vrRigRoot.position += positionDelta;

            if (!maintainWorldUp)
            {
                vrRigRoot.rotation = rotationDelta * vrRigRoot.rotation;
            }
        }

        // 更新记录的位置和旋转
        lastVehiclePosition = transform.position;
        lastVehicleRotation = transform.rotation;
    }

    // 重置玩家位置到驾驶座
    public void ResetPlayerPosition()
    {
        if (!isInitialized) return;

        InitializePlayerPosition();
        lastVehiclePosition = transform.position;
        lastVehicleRotation = transform.rotation;
        velocity = Vector3.zero;

        Debug.Log("Player position reset to driver seat.");
    }

    // 动态重新定位驾驶座（用于调整座位位置）
    public void RepositionDriverSeat(Vector3 localPosition, Quaternion localRotation)
    {
        if (driverSeat != null)
        {
            driverSeat.localPosition = localPosition;
            driverSeat.localRotation = localRotation;
            ResetPlayerPosition();
        }
    }

    // 获取当前头部相对于车辆的本地位置（用于调试）
    public Vector3 GetHeadLocalPosition()
    {
        if (playerHead != null && driverSeat != null)
        {
            return driverSeat.InverseTransformPoint(playerHead.position);
        }
        return Vector3.zero;
    }

    // 启用/禁用平滑移动
    public void SetSmoothMovement(bool enabled)
    {
        useSmoothMovement = enabled;
        if (!enabled) velocity = Vector3.zero;
    }

    // 获取当前VR输入状态（用于UI显示）
    public void GetVRInputState(out float throttle, out float steering, out bool brake, out bool handbrake)
    {
        throttle = vrThrottleInput;
        steering = vrSteeringInput;
        brake = vrBrakeInput;
        handbrake = vrHandbrakeInput;
    }

    // 在Inspector中调用的方法
    [ContextMenu("Reset VR Player Position")]
    void EditorResetPosition()
    {
        if (Application.isPlaying)
        {
            ResetPlayerPosition();
        }
    }

    [ContextMenu("Auto Setup References")]
    void EditorAutoSetup()
    {
        AutoFindReferences();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    void OnDrawGizmosSelected()
    {
        // 在Scene视图中显示驾驶座位置
        if (driverSeat != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(driverSeat.position, 0.1f);
            Gizmos.DrawRay(driverSeat.position, driverSeat.forward * 0.3f);
        }
    }
}