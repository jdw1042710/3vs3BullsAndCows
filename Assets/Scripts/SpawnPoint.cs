using UnityEngine;

public class SpawnPoint : MonoBehaviour
{

    [Header("Settings")]
    public eTeamType team = eTeamType.None;

    [Tooltip("스폰 위치 Y좌표 Offset")]
    [SerializeField] private float yOffset = 0.05f;

    [Tooltip("바닥으로 인식할 레이어")]
    [SerializeField] private LayerMask groundLayer = 1; // Default or Terrain

    [Tooltip("바닥을 탐색할 최대 거리")]
    [SerializeField] private float maxCheckDistance = 50f;

    [Header("Gizmo Settings")]
    [SerializeField] private Color gizmoColor = Color.green;
    [SerializeField] private float sphereRadius = 0.3f;

    /// <summary>
    /// 외부(NetworkController)에서 호출하는 함수입니다.
    /// 현재 트랜스폼 위치에서 바닥을 찾아 최종 스폰 좌표를 반환합니다.
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        // 1. 현재 위치에서 아래로 레이를 쏩니다.
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxCheckDistance, groundLayer))
        {
            // 2. 바닥을 찾았다면: 바닥 위치 + 오프셋 반환
            return hit.point + Vector3.up * yOffset;
        }

        // 3. 바닥이 없다면: 그냥 현재 오브젝트 위치 반환
        return transform.position;
    }

    // 에디터 상에서 시각적으로 보여주는 부분
    private void OnDrawGizmos()
    {
        Vector3 spawnPos = GetSpawnPosition();

        // 실제 스폰될 위치에 구체를 그립니다.
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(spawnPos, sphereRadius);

        // 캐릭터가 바라볼 방향을 화살표처럼 표시합니다.
        Gizmos.color = Color.red;
        Vector3 direction = transform.forward * 1.5f;
        Gizmos.DrawRay(spawnPos, direction);
    }
}