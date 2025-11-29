using System;
using System.IO;
using System.Text;
using UnityEngine;
using ViveSR.anipal.Eye;
using System.Runtime.InteropServices;
using UnityEngine.Assertions;
using System.Globalization;

namespace ViveSR.anipal.Eye
{
    public class SRanipal_GazeRaySample_v2 : MonoBehaviour
    {
        public int LengthOfRay = 25;
        [SerializeField] private LineRenderer GazeRayRenderer;
        private static EyeData_v2 eyeData = new EyeData_v2();
        private bool eye_callback_registered = false;

        // 眼动数据参数
        private float pupilDiameterLeft, pupilDiameterRight;
        private Vector2 pupilPositionLeft, pupilPositionRight;
        private float eyeOpenLeft, eyeOpenRight;

        // 文件记录参数
        private string datasetFilePath;
        private StreamWriter datasetFileWriter;
        private const string TimeFormat = "yyyy-MM-dd_HH:mm:ss.ffff";

        // 用于存储最新眼动数据的缓存
        private Vector3 latestGazeDirection;
        private bool hasNewGazeData = false;

        public event Action<Vector3> CollisionPointEvent;

        void Start()
        {
            if (!SRanipal_Eye_Framework.Instance.EnableEye)
            {
                enabled = false;
                return;
            }
            Assert.IsNotNull(GazeRayRenderer);

            // 文件路径
            string directory = Path.Combine(Application.dataPath, "../PlayerTraces");
            Directory.CreateDirectory(directory);

            string fileName = $"Eye_Tracking_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.csv";
            datasetFilePath = Path.Combine(directory, fileName);

            datasetFileWriter = new StreamWriter(datasetFilePath, false, new UTF8Encoding(true))
            {
                AutoFlush = true
            };

            // 扩展CSV表头：使用欧拉角XYZ
            datasetFileWriter.WriteLine(
                "TimeStamp," + "玩家位置X,玩家位置Y,玩家位置Z," +
                "玩家旋转X,玩家旋转Y,玩家旋转Z," +
                "碰撞点X,碰撞点Y,碰撞点Z," + "碰撞物体名称," +
                "左瞳孔直径(mm),右瞳孔直径(mm)," +
                "左瞳孔X,左瞳孔Y," +
                "右瞳孔X,右瞳孔Y," +
                "左眼睁眼度,右眼睁眼度"

            );

            Debug.Log($"眼动数据文件已创建: {datasetFilePath}");
        }

        void Update()
        {
            if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
                return;

            UpdateEyeCallback();
            UpdateGazeData();
        }

        void FixedUpdate()
        {
            if (!hasNewGazeData) return;

            // 获取玩家位置和旋转（使用主摄像机）
            Vector3 playerPosition = Camera.main.transform.position;
            Vector3 playerEulerAngles = Camera.main.transform.eulerAngles; // 使用欧拉角

            // 处理并记录眼动数据
            ProcessGazeData(playerPosition, playerEulerAngles);
            hasNewGazeData = false;
        }

        private void UpdateEyeCallback()
        {
            bool shouldRegister = SRanipal_Eye_Framework.Instance.EnableEyeDataCallback;
            if (shouldRegister != eye_callback_registered)
            {
                if (shouldRegister)
                {
                    SRanipal_Eye_v2.WrapperRegisterEyeDataCallback(
                        Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                }
                else
                {
                    SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(
                        Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                }
                eye_callback_registered = shouldRegister;
            }
        }

        private void UpdateGazeData()
        {
            if (!GetGazeRay(out Vector3 origin, out Vector3 direction)) return;

            // 更新射线可视化
            UpdateRayVisualization(direction);

            // 获取眼动指标
            GetEyeMetrics();

            // 缓存最新数据用于FixedUpdate处理
            latestGazeDirection = direction;
            hasNewGazeData = true;
        }

        private void ProcessGazeData(Vector3 playerPosition, Vector3 playerEulerAngles)
        {
            // 使用缓存的最新数据
            CheckCollision(latestGazeDirection, playerPosition, playerEulerAngles);
        }

        private bool GetGazeRay(out Vector3 origin, out Vector3 direction)
        {
            if (eye_callback_registered)
            {
                return SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out origin, out direction, eyeData) ||
                       SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out origin, out direction, eyeData) ||
                       SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out origin, out direction, eyeData);
            }
            return SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out origin, out direction) ||
                   SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out origin, out direction) ||
                   SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out origin, out direction);
        }

        private void UpdateRayVisualization(Vector3 direction)
        {
            Vector3 worldDir = Camera.main.transform.TransformDirection(direction);
            GazeRayRenderer.SetPosition(0, Camera.main.transform.position);
            GazeRayRenderer.SetPosition(1, Camera.main.transform.position + worldDir * LengthOfRay);
        }

        private void GetEyeMetrics()
        {
            var left = eyeData.verbose_data.left;
            var right = eyeData.verbose_data.right;

            pupilDiameterLeft = left.pupil_diameter_mm;
            pupilDiameterRight = right.pupil_diameter_mm;
            pupilPositionLeft = left.pupil_position_in_sensor_area;
            pupilPositionRight = right.pupil_position_in_sensor_area;
            eyeOpenLeft = left.eye_openness;
            eyeOpenRight = right.eye_openness;
        }

        private void CheckCollision(Vector3 direction, Vector3 playerPosition, Vector3 playerEulerAngles)
        {
            RaycastHit hit;
            Vector3 worldDir = Camera.main.transform.TransformDirection(direction).normalized;
            bool isHit = Physics.SphereCast(
                Camera.main.transform.position,
                0.1f,
                worldDir,
                out hit,
                LengthOfRay
            );

            string timestamp = DateTime.Now.ToString(TimeFormat);

            if (isHit)
            {
                HandleCollision(hit, timestamp, playerPosition, playerEulerAngles);
            }
            else
            {
                HandleNoCollision(timestamp, playerPosition, playerEulerAngles);
            }
        }

        private void HandleCollision(RaycastHit hit, string timestamp, Vector3 playerPos, Vector3 playerEulerAngles)
        {
            Vector3 point = hit.point;
            CollisionPointEvent?.Invoke(point);

            WriteDataLine(
                timestamp,
                hit.collider.name,
                point.x, point.y, point.z,
                playerPos, playerEulerAngles
            );
        }

        private void HandleNoCollision(string timestamp, Vector3 playerPos, Vector3 playerEulerAngles)
        {
            WriteDataLine(
                timestamp,
                "无碰撞",
                float.NaN, float.NaN, float.NaN,
                playerPos, playerEulerAngles
            );
        }

        private void WriteDataLine(string time, string name, float x, float y, float z, Vector3 playerPos, Vector3 playerEulerAngles)
        {
            datasetFileWriter.WriteLine(string.Join(",",
                FormatField(time),
                FormatNumber(playerPos.x),
                FormatNumber(playerPos.y),
                FormatNumber(playerPos.z),
                FormatNumber(playerEulerAngles.x),
                FormatNumber(playerEulerAngles.y),
                FormatNumber(playerEulerAngles.z),
                FormatNumber(x),
                FormatNumber(y),
                FormatNumber(z),
                FormatField(name),
                FormatNumber(pupilDiameterLeft),
                FormatNumber(pupilDiameterRight),
                FormatNumber(pupilPositionLeft.x),
                FormatNumber(pupilPositionLeft.y),
                FormatNumber(pupilPositionRight.x),
                FormatNumber(pupilPositionRight.y),
                FormatNumber(eyeOpenLeft),
                FormatNumber(eyeOpenRight)
            ));
        }

        private string FormatField(string value)
        {
            if (string.IsNullOrEmpty(value)) return "\"\"";
            return value.Contains(",") ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
        }

        private string FormatNumber(float value)
        {
            return value.ToString("F4", CultureInfo.InvariantCulture);
        }

        private void ReleaseEyeCallback()
        {
            if (eye_callback_registered)
            {
                SRanipal_Eye_v2.WrapperUnRegisterEyeDataCallback(
                    Marshal.GetFunctionPointerForDelegate((SRanipal_Eye_v2.CallbackBasic)EyeCallback));
                eye_callback_registered = false;
            }
        }

        private static void EyeCallback(ref EyeData_v2 eye_data)
        {
            eyeData = eye_data;
        }

        void OnDestroy()
        {
            datasetFileWriter?.Close();
            ReleaseEyeCallback();
        }
    }
}