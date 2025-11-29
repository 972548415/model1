// EventType.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventType
{
    // 车辆相关事件
    MainVehicleReachPosition,
    VehicleStartMoving,
    VehicleDisappear,

    // 烟雾相关事件
    SmokeStartGenerate,
    SmokeDisappear,

    // 声音相关事件
    PlayTriggerSound,

    // 游戏状态事件
    GameStart,
    GameOver,
    LevelComplete,

    // 新添加的事件 - 物体移动控制
    ObjectStartMovingZ,      // 物体开始沿Z轴移动
    ObjectStopMovingZ,       // 物体停止沿Z轴移动
    ObjectChangeMovingSpeed  // 改变物体移动速度
}