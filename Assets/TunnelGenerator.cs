using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TunnelGenerator : MonoBehaviour
{
    public GameObject tunnelSegment; // ���ν���Ԥ����
    public int segmentCount = 20;    // �ֶ���
    public float curveRadius = 5f;   // S �������뾶

    void Start()
    {
        for (int i = 0; i < segmentCount; i++)
        {
            // ���� S ��·���㣨�������ߣ�
            float x = Mathf.Sin(i * 0.3f) * curveRadius;
            float z = i * 1f;
            Vector3 pos = new Vector3(x, 0, z);

            // ���������
            GameObject segment = Instantiate(tunnelSegment, pos, Quaternion.identity);
            segment.transform.parent = this.transform;
        }
    }
}