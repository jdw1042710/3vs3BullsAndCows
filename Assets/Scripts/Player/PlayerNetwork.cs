using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public enum InputButton
{
    Jump = 0,
    Attack = 1,
    Interact = 2
}

public struct NetworkInputData : INetworkInput
{
    public Vector3 moveDirection;
    public float lookRotationY;
    public NetworkButtons buttons;
}

[RequireComponent(typeof(NetworkRigidbody3D))]
[RequireComponent(typeof(NetworkObject))]
public class PlayerNetwork : NetworkBehaviour
{
    private Player player;
    private PlayerMovement movement;
    private PlayerController controller;
    private PlayerCameraHandler cameraHandler;

    [Networked] public int NetworkHealth { get; set; }
    [Networked] public NetworkButtons PreviousButtons { get; set; }

    private ChangeDetector changes;

    public override void Spawned()
    {
        player = GetComponent<Player>();
        movement = GetComponent<PlayerMovement>();
        controller = GetComponent<PlayerController>();
        cameraHandler = GetComponent<PlayerCameraHandler>();

        changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        // [Server]
        if (HasStateAuthority)
        {
            NetworkHealth = player.maxHealth;
        }

        // [Client] 내 캐릭터라면
        if (HasInputAuthority)
        {
            // Controller의 네트워크 제어 모드 활성화 (직접 이동 방지)
            if (controller != null)
            {
                controller.enabled = true;
                controller.IsNetworkControlled = true;
            }

            // 카메라 강제 연결 (재접속이나 스폰 시점 문제 해결)
            if (cameraHandler != null)
            {
                cameraHandler.LinkCinemachine();
            }

            NetworkManager.Instance?.SetLocalPlayer(this);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            movement.ProcessMovement(data.moveDirection, data.buttons.IsSet(InputButton.Jump));
            movement.ProcessRotation(data.lookRotationY);
            
            if (data.buttons.WasPressed(PreviousButtons, InputButton.Attack))
            {
                RPC_Attack();
            }

            PreviousButtons = data.buttons;
        }
    }

    public override void Render()
    {
        foreach (var change in changes.DetectChanges(this))
        {
            if (change == nameof(NetworkHealth))
            {
                player.OnHealthUpdated(NetworkHealth);
            }
        }
    }

    [Rpc(RpcSources.InputAuthority | RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Attack()
    {
        player.PlayAttackAnimation();
    }

    public void TakeDamage(int damage)
    {
        if (HasStateAuthority)
        {
            NetworkHealth -= damage;
        }
    }

    // [참고] 외부(InputManager/Spawner)에서 OnInput 호출 시 사용할 헬퍼 메소드
    public NetworkInputData GetLocalInput()
    {
        if (controller == null) return default;

        NetworkInputData data = new NetworkInputData();
        data.moveDirection = controller.InputMoveDir;
        data.lookRotationY = controller.InputLookX; // 마우스 X는 캐릭터 회전(Yaw)

        data.buttons.Set(InputButton.Jump, controller.InputJump);
        data.buttons.Set(InputButton.Attack, controller.InputAttack);

        return data;
    }
}