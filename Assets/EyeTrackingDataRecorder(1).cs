using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace ViveSR.anipal.Eye
{
    public class EyeTrackingDataRecorder : MonoBehaviour
    {
        [Header("数据记录设置")]
        public bool enableRecording = true;
        public Transform targetObject1; // 拖拽指定的目标物体到这里

        [Header("射线检测设置")]
        public LayerMask raycastLayerMask = -1; // 默认检测所有层
        public float maxRayDistance = 50f;

        [Header("射线可视化设置")]
        public bool showGazeRay = true;
        public Material rayMaterial;
        public Color rayColor = Color.red;
        public float rayWidth = 0.01f;
        public GameObject hitMarkerPrefab; // 拖拽碰撞点标记预制体到这里

        // 文件记录相关
        private StreamWriter dataWriter;
        private string filePath;
        private int frameCount = 0;
        private bool isRecording = false;

        // 眼动数据相关
        private static EyeData_v2 eyeData = new EyeData_v2(); // 改为静态
        private bool eye_callback_registered = false;
        private VerboseData verboseData;

        // 射线可视化相关
        private LineRenderer gazeRayRenderer;
        private GameObject hitMarkerInstance;

        void Start()
        {
            // 检查眼动框架
            if (!SRanipal_Eye_Framework.Instance.EnableEye)
            {
                enabled = false;
                return;
            }

            // 初始化射线可视化
            InitializeRayVisualization();

            // 初始化数据记录
            InitializeDataRecording();
        }

        void Update()
        {
            // 检查眼动框架状态
            if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
                return;

            // 注册/取消注册回调
            HandleEyeCallbackRegistration();

            // 获取眼动数据并记录
            if (enableRecording && isRecording)
            {
                RecordEyeTrackingData();
            }

        }

        void OnDestroy()
        {
            // 清理资源
            ReleaseResources();
        }

        private void InitializeRayVisualization()
        {
            // 添加或获取LineRenderer组件
            gazeRayRenderer = gameObject.GetComponent<LineRenderer>();
            if (gazeRayRenderer == null)
            {
                gazeRayRenderer = gameObject.AddComponent<LineRenderer>();
            }

            // 配置LineRenderer
            ConfigureLineRenderer();

            // 实例化碰撞点标记
            if (hitMarkerPrefab != null)
            {
                hitMarkerInstance = Instantiate(hitMarkerPrefab);
                hitMarkerInstance.SetActive(false);
            }
            else
            {
                Debug.LogWarning("未分配碰撞点标记预制体，将无法显示碰撞点标记");
            }
        }

        private void ConfigureLineRenderer()
        {
            if (gazeRayRenderer == null) return;

            // 基础设置
            gazeRayRenderer.positionCount = 2;
            gazeRayRenderer.useWorldSpace = true;

            // 材质和颜色
            if (rayMaterial != null)
            {
                gazeRayRenderer.material = rayMaterial;
            }
            else
            {
                // 创建默认材质
                gazeRayRenderer.material = new Material(Shader.Find("Sprites/Default"));
                gazeRayRenderer.material.color = rayColor;
            }

            // 宽度设置
            gazeRayRenderer.startWidth = rayWidth;
            gazeRayRenderer.endWidth = rayWidth;

            // 初始位置
            gazeRayRenderer.SetPosition(0, Vector3.zero);
            gazeRayRenderer.SetPosition(1, Vector3.zero);

            // 初始状态
            gazeRayRenderer.enabled = showGazeRay;
        }

        private void InitializeDataRecording()
        {
            // 创建数据文件夹
            string folderPath = Path.Combine(Application.dataPath, "../EyeTrackingData");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // 创建CSV文件
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            filePath = Path.Combine(folderPath, $"EyeTrackingData_{timestamp}.csv");

            try
            {
                dataWriter = new StreamWriter(filePath, false, Encoding.UTF8);
                WriteCSVHeader();
                isRecording = true;
                Debug.Log($"开始记录眼动数据到: {filePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"创建数据文件失败: {e.Message}");
                enableRecording = false;
            }
        }

        private void WriteCSVHeader()
        {
            var header = new List<string>
            {
                "时间戳",
                "帧计数器",
                "游戏时间",
                "挂载对象位置X", "挂载对象位置Y", "挂载对象位置Z",
                "挂载对象旋转X", "挂载对象旋转Y", "挂载对象旋转Z",
                "目标对象1旋转X", "目标对象1旋转Y", "目标对象1旋转Z",
                "注视原点X", "注视原点Y", "注视原点Z",
                "注视方向X", "注视方向Y", "注视方向Z",
                "碰撞点X", "碰撞点Y", "碰撞点Z",
                "碰撞物体名称",
                "左瞳孔直径", "右瞳孔直径",
                "左瞳孔X", "左瞳孔Y",
                "右瞳孔X", "右瞳孔Y",
                "左眼睁眼度", "右眼睁眼度",
                "眼动追踪状态"
            };

            dataWriter.WriteLine(string.Join(",", header));
            dataWriter.Flush();
        }

        private void HandleEyeCallbackRegistration()
        {
            if (SRanipal_Eye_Framework.Instance.EnableEyeDataCallback && !eye_callback_registered)
            {
                SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                eye_callback_registered = true;
            }
            else if (!SRanipal_Eye_Framework.Instance.EnableEyeDataCallback && eye_callback_registered)
            {
                SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                eye_callback_registered = false;
            }
        }

        private void RecordEyeTrackingData()
        {
            frameCount++;

            // 获取注视射线
            Vector3 gazeOriginLocal, gazeDirectionLocal;
            bool gazeValid = GetGazeRay(out gazeOriginLocal, out gazeDirectionLocal);

            // 转换为世界坐标
            Vector3 gazeOriginWorld = transform.TransformPoint(gazeOriginLocal);
            Vector3 gazeDirectionWorld = transform.TransformDirection(gazeDirectionLocal);

            // 射线检测碰撞
            RaycastHit hit;
            Vector3 collisionPoint = Vector3.zero;
            string collisionObjectName = "None";
            bool hasHit = false;

            if (Physics.Raycast(gazeOriginWorld, gazeDirectionWorld, out hit, maxRayDistance, raycastLayerMask))
            {
                collisionPoint = hit.point;
                collisionObjectName = hit.collider.gameObject.name;
                hasHit = true;
            }
            else
            {
                // 如果没有碰撞，使用射线方向上的最大距离点
                collisionPoint = gazeOriginWorld + gazeDirectionWorld * maxRayDistance;
            }

            // 获取详细眼动数据
            bool verboseDataValid = SRanipal_Eye_v2.GetVerboseData(out verboseData);

            // 记录数据到CSV
            RecordDataToCSV(gazeOriginWorld, gazeDirectionWorld, collisionPoint, collisionObjectName, gazeValid && verboseDataValid);

            // 每10帧更新一次射线可视化（性能优化）
            if (frameCount % 10 == 0 && showGazeRay)
            {
                UpdateRayVisualization(gazeOriginWorld, collisionPoint, hasHit);
            }
        }

        private void UpdateRayVisualization(Vector3 gazeOrigin, Vector3 collisionPoint, bool hasHit)
        {
            // 更新LineRenderer
            if (gazeRayRenderer != null && gazeRayRenderer.enabled)
            {
                gazeRayRenderer.SetPosition(0, gazeOrigin);
                gazeRayRenderer.SetPosition(1, collisionPoint);
            }

            // 更新碰撞点标记
            if (hitMarkerInstance != null)
            {
                hitMarkerInstance.SetActive(hasHit);
                if (hasHit)
                {
                    hitMarkerInstance.transform.position = collisionPoint;
                }
            }
        }

        private bool GetGazeRay(out Vector3 origin, out Vector3 direction)
        {
            if (eye_callback_registered)
            {
                if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out origin, out direction, eyeData)) return true;
                else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out origin, out direction, eyeData)) return true;
                else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out origin, out direction, eyeData)) return true;
            }
            else
            {
                if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out origin, out direction)) return true;
                else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out origin, out direction)) return true;
                else if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out origin, out direction)) return true;
            }

            origin = Vector3.zero;
            direction = Vector3.forward;
            return false;
        }

        private void RecordDataToCSV(Vector3 gazeOrigin, Vector3 gazeDirection, Vector3 collisionPoint, string collisionObjectName, bool trackingValid)
        {
            try
            {
                var data = new List<string>
                {
                    // 基础信息
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"),
                    frameCount.ToString(),
                    Time.time.ToString("F4"),
                    
                    // 变换数据
                    transform.position.x.ToString("F6"),
                    transform.position.y.ToString("F6"),
                    transform.position.z.ToString("F6"),
                    transform.eulerAngles.x.ToString("F6"),
                    transform.eulerAngles.y.ToString("F6"),
                    transform.eulerAngles.z.ToString("F6"),
                    
                    // 目标对象数据
                    targetObject1 ? targetObject1.eulerAngles.x.ToString("F6") : "0",
                    targetObject1 ? targetObject1.eulerAngles.y.ToString("F6") : "0",
                    targetObject1 ? targetObject1.eulerAngles.z.ToString("F6") : "0",
                    
                    // 眼动数据
                    gazeOrigin.x.ToString("F6"),
                    gazeOrigin.y.ToString("F6"),
                    gazeOrigin.z.ToString("F6"),
                    gazeDirection.x.ToString("F6"),
                    gazeDirection.y.ToString("F6"),
                    gazeDirection.z.ToString("F6"),
                    
                    // 碰撞数据
                    collisionPoint.x.ToString("F6"),
                    collisionPoint.y.ToString("F6"),
                    collisionPoint.z.ToString("F6"),
                    EscapeCSVField(collisionObjectName),
                    
                    // 生理数据
                    verboseData.left.pupil_diameter_mm.ToString("F6"),
                    verboseData.right.pupil_diameter_mm.ToString("F6"),
                    verboseData.left.pupil_position_in_sensor_area.x.ToString("F6"),
                    verboseData.left.pupil_position_in_sensor_area.y.ToString("F6"),
                    verboseData.right.pupil_position_in_sensor_area.x.ToString("F6"),
                    verboseData.right.pupil_position_in_sensor_area.y.ToString("F6"),
                    verboseData.left.eye_openness.ToString("F6"),
                    verboseData.right.eye_openness.ToString("F6"),
                    
                    // 追踪状态
                    trackingValid ? "Valid" : "Invalid"
                };

                dataWriter.WriteLine(string.Join(",", data));

                // 每10帧刷新一次缓冲区，平衡性能和数据安全
                if (frameCount % 10 == 0)
                {
                    dataWriter.Flush();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"记录数据失败: {e.Message}");
            }
        }

        private string EscapeCSVField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "\"\"";

            // 如果字段包含逗号、换行或引号，需要用引号包围并转义内部引号
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }

        private void ReleaseResources()
        {
            // 取消注册回调
            if (eye_callback_registered)
            {
                SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                eye_callback_registered = false;
            }

            // 关闭文件
            if (dataWriter != null)
            {
                dataWriter.Flush();
                dataWriter.Close();
                dataWriter = null;
                Debug.Log($"眼动数据记录已停止，文件保存至: {filePath}");
            }

            // 清理可视化对象
            if (hitMarkerInstance != null)
            {
                Destroy(hitMarkerInstance);
                hitMarkerInstance = null;
            }
        }

        private static void EyeCallback(ref EyeData_v2 eye_data)
        {
            eyeData = eye_data;
        }

        // 公共方法用于控制记录
        public void StartRecording()
        {
            enableRecording = true;
            if (!isRecording)
            {
                InitializeDataRecording();
            }
        }

        public void StopRecording()
        {
            enableRecording = false;
            ReleaseResources();
        }

        // 公共方法用于控制射线可视化
        public void ShowGazeRay()
        {
            showGazeRay = true;
            if (gazeRayRenderer != null)
            {
                gazeRayRenderer.enabled = true;
            }
        }

        public void HideGazeRay()
        {
            showGazeRay = false;
            if (gazeRayRenderer != null)
            {
                gazeRayRenderer.enabled = false;
            }
            if (hitMarkerInstance != null)
            {
                hitMarkerInstance.SetActive(false);
            }
        }
    }
}