using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Enemy : MonoBehaviour
{
    public float speed;
    private Vector2 direction;
    private bool isHit;
    private bool isDown;
    private AnimatorStateInfo info;

    private Animator animator;
    private Animator hitAnimator;
    new private Rigidbody2D rigidbody;
    private Collider2D coll;

    private bool isBlowup = false;
    private bool isGround;
    private bool isGetDown;
    private bool isRotable = true; 
    private float fallAttackSenseTimer = 1f;

    public float groundTimer = 0;
    private float rotateTimer = 0;
    [SerializeField]private Vector3 check_right;
    [SerializeField]private Vector3 check_left;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask layer;

    [Space]
    [Header("Debug UI")]
    [SerializeField]private TextMeshProUGUI debug_ui;  

    public float blowUpForce;

    void Start()
    {
        animator = transform.GetComponent<Animator>();
        hitAnimator = transform.GetChild(0).GetComponent<Animator>();
        rigidbody = transform.GetComponent<Rigidbody2D>();
        coll = transform.GetComponent<BoxCollider2D>();

    }

    void Update()
    {
        StateUpdate();
        update_ui();
        if(isGround) fallBlowUp();
        
    }

    public void GetHit(Vector2 direction, bool isAddForce = false)
    {
        transform.localScale = new Vector3(-direction.x, 1, 1);
        isHit = true;
        this.direction = direction; 
        animator.SetTrigger("Hit");
        hitAnimator.SetTrigger("Hit");
        
        if(!isBlowup){
            rigidbody.velocity = new Vector2(direction.x * speed, 0);
        }
        else{
            // if is blowed up, then five it .8f s to stay in the air
            StopAllCoroutines();
            
            StartCoroutine(timerBlowUpGravity(0.8f, isStart:false));
            
        }
    }

    public void GetBlowUp(Vector2 direction){

        // refresh all the timer
        StopAllCoroutines();
        rigidbody.isKinematic = false;
        rigidbody.gravityScale = 1; // incase
        rigidbody.drag = 1;

        // rotation
        if(Mathf.Abs(transform.rotation.z) < 5f && !isDown && isRotable){
            transform.DORotate(new Vector3(0,0,transform.localScale.x * 60), 0.5f);
        }

        // blowup
        rigidbody.velocity = new Vector2(rigidbody.velocity.x, blowUpForce);
        transform.localScale = new Vector3(-direction.x, 1, 1); // rotate to player
        // gethit animation
        isHit = true;
        this.direction = direction;
        animator.SetTrigger("Hit");
        hitAnimator.SetTrigger("Hit");
        // set property
        isBlowup = true;
        // set gravity timer
        StartCoroutine(timerBlowUpGravity(0.8f, true));
    }

    IEnumerator timerBlowUpGravity(float waitTime = 0.8f, bool isStart = false){
        if(!isStart){
            rigidbody.drag = 1;
            rigidbody.velocity = new Vector2(0f,0f);
            rigidbody.gravityScale = 0.1f;
            //rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            yield return new WaitForSeconds(waitTime);
            rigidbody.gravityScale = 10f;
            //rigidbody.constraints = RigidbodyConstraints2D.None;
        }
        else if(isStart){
            rigidbody.drag = 1;
            while(rigidbody.velocity.y > 0) yield return null;
            
            // 腾空阶段
            rigidbody.velocity = new Vector2(0f,0f);
            DOVirtual.Float(0f,0.5f,waitTime,setGravityScale).SetEase(Ease.InOutSine);
            //rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            yield return new WaitForSeconds(waitTime);

            // 降落阶段
            while(rigidbody.gravityScale < 10f){
                rigidbody.gravityScale = Mathf.Lerp(rigidbody.gravityScale, 10f, Time.deltaTime * 10);
                yield return null;
            }
            while(rigidbody.gravityScale > 10f){
                rigidbody.gravityScale = Mathf.Lerp(rigidbody.gravityScale, 1f, Time.deltaTime * 10);
                yield return null;
            }
            
        }
    }


    // could be used for any fall
    private void fallBlowUp(){

        
        if(isBlowup && !isHit && rigidbody.velocity.sqrMagnitude > 2f && fallAttackSenseTimer > 0.03f){ // blow up 状态落地
            
            rigidbody.drag = 1f;
            rigidbody.gravityScale = 1f; 
            hitAnimator.SetTrigger("Hit");
            AttackSense.Instance.HitPause(5);
            AttackSense.Instance.CameraShake(0.1f, 0.05f);
            groundTimer = 0f;
            fallAttackSenseTimer = 0f;
        }
        else if((rigidbody.velocity.sqrMagnitude < 0.01f && isBlowup && !isHit) || transform.rotation !=Quaternion.identity){
            groundTimer += Time.deltaTime;
            if(groundTimer > 3f){
                isBlowup = false;
                rigidbody.gravityScale = 1f;
                getUp();
                groundTimer = 0;     
            }
        }
        else groundTimer = 0;    
    }

    private void getUp(){
        isRotable = true;
        // if(Mathf.Abs(transform.rotation.z) < 60) transform.Rotate(new Vector3(0, 0, transform.localScale.x * 20));
        // rigidbody.constraints = RigidbodyConstraints2D.None;
        transform.position += new Vector3(0,0.2f,0); 
        transform.rotation = Quaternion.identity;
        groundTimer = 0f;
    }


    public void getDownForce(float waitTime){
        StopAllCoroutines();
        rigidbody.gravityScale = 1; // incase

        // gethit animation
        animator.SetTrigger("Hit");
        hitAnimator.SetTrigger("Hit");
        AttackSense.Instance.HitPause(5);
        // DOVirtual.Float(0.1f,0.8f,0.5f,setTimeScale).SetEase(Ease.InOutSine);

        StartCoroutine(addImpulseForce(waitTime, Vector2.down / 5, 0.3f));
    }


    public void getPushForce(float waitTime, Vector2 dir){
        StopAllCoroutines();
        rigidbody.gravityScale = 1; // incase

        // gethit animation
        animator.SetTrigger("Hit");
        hitAnimator.SetTrigger("Hit");
        AttackSense.Instance.HitPause(5);
        // DOVirtual.Float(0.1f,0.8f,0.5f,setTimeScale).SetEase(Ease.InOutSine);
        // DOVirtual.Float(1f,10f,2f,setGravityScale);

        StartCoroutine(addImpulseForce(waitTime, dir / 5, 0.3f));
        StartCoroutine(timerBlowUpGravity());
    }

    public void getPullForce(Vector3 dir, Vector3 target){
        StopAllCoroutines();
        rigidbody.gravityScale = 1; // incase
        rigidbody.drag = 1;

        // gethit animation
        animator.SetTrigger("Hit");
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.DOMove(target - Vector3.down * 0.3f, 0.2f);
        transform.GetComponent<Collider2D>().enabled = false;
        StartCoroutine(stopPullForce(target));
    }


    IEnumerator stopPullForce(Vector2 target){
        // FindObjectOfType<GhostTrail>().ShowGhostEnemy();
        while(Vector2.Distance(target, transform.position) > 1.3f) yield return null;
        rigidbody.velocity = Vector2.zero;
        //rigidbody.constraints = ~RigidbodyConstraints2D.FreezePositionY;
        rigidbody.drag = 1f;
        transform.GetComponent<Collider2D>().enabled = true;
        
    }

    IEnumerator addImpulseForce(float waitTime, Vector2 dir, float time = 0.2f){
        //rigidbody.isKinematic = true;
        yield return new WaitForSeconds(waitTime);
        rigidbody.AddForce(dir * 50,ForceMode2D.Impulse);
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

        yield return new WaitForSeconds(time);
        rigidbody.drag = 5f;
        //rigidbody.constraints = ~RigidbodyConstraints2D.FreezePositionY;
        yield return new WaitForSeconds(time);
        rigidbody.drag = 1f;
        //rigidbody.isKinematic = false;
    }


    private void setGravityScale(float gravity){
        rigidbody.gravityScale = gravity;
    }

    private void setTimeScale(float scale){
        Time.timeScale = scale;
    }


    private void StateUpdate(){
        if(rigidbody.velocity.SqrMagnitude() < 0.1f){
            rotateTimer += Time.deltaTime;
            if(rotateTimer > 4f){
                isBlowup = false;
                getUp();
                rotateTimer = 0;
            }
        }
        else rotateTimer = 0;
        
        if(!isGround && fallAttackSenseTimer < 0.3f){
            fallAttackSenseTimer += Time.deltaTime;
        }
        isGround = Physics2D.OverlapCircle(groundCheck.position + new Vector3(check_right.x, check_right.y, 0), check_right.z, layer);          
    }

    private void hitOver(){
        isHit = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position + new Vector3(check_right.x, check_right.y, 0), check_right.z);
        
    }

    private void update_ui(){
        string debug_UI = $"isblowwp:{isBlowup}   <br>velocity:{rigidbody.velocity} " +
        $"<br>fallAttackTime:{fallAttackSenseTimer} <br>isGround:{isGround} <br>groundTimer:{groundTimer}";
        debug_ui.text = debug_UI;
    }
        
}
    
    


