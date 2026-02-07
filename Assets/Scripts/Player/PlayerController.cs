using UnityEngine;

// 로컬 플레이어에게만 존재하는 컴포넌트라고 가정
public class PlayerController : MonoBehaviour
{
    // PlayerNetwork 존재 여부
    public bool IsNetworkControlled { get; set; } = false;

    // 사용자 입력값
    public Vector3 InputMoveDir { get; private set; }
    public float InputLookX { get; private set; }
    public float InputLookY { get; private set; }
    public bool InputJump { get; private set; }
    public bool InputAttack { get; private set; }

    // 내부 변수들
    private PlayerCameraHandler cameraHandler;
    private PlayerMovement movement;
    private Player player;

    private void Awake()
    {
        cameraHandler = GetComponent<PlayerCameraHandler>();
        movement = GetComponent<PlayerMovement>();
        player = GetComponent<Player>();
    }

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        InputMoveDir = new Vector3(h, 0, v);
        InputLookX = Input.GetAxisRaw("Mouse X") * cameraHandler.LookSensitivity.x;
        InputLookY = Input.GetAxisRaw("Mouse Y") * cameraHandler.LookSensitivity.y;
        InputJump = Input.GetKeyDown(KeyCode.Space);
        InputAttack = Input.GetMouseButtonDown(0);
        // 로컬에서만 처리함 (네트워크 처리 필요 X)
        if (cameraHandler != null)
        {
            cameraHandler.ProcessLookInput(InputLookY);
        }

        if (!IsNetworkControlled)
            HandleMovement();
    }

    private void HandleMovement()
    {
        if (movement != null)
        {
            movement.ProcessMovement(InputMoveDir, InputJump);
            movement.ProcessRotation(InputLookX);
        }

        if (InputAttack && player != null)
        {
            player.PlayAttackAnimation();
        }
    }
}