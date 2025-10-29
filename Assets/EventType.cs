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
    LevelComplete
}
