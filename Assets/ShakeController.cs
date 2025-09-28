using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeController : MonoBehaviour
{
    public ��ͷ���� CameraShake; // ���� CameraShake �ű�

    public float shakeDuration = 1f; // �𶯳���ʱ��
    public float shakeIntensity = 0.1f; // ��ǿ��

    void Start()
    {
        // ��ȡ����ϵ� CameraShake ���
        CameraShake = GetComponent<��ͷ����>();
    }

    void Update()
    {
        // �����¿ո��ʱ������ͷ����
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CameraShake.TriggerShake(shakeDuration, shakeIntensity);
        }
    }
}