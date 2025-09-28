using System;
using System.Collections.Generic;
using UnityEngine;

// 触发器类型
public enum TriggerType
{
    MainCarReachedPoint,
    AllCarsStartMoving,
    CarGroupMovement
}

// 触发器事件参数
public class TriggerEventArgs : EventArgs
{
    public TriggerType TriggerType { get; private set; }
    public Transform TriggerPoint { get; private set; }
    public GameObject TriggeringCar { get; private set; }

    public TriggerEventArgs(TriggerType type, Transform point, GameObject car)
    {
        TriggerType = type;
        TriggerPoint = point;
        TriggeringCar = car;
    }
}

// 事件管理器
public static class TriggerEventManager
{
    public static event Action<TriggerEventArgs> OnTriggerActivated;

    public static void ActivateTrigger(TriggerType type, Transform point, GameObject car)
    {
        OnTriggerActivated?.Invoke(new TriggerEventArgs(type, point, car));
    }
}
