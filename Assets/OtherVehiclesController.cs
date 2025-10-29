// OtherVehiclesController.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OtherVehiclesController : MonoBehaviour
{
    [Header("������������")]
    public List<GameObject> otherVehicles = new List<GameObject>();
    public Transform destination;
    public float moveSpeed = 3.0f;
    public float rotationSpeed = 2.0f;

    [Header("��ʧ����")]
    public float fadeDuration = 3.0f;
    public bool destroyAfterFade = false;

    [Header("�ƶ�����")]
    public float startMoveDelay = 0.5f;
    public float perVehicleDelay = 0.2f;

    private List<Vector3> originalPositions = new List<Vector3>();
    private List<Renderer> vehicleRenderers = new List<Renderer>();
    private List<Material[]> originalMaterials = new List<Material[]>();
    private bool isMoving = false;

    void Start()
    {
        InitializeVehicles();

        // ע���¼�����
        EgoEventCenter.AddListener(EventType.MainVehicleReachPosition, OnMainVehicleReach);
        EgoEventCenter.AddListener(EventType.VehicleStartMoving, OnVehicleStartMoving);

        Debug.Log($"���������������������������� {otherVehicles.Count} ����");
    }

    void OnDestroy()
    {
        EgoEventCenter.RemoveListener(EventType.MainVehicleReachPosition, OnMainVehicleReach);
        EgoEventCenter.RemoveListener(EventType.VehicleStartMoving, OnVehicleStartMoving);
    }

    void InitializeVehicles()
    {
        originalPositions.Clear();
        vehicleRenderers.Clear();
        originalMaterials.Clear();

        foreach (var vehicle in otherVehicles)
        {
            if (vehicle != null)
            {
                // ����ԭʼλ��
                originalPositions.Add(vehicle.transform.position);

                // ��ȡ��Ⱦ���Ͳ���
                Renderer renderer = vehicle.GetComponent<Renderer>();
                if (renderer != null)
                {
                    vehicleRenderers.Add(renderer);
                    originalMaterials.Add(renderer.materials);
                }
                else
                {
                    vehicleRenderers.Add(null);
                    originalMaterials.Add(null);
                }

                Debug.Log($"��ʼ������: {vehicle.name} λ��: {vehicle.transform.position}");
            }
        }
    }

    void OnMainVehicleReach()
    {
        Debug.Log("�յ������������¼�");
        StartVehicleActions();
    }

    void OnVehicleStartMoving()
    {
        Debug.Log("�յ�������ʼ�ƶ��¼�");
        StartVehicleActions();
    }

    void StartVehicleActions()
    {
        if (isMoving) return;

        Debug.Log("��ʼִ��������������");
        isMoving = true;

        // ��ʼ�ƶ�����
        StartCoroutine(MoveAllVehicles());

        // ��ʼ����Ч��
        StartCoroutine(FadeOutAllVehicles());
    }

    IEnumerator MoveAllVehicles()
    {
        if (destination == null)
        {
            Debug.LogError("Ŀ�ĵ�δ���ã����������޷��ƶ�");
            yield break;
        }

        Debug.Log($"��ʼ�ƶ� {otherVehicles.Count} ������Ŀ�ĵ�: {destination.position}");

        // ��ʼ�ӳ�
        yield return new WaitForSeconds(startMoveDelay);

        // Ϊÿ���������ƶ�Э��
        for (int i = 0; i < otherVehicles.Count; i++)
        {
            if (otherVehicles[i] != null)
            {
                StartCoroutine(MoveSingleVehicle(i));
                yield return new WaitForSeconds(perVehicleDelay);
            }
        }
    }

    IEnumerator MoveSingleVehicle(int vehicleIndex)
    {
        GameObject vehicle = otherVehicles[vehicleIndex];
        if (vehicle == null) yield break;

        string vehicleName = vehicle.name;
        Debug.Log($"��ʼ�ƶ�����: {vehicleName}");

        Vector3 startPosition = vehicle.transform.position;
        float distance = Vector3.Distance(startPosition, destination.position);
        float moveTime = distance / moveSpeed;

        float elapsedTime = 0f;

        while (elapsedTime < moveTime && vehicle != null)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / moveTime;

            // �ƶ�λ��
            vehicle.transform.position = Vector3.Lerp(startPosition, destination.position, progress);

            // ��ת����Ŀ�ĵ�
            if (rotationSpeed > 0)
            {
                Vector3 direction = (destination.position - vehicle.transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    vehicle.transform.rotation = Quaternion.Slerp(
                        vehicle.transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
                }
            }

            yield return null;
        }

        // ȷ������λ����ȷ
        if (vehicle != null)
        {
            vehicle.transform.position = destination.position;
            Debug.Log($"���� {vehicleName} �ѵ���Ŀ�ĵ�");
        }
    }

    IEnumerator FadeOutAllVehicles()
    {
        Debug.Log($"��ʼ���� {vehicleRenderers.Count} ����������ʱ��: {fadeDuration}��");

        // �ȴ�һ��ʱ���ʼ����
        yield return new WaitForSeconds(startMoveDelay);

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            for (int i = 0; i < vehicleRenderers.Count; i++)
            {
                if (vehicleRenderers[i] != null && otherVehicles[i] != null)
                {
                    SetVehicleAlpha(vehicleRenderers[i], alpha);
                }
            }

            yield return null;
        }

        // ���մ���
        for (int i = 0; i < otherVehicles.Count; i++)
        {
            if (otherVehicles[i] != null)
            {
                if (destroyAfterFade)
                {
                    Destroy(otherVehicles[i]);
                    Debug.Log($"���ٳ���: {otherVehicles[i].name}");
                }
                else
                {
                    otherVehicles[i].SetActive(false);
                    Debug.Log($"���س���: {otherVehicles[i].name}");
                }
            }
        }

        Debug.Log("���г����������");
    }

    void SetVehicleAlpha(Renderer renderer, float alpha)
    {
        Material[] materials = renderer.materials;
        for (int i = 0; i < materials.Length; i++)
        {
            Color color = materials[i].color;
            color.a = alpha;
            materials[i].color = color;
        }
    }

    // �������г���״̬�����ڲ��ԣ�
    public void ResetVehicles()
    {
        StopAllCoroutines();
        isMoving = false;

        for (int i = 0; i < otherVehicles.Count; i++)
        {
            if (otherVehicles[i] != null)
            {
                // ����λ��
                if (i < originalPositions.Count)
                {
                    otherVehicles[i].transform.position = originalPositions[i];
                }

                // ���ò���͸����
                if (i < vehicleRenderers.Count && vehicleRenderers[i] != null)
                {
                    SetVehicleAlpha(vehicleRenderers[i], 1f);
                }

                // ȷ�������ɼ�
                otherVehicles[i].SetActive(true);
            }
        }

        Debug.Log("���г���״̬������");
    }

    // ��Scene��ͼ����ʾ������Ϣ
    void OnDrawGizmosSelected()
    {
        if (destination != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(destination.position, 1f);

            // ���ƴ�ÿ������Ŀ�ĵص���
            Gizmos.color = Color.blue;
            foreach (var vehicle in otherVehicles)
            {
                if (vehicle != null && destination != null)
                {
                    Gizmos.DrawLine(vehicle.transform.position, destination.position);
                }
            }
        }
    }

    [ContextMenu("�����ƶ�����")]
    public void TestMoveVehicles()
    {
        Debug.Log("�ֶ������ƶ�����");
        StartVehicleActions();
    }
}
