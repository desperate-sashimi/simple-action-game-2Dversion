using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anim_Controller : MonoBehaviour
{
    [Space]
    new private Rigidbody2D rigidbody;
    private Animator animator;
    private Collision coll;
    private PlayerController pc;

    [Space]
    private bool isGround;
    private float Horizontal;
    private float Vertical;
    public int ComboStep;

    void Start(){
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        coll = GetComponent<Collision>();
        pc = GetComponent<PlayerController>();
    }

    void Update(){
        // update the animation
        animator.SetFloat("Horizontal", rigidbody.velocity.x);
        animator.SetFloat("Vertical", rigidbody.velocity.y);
        animator.SetBool("isGround", coll.isGround);
    }

    public void jump(){
        animator.SetTrigger("Jump");
    }

    public void dash(){
        animator.SetTrigger("Dash");
    }

    public void LightAttack(){
        ComboStep++;
        if (ComboStep > 3)
            ComboStep = 1;
        animator.SetInteger("ComboStep", ComboStep);
        animator.SetTrigger("LightAttack");
    }

    public void HeavyAttack(){
        ComboStep++;
        if (ComboStep > 3)
            ComboStep = 1;
        if(pc.attackType == "GetDown") animator.speed = 2f;
        animator.SetInteger("ComboStep", ComboStep);
        animator.SetTrigger("HeavyAttack");
        
    }

    public void BlowUp(){
        animator.SetTrigger("BlowUp");
    }


    private void AnimSpeedReset(){
        animator.speed = 1;
    }

}
