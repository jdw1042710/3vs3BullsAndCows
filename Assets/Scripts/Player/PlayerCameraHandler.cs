using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraHandler : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("카메라가 쳐다볼 오프셋 트랜스폼")]
    [SerializeField] private Transform cameraTarget;

    [Header("Look Settings")]
    [SerializeField] private Vector2 lookSensitivity = new Vector2(4, 2);
    [SerializeField] private float clamp = 30.0f;    // 상하 시야각 제한
    [SerializeField] private float distance = 2f;

    public Vector2 LookSensitivity { get => lookSensitivity; }

    private float targetPitch = 0;

    [Header("Debug")]
    [SerializeField] private bool initializedWithoutNetwork = false;
    #region Unity Life Cycle

    private void Awake()
    {
        if (cameraTarget == null)
            Debug.LogError("There is no target camera transform");
    }

    private void Start()
    {
        LinkCinemachine();
    }

    #endregion
    #region Initialize
    public void LinkCinemachine()
    {
        var cam = FindAnyObjectByType<CinemachineCamera>();
        if (cam != null)
        {
            Transform target = cameraTarget;
            cam.Follow = target;
            cam.LookAt = target; // 3인칭 백뷰 스타일
            
        }
    }
    #endregion
    #region Movement
    /// <summary>
    /// PlayerController에서 호출: 마우스 입력을 받아 카메라 타겟을 회전시킴
    /// </summary>
    public void ProcessLookInput(float inputY)
    {
        if (cameraTarget == null) return;

        targetPitch -= inputY * lookSensitivity.y;

        // 각도 제한 (목 꺾임 방지)
        targetPitch = ClampAngle(targetPitch, -clamp, clamp);

        // 타겟 오브젝트 회전 적용 (Local Rotation)
        cameraTarget.localRotation = Quaternion.Euler(targetPitch, 0.0f, 0.0f);
    }

    /// <summary>
    /// 각도 클램프 헬퍼 함수
    /// </summary>
    /// <returns></returns>
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        while (lfAngle < -360f) lfAngle += 360f;
        while (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    #endregion
}
