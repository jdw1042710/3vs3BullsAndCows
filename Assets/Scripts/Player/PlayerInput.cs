using Fusion;
using UnityEngine;

public enum InputButton
{
    Jump = 0,
    Attack = 1,
    Interact = 2
}

// 입력 데이터 패킷
public struct NetworkInputData : INetworkInput
{
    public Vector3 moveDirection; // x: Horizontal, z: Vertical
    public float lookRotationY;   // Mouse X
    public NetworkButtons buttons;
}