using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    bool canAttack = true;

    public void PunchAttack() {
        
        if (!canAttack) return;
        
        animator.SetInteger("AttackType", 0);
        animator.SetTrigger("AttackTrigger");
        DisableAttack();
    }

    public void KickAttack() {
        
        if (!canAttack) return;
        
        animator.SetInteger("AttackType", 1);
        animator.SetTrigger("AttackTrigger");
        DisableAttack();
    }
    
    // Called from animation event
    public void EnableAttack()
    {
        canAttack = true;
        animator.SetBool("IsAttacking", false);
    }

    // Called from animation event
    public void DisableAttack()
    {
        canAttack = false;
        animator.SetBool("IsAttacking", true);
    }

    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.F))
    {
        Debug.Log("Light attack input detected");
        PunchAttack();
    }

    if (Input.GetKeyDown(KeyCode.G))
    {
        Debug.Log("Heavy attack input detected");
        KickAttack();
    }
    }
}
