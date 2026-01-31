using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

[RequireComponent(typeof(NetworkRigidbody3D))]
[RequireComponent(typeof(NetworkObject))]
public class PlayerNetwork : NetworkBehaviour
{
    // 로컬 컴포넌트 참조
    private Player player;
    private PlayerMovement movement;
    private PlayerController controller;

    // [Networked] 변수: 값이 변하면 모든 클라이언트에 자동 동기화됨
    [Networked] public int NetworkHealth { get; set; }
    [Networked] public NetworkButtons PreviousButtons { get; set; }

    // 체력 변경 감지용 (UI 업데이트 등)
    private ChangeDetector changes;

    public override void Spawned()
    {
        player = GetComponent<Player>();
        movement = GetComponent<PlayerMovement>();
        controller = GetComponent<PlayerController>(); // 로컬 플레이어만 있음
        changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (HasStateAuthority)
        {
            NetworkHealth = player.maxHealth;
        }

        // 내 캐릭터라면 카메라 활성화 등 Controller 초기화
        if (HasInputAuthority && controller != null)
        {
            controller.enabled = true;
        }
    }

    // Fusion의 물리/로직 루프
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            // 1. 이동 처리
            movement.ProcessMovement(data.moveDirection, data.buttons.IsSet(InputButton.Jump));
            movement.ProcessRotation(data.lookRotationY);

            // 2. 공격 처리 (버튼이 눌린 순간 감지)
            // [수정] Runner.InputPrevious 대신 내 변수(PreviousButtons) 사용
            if (data.buttons.WasPressed(PreviousButtons, InputButton.Attack))
            {
                RPC_Attack();
            }

            // [중요] 처리가 끝난 후 현재 버튼 상태를 '이전 상태'로 저장
            PreviousButtons = data.buttons;
        }
    }

    public override void Render()
    {
        // NetworkHealth 변경 감지 -> Player(Model) 업데이트
        foreach (var change in changes.DetectChanges(this))
        {
            if (change == nameof(NetworkHealth))
            {
                player.OnHealthUpdated(NetworkHealth);
            }
        }
    }

    // 공격 시각 효과 동기화
    [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Attack()
    {
        player.PlayAttackAnimation();
    }

    // 피격 처리 (충돌 감지는 서버에서 수행한다고 가정)
    public void TakeDamage(int damage)
    {
        if (HasStateAuthority)
        {
            NetworkHealth -= damage;
        }
    }
}