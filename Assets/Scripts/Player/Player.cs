using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Status")]
    public int maxHealth = 10;
    public int currentHealth; // PlayerNetwork가 이 값을 덮어씀
    public eTeamType team;

    [Header("References")]
    [SerializeField] private Weapon weapon;

    public event Action OnDeath; // 사망 시 UI 처리 등을 위해 이벤트 연결 가능

    private Animator _animator;

    private void Awake()
    {
        currentHealth = maxHealth;
        _animator = GetComponent<Animator>();
        if (weapon) weapon.team = team;
    }

    // 시각적(애니메이션) 처리만 담당
    public void PlayAttackAnimation()
    {
        _animator.SetTrigger("Attack");
        if (weapon) weapon.Attack();
    }

    public void OnHealthUpdated(int newHealth)
    {
        // 체력이 변했을 때 UI 업데이트 로직 등을 여기에 작성
        currentHealth = newHealth;

        if (currentHealth <= 0)
        {
            _animator.SetTrigger("Death");
            OnDeath?.Invoke();
        }
    }
}