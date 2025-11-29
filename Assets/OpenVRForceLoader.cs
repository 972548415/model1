using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class OpenVRForceLoader : MonoBehaviour
{
    [Header("Settings")]
    public bool enableVRAtStart = true;
    public string deviceToLoad = "OpenVR"; // 使用 "OpenVR" 或 ""

    void Start()
    {
        if (enableVRAtStart)
        {
            StartCoroutine(LoadVRDevice(deviceToLoad));
        }
    }

    /// <summary>
    /// 在运行时加载VR设备
    /// </summary>
    public IEnumerator LoadVRDevice(string newDevice)
    {
        Debug.Log("开始加载VR设备: " + newDevice);

        // 首先加载指定的VR设备[citation:9]
        XRSettings.LoadDeviceByName(newDevice);

        // 等待一帧让设备初始化[citation:9]
        yield return null;

        // 启用XR设置[citation:9]
        XRSettings.enabled = true;

        Debug.Log("VR设备加载完成: " + XRSettings.loadedDeviceName);
        Debug.Log("XR是否启用: " + XRSettings.enabled);

        // 检查是否成功初始化
        if (XRSettings.loadedDeviceName == newDevice && XRSettings.enabled)
        {
            OnVRInitializedSuccess();
        }
        else
        {
            OnVRInitializedFailed();
        }
    }

    /// <summary>
    /// 禁用VR
    /// </summary>
    public void DisableVR()
    {
        StartCoroutine(LoadVRDevice(""));
    }

    /// <summary>
    /// 重新启用VR
    /// </summary>
    public void EnableVR()
    {
        StartCoroutine(LoadVRDevice(deviceToLoad));
    }

    private void OnVRInitializedSuccess()
    {
        Debug.Log("VR初始化成功！当前设备: " + XRSettings.loadedDeviceName);

        // 这里可以添加VR初始化成功后的逻辑
        // 例如：启用VR相关的游戏对象、调整相机设置等
    }

    private void OnVRInitializedFailed()
    {
        Debug.LogError("VR初始化失败！当前设备: " + XRSettings.loadedDeviceName);

        // 这里可以添加失败处理逻辑
        // 例如：回退到非VR模式、显示错误信息等
    }

    void Update()
    {
        // 调试用：按V键切换VR状态
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (XRSettings.enabled)
            {
                DisableVR();
            }
            else
            {
                EnableVR();
            }
        }
    }
}