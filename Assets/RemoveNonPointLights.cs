using UnityEngine;
using UnityEditor; // 必须引用UnityEditor命名空间
using System.Collections.Generic;

public class RemoveNonPointLights : EditorWindow
{
    [MenuItem("Tools/灯光/删除所有非点光源")] // 在Unity顶部菜单栏创建路径
    public static void ShowWindow()
    {
        GetWindow<RemoveNonPointLights>("删除非点光源"); // 创建一个自定义窗口
    }

    void OnGUI()
    {
        GUILayout.Label("删除场景中所有非点光源（Point Light）", EditorStyles.boldLabel);

        if (GUILayout.Button("查找并删除非点光源"))
        {
            FindAndRemoveLights();
        }

        if (GUILayout.Button("仅查找并列出光源"))
        {
            FindAndListLights();
        }
    }

    void FindAndRemoveLights()
    {
        // 1. 找到场景中所有光源对象
        Light[] allLights = FindObjectsOfType<Light>();
        List<Light> lightsToDelete = new List<Light>();

        // 2. 遍历所有光源，筛选出非点光源
        foreach (Light light in allLights)
        {
            if (light.type != LightType.Point) // 如果不是点光源
            {
                lightsToDelete.Add(light);
            }
        }

        // 3. 记录日志并删除
        if (lightsToDelete.Count == 0)
        {
            Debug.Log("场景中没有找到非点光源。");
            return;
        }

        // 开始记录可撤销操作
        Undo.RecordObjects(lightsToDelete.ToArray(), "Delete Non-Point Lights");

        foreach (Light light in lightsToDelete)
        {
            Debug.Log("正在删除非点光源: " + light.name + " (类型: " + light.type + ")", light);
            DestroyImmediate(light.gameObject); // 立即销毁光源所属的游戏对象
            // 如果只想删除Light组件而保留游戏对象，使用：
            // DestroyImmediate(light);
        }

        Debug.Log($"已完成！共删除了 {lightsToDelete.Count} 个非点光源。");
    }

    void FindAndListLights()
    {
        // 这个功能只查找并列出光源，不删除，用于安全检查
        Light[] allLights = FindObjectsOfType<Light>();

        Debug.Log("=== 场景光源列表 ===");
        foreach (Light light in allLights)
        {
            Debug.Log("光源: " + light.name + " | 类型: " + light.type, light);
        }
        Debug.Log("=== 列表结束 ===");
    }
}