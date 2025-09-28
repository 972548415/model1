using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCNavigator : MonoBehaviour
{
    public Transform[] exitPoints; // 拖拽所有出口到Inspector数组
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(UpdateExitRoutine());
    }

    IEnumerator UpdateExitRoutine()
    {
        while (true)
        {
            FindClosestExit();
            yield return new WaitForSeconds(1f); // 每1秒检测一次
        }
    }

    void FindClosestExit()
    {
        Transform closestExit = null;
        float minDistance = Mathf.Infinity;

        // 遍历所有出口，找到最近的
        foreach (Transform exit in exitPoints)
        {
            float distance = Vector3.Distance(transform.position, exit.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestExit = exit;
            }
        }

        if (closestExit != null)
            agent.SetDestination(closestExit.position);
    }

    // 可选：定期更新目标（如出口位置变化时）
    void Update()
    {
        // if (需要动态更新) FindClosestExit();
    }
}