using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 镜头抖动 : MonoBehaviour
{
    private Vector3 _originalPos;    // 相机初始位置
    private Quaternion _originalRot; // 相机初始旋转
    private float _shakeDuration = 0f; // 震动持续时间
    private float _shakeIntensity = 0.1f; // 震动强度
    private float _dampingSpeed = 1.5f;   // 震动衰减速度

    void Start()
    {
        _originalPos = transform.localPosition;
        _originalRot = transform.localRotation;
    }

    void Update()
    {
        if (_shakeDuration > 0)
        {
            // 使用Perlin噪声生成平滑随机偏移
            float x = Mathf.PerlinNoise(Time.time * 10, 0) * 2 - 1;
            float y = Mathf.PerlinNoise(0, Time.time * 10) * 2 - 1;
            float z = Mathf.PerlinNoise(Time.time * 10, Time.time * 10) * 2 - 1;

            // 更新相机位置和旋转
            transform.localPosition = _originalPos + new Vector3(x, y, z) * _shakeIntensity;
            transform.localRotation = _originalRot * Quaternion.Euler(x * 2, y * 2, z * 2);

            // 随时间衰减震动强度
            _shakeDuration -= Time.deltaTime * _dampingSpeed;
        }
        else
        {
            // 恢复初始状态
            transform.localPosition = _originalPos;
            transform.localRotation = _originalRot;
        }
    }

    // 外部调用接口：触发地震效果
    public void TriggerShake(float duration, float intensity)
    {
        _shakeDuration = duration;
        _shakeIntensity = intensity;
    }
}
