using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TunnelGenerator : MonoBehaviour
{
    public GameObject tunnelSegment; // 拱形截面预制体
    public int segmentCount = 20;    // 分段数
    public float curveRadius = 5f;   // S 形弯曲半径

    void Start()
    {
        for (int i = 0; i < segmentCount; i++)
        {
            // 计算 S 形路径点（正弦曲线）
            float x = Mathf.Sin(i * 0.3f) * curveRadius;
            float z = i * 1f;
            Vector3 pos = new Vector3(x, 0, z);

            // 生成隧道段
            GameObject segment = Instantiate(tunnelSegment, pos, Quaternion.identity);
            segment.transform.parent = this.transform;
        }
    }
}