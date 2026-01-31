using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;

    // StringToHash를 사용하면 Update에서 문자열 비교를 하지 않아 성능이 향상됩니다.
    private readonly int hashHorizontal = Animator.StringToHash("Horizontal");
    private readonly int hashVertical = Animator.StringToHash("Vertical");
    private readonly int hashMove = Animator.StringToHash("Move");
    private readonly int hashJump = Animator.StringToHash("Jump");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashDeath = Animator.StringToHash("Death");
    private readonly int hashRespawn = Animator.StringToHash("Respawn");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    #region Movement
    public void UpdateMoveAnimation(float h, float v)
    {
        // 0.1f는 Damping(부드러운 전환) 시간
        animator.SetFloat(hashHorizontal, h, 0.1f, Time.deltaTime);
        animator.SetFloat(hashVertical, v, 0.1f, Time.deltaTime);
        animator.SetBool(hashMove, Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f);
    }

    public void TriggerJump()
    {
        animator.SetTrigger(hashJump);
    }
    #endregion
    #region Action
    public void TriggerAttack()
    {
        animator.SetTrigger(hashAttack);
    }

    public void TriggerDeath()
    {
        animator.SetTrigger(hashDeath);
    }

    public void TriggerRespawn()
    {
        animator.SetTrigger(hashRespawn);
    }
    #endregion
}