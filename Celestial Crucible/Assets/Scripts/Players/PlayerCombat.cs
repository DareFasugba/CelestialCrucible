using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    [Header("UI")]
    public Button PunchButton;
    public Button KickButton;
    public Button FireballButton;

    [Header("Fireball")]
    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireballSpeed = 15f;
    public float fireballDelay = 0.6f;     // ⏱ delay before spawn
    public float fireballCooldown = 1.5f;

    bool canAttack = true;
    bool canFireball = true;

    // --------------------------------------------------
    // BASIC ATTACKS
    // --------------------------------------------------
    public void PunchAttack()
    {
        if (!canAttack) return;

        animator.SetInteger("AttackType", 0);
        animator.SetTrigger("AttackTrigger");
        DisableAttack();
    }

    public void KickAttack()
    {
        if (!canAttack) return;

        animator.SetInteger("AttackType", 1);
        animator.SetTrigger("AttackTrigger");
        DisableAttack();
    }

    // --------------------------------------------------
    // FIREBALL SPECIAL
    // --------------------------------------------------
    public void FireballAttack()
    {
        if (!canAttack || !canFireball) return;

        animator.SetTrigger("Fireball");

        StartCoroutine(DelayedFireball());
        StartCoroutine(FireballCooldown());
    }

    IEnumerator DelayedFireball()
    {
        yield return new WaitForSeconds(fireballDelay);
        SpawnFireball();
    }

    void SpawnFireball()
    {
        GameObject fireball = Instantiate(
            fireballPrefab,
            firePoint.position,
            firePoint.rotation
        );

        Rigidbody rb = fireball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * fireballSpeed;
        }
    }

    IEnumerator FireballCooldown()
    {
        canFireball = false;

        if (FireballButton != null)
            FireballButton.interactable = false;

        yield return new WaitForSeconds(fireballCooldown);

        canFireball = true;

        if (FireballButton != null)
            FireballButton.interactable = true;
    }

    // --------------------------------------------------
    // ANIMATION EVENTS (MELEE ONLY)
    // --------------------------------------------------
    public void EnableAttack()
    {
        canAttack = true;
        animator.SetBool("IsAttacking", false);

        PunchButton.interactable = true;
        KickButton.interactable = true;
    }

    public void DisableAttack()
    {
        canAttack = false;
        animator.SetBool("IsAttacking", true);

        PunchButton.interactable = false;
        KickButton.interactable = false;
    }

    // --------------------------------------------------
    // KEYBOARD INPUT
    // --------------------------------------------------
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            PunchAttack();

        if (Input.GetKeyDown(KeyCode.G))
            KickAttack();

        if (Input.GetKeyDown(KeyCode.R))
            FireballAttack();
    }
}


