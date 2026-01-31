using UnityEngine;
using Fusion;

// 로컬 플레이어에게만 존재하는 컴포넌트라고 가정
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float lookSensitivity = 2f;

    // 현재 프레임의 입력 데이터 (Network가 가져감)
    public NetworkInputData CurrentInputData;

    private void Update()
    {
        // 1. 마우스 회전 (이건 로컬에서 즉시 반영해도 되고, 네트워크로 보내도 됨)
        // 여기서는 네트워크로 보내서 동기화하는 방식을 택함
        float mouseX = Input.GetAxisRaw("Mouse X") * lookSensitivity;

        // 2. 키보드 이동
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 3. 버튼 상태
        NetworkButtons buttons = default;
        buttons.Set(InputButton.Jump, Input.GetButton("Jump"));
        buttons.Set(InputButton.Attack, Input.GetButton("Fire1"));

        // 데이터 포장
        CurrentInputData = new NetworkInputData
        {
            moveDirection = new Vector3(h, 0, v),
            lookRotationY = mouseX,
            buttons = buttons
        };
    }
}