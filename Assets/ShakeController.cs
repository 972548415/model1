using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeController : MonoBehaviour
{
    public 镜头抖动 CameraShake; // 引用 CameraShake 脚本

    public float shakeDuration = 1f; // 震动持续时间
    public float shakeIntensity = 0.1f; // 震动强度

    void Start()
    {
        // 获取相机上的 CameraShake 组件
        CameraShake = GetComponent<镜头抖动>();
    }

    void Update()
    {
        // 当按下空格键时触发镜头抖动
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CameraShake.TriggerShake(shakeDuration, shakeIntensity);
        }
    }
}