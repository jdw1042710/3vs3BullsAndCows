using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    public float walkSpeed = 5f;
    public float jumpPower = 5f;

    private Rigidbody rigid;
    private Animator animator;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// �ܺ�(Network)���� ȣ���Ͽ� ���� �̵� ó��
    /// </summary>
    public void ProcessMovement(Vector3 direction, bool isJumping)
    {
        // 1. �̵� ó�� (Velocity ���� ����)
        Vector3 moveDir = direction.normalized;
        // ���� ���� -> ���� ���� ��ȯ
        Vector3 worldDir = transform.TransformDirection(moveDir);

        Vector3 targetVelocity = worldDir * walkSpeed;
        rigid.linearVelocity = new Vector3(targetVelocity.x, rigid.linearVelocity.y, targetVelocity.z);

        // 2. ���� ó��
        if (isJumping && IsGrounded())
        {
            rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, jumpPower, rigid.linearVelocity.z);
            animator.SetTrigger("Jump");
        }

        // 3. �ִϸ��̼� ����ȭ
        UpdateAnimations(direction.x, direction.z);
    }

    /// <summary>
    /// �ܺ�(Network)���� ȣ���Ͽ� ȸ�� ó��
    /// </summary>
    public void ProcessRotation(float yRotation)
    {
        if (Mathf.Abs(yRotation) > 0.01f)
        {
            Quaternion deltaRot = Quaternion.Euler(0, yRotation, 0);
            rigid.MoveRotation(rigid.rotation * deltaRot);
        }
    }

    private void UpdateAnimations(float h, float v)
    {
        animator.SetFloat("Horizontal", h, 0.1f, Time.deltaTime);
        animator.SetFloat("Vertical", v, 0.1f, Time.deltaTime);
        animator.SetBool("Move", h != 0 || v != 0);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.3f);
    }
}