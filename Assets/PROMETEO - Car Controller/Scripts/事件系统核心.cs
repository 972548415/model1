using System;
using System.Collections.Generic;
using UnityEngine;

// ����������
public enum TriggerType
{
    MainCarReachedPoint,
    AllCarsStartMoving,
    CarGroupMovement
}

// �������¼�����
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

// �¼�������
public static class TriggerEventManager
{
    public static event Action<TriggerEventArgs> OnTriggerActivated;

    public static void ActivateTrigger(TriggerType type, Transform point, GameObject car)
    {
        OnTriggerActivated?.Invoke(new TriggerEventArgs(type, point, car));
    }
}
