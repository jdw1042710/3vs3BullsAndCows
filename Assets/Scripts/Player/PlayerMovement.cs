using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    public float walkSpeed = 5f;
    public float jumpPower = 5f;

    private Rigidbody rigid;
    private PlayerAnimator animator;

    #region Unity Life Cycle
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        animator = GetComponent<PlayerAnimator>();
    }
    #endregion

    #region Public Methods
    public void ProcessMovement(Vector3 direction, bool isJumping)
    {
        Vector3 moveDir = direction.normalized;
        Vector3 worldDir = transform.TransformDirection(moveDir);

        Vector3 targetVelocity = worldDir * walkSpeed;
        rigid.linearVelocity = new Vector3(targetVelocity.x, rigid.linearVelocity.y, targetVelocity.z);

        if (isJumping && IsGrounded())
        {
            rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, jumpPower, rigid.linearVelocity.z);
            if (animator) animator.TriggerJump();
        }

        if (animator) animator.UpdateMoveAnimation(direction.x, direction.z);
    }

    public void ProcessRotation(float yRotation)
    {
        if (Mathf.Abs(yRotation) > 0.01f)
        {
            Quaternion deltaRot = Quaternion.Euler(0, yRotation, 0);
            rigid.MoveRotation(rigid.rotation * deltaRot);
        }
    }
    #endregion

    #region Private Methods
    private bool IsGrounded()
    {
        Debug.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * 0.3f, Color.red);
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.3f);
    }
    #endregion
}