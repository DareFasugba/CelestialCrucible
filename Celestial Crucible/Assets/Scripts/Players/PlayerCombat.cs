using UnityEngine;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    bool canAttack = true;
    public Button PunchButton;
    public Button KickButton;

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

        PunchButton.interactable = true;
        KickButton.interactable = true;
    }

    // Called from animation event
    public void DisableAttack()
    {
        canAttack = false;
        animator.SetBool("IsAttacking", true);

        PunchButton.interactable = false;
        KickButton.interactable = false;
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
