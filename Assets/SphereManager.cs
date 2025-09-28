using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 以后脚本名和类名一定保持这个风格，使用驼峰命名法
 */
public class SphereManager : MonoBehaviour
{
    /*
     * 变量名也尽量使用驼峰命名法
     */
    public GameObject spherePrefab;
    public int gridSize = 5;
    public float spacing = 2.0f;

    void Start()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                Vector3 position = new Vector3(x * spacing, 0, z * spacing);
                /*
                 * 下面这一行在运行时报空
                 * 在初始化逻辑中，应添加保护，如何资源文件为空，及时弹出错误日志
                 */
                GameObject sphere = Instantiate(spherePrefab, position, Quaternion.identity);
                sphere.name = "Sphere_" + x + "_" + z;
            }
        }
    }

    /*
     * 这里IEnumerator没有拉起来，可以结合Unity文档和大模型继续研究下
     */
    private IEnumerator MoveSphereRandomly(GameObject sphere)
    {
        while (true)
        {
            Vector3 newPosition = sphere.transform.position;
            newPosition.x = Random.Range(-5f, 5f);
            newPosition.z = Random.Range(-5f, 5f);
            sphere.transform.position = newPosition;
            /*
             * 时间间隔作为常量可以抽出来，在类中进行声明
             */
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator LogSpherePosition(GameObject sphere)
    {
        while (true)
        {
            Debug.Log(sphere.name + " Position: " + sphere.transform.position);
            /*
            * 时间间隔作为常量可以抽出来，在类中进行声明
            */
            yield return new WaitForSeconds(0.5f);
        }
    }

    void Update()
    {
        
    }
}
