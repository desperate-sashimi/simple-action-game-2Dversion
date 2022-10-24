using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{
    public bool isGround;
    private PlayerController pc;
    private Rigidbody2D rb;

    [SerializeField] private LayerMask layer;
    [SerializeField] private Vector3 check;



    void Start(){
        pc = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        update_collision_state();
        
        isGround = Physics2D.OverlapCircle(transform.position + new Vector3(check.x, check.y, 0), check.z, layer);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position + new Vector3(check.x, check.y, 0), check.z);
    }

    private void update_collision_state(){
        if(isGround && !pc.isDashing){
            pc.hasSecondJumped = true;
            pc.attackAboveTime = 0f;
            rb.gravityScale = 1f;
            pc.hasDashAttack = false;
            pc.hasDashed = false;
            
        }
    }




}