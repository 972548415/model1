using System.IO;
using UnityEditor;
using UnityEngine;

public class LargeFileFinder : MonoBehaviour
{
    [MenuItem("Assets/查找工程中过大的文件", false, 2000)]
    private static void FindLargeFiles()
    {
        // 指定要扫描的文件夹路径
        string folderPath = "Assets";

        // 获取文件信息
        DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
        FileInfo[] files = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);

        // 遍历文件并查找超过 100 MB 的文件
        foreach (FileInfo file in files)
        {
            if (file.Length > 100 * 1024 * 1024) // 大于 100 MB
            {
                Debug.Log($"File: {file.Name} | Size: {file.Length / (1024 * 1024)} MB\nFull Path: {file.FullName}");
            }
        }

        Debug.Log("完成检索大文件");
    }
}
