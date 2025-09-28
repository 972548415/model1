using UnityEngine;
using UnityEditor; // ��������UnityEditor�����ռ�
using System.Collections.Generic;

public class RemoveNonPointLights : EditorWindow
{
    [MenuItem("Tools/�ƹ�/ɾ�����зǵ��Դ")] // ��Unity�����˵�������·��
    public static void ShowWindow()
    {
        GetWindow<RemoveNonPointLights>("ɾ���ǵ��Դ"); // ����һ���Զ��崰��
    }

    void OnGUI()
    {
        GUILayout.Label("ɾ�����������зǵ��Դ��Point Light��", EditorStyles.boldLabel);

        if (GUILayout.Button("���Ҳ�ɾ���ǵ��Դ"))
        {
            FindAndRemoveLights();
        }

        if (GUILayout.Button("�����Ҳ��г���Դ"))
        {
            FindAndListLights();
        }
    }

    void FindAndRemoveLights()
    {
        // 1. �ҵ����������й�Դ����
        Light[] allLights = FindObjectsOfType<Light>();
        List<Light> lightsToDelete = new List<Light>();

        // 2. �������й�Դ��ɸѡ���ǵ��Դ
        foreach (Light light in allLights)
        {
            if (light.type != LightType.Point) // ������ǵ��Դ
            {
                lightsToDelete.Add(light);
            }
        }

        // 3. ��¼��־��ɾ��
        if (lightsToDelete.Count == 0)
        {
            Debug.Log("������û���ҵ��ǵ��Դ��");
            return;
        }

        // ��ʼ��¼�ɳ�������
        Undo.RecordObjects(lightsToDelete.ToArray(), "Delete Non-Point Lights");

        foreach (Light light in lightsToDelete)
        {
            Debug.Log("����ɾ���ǵ��Դ: " + light.name + " (����: " + light.type + ")", light);
            DestroyImmediate(light.gameObject); // �������ٹ�Դ��������Ϸ����
            // ���ֻ��ɾ��Light�����������Ϸ����ʹ�ã�
            // DestroyImmediate(light);
        }

        Debug.Log($"����ɣ���ɾ���� {lightsToDelete.Count} ���ǵ��Դ��");
    }

    void FindAndListLights()
    {
        // �������ֻ���Ҳ��г���Դ����ɾ�������ڰ�ȫ���
        Light[] allLights = FindObjectsOfType<Light>();

        Debug.Log("=== ������Դ�б� ===");
        foreach (Light light in allLights)
        {
            Debug.Log("��Դ: " + light.name + " | ����: " + light.type, light);
        }
        Debug.Log("=== �б���� ===");
    }
}