using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCNavigator : MonoBehaviour
{
    public Transform[] exitPoints; // ��ק���г��ڵ�Inspector����
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
            yield return new WaitForSeconds(1f); // ÿ1����һ��
        }
    }

    void FindClosestExit()
    {
        Transform closestExit = null;
        float minDistance = Mathf.Infinity;

        // �������г��ڣ��ҵ������
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

    // ��ѡ�����ڸ���Ŀ�꣨�����λ�ñ仯ʱ��
    void Update()
    {
        // if (��Ҫ��̬����) FindClosestExit();
    }
}