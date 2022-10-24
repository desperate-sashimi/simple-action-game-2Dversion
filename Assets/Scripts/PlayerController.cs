using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class PlayerController : MonoBehaviour
{
# region field
    
    [Header("补偿速度")]
    public float lightSpeed;
    public float heavySpeed;

    [Header("打击感")]
    public float shakeTime;
    public int lightPause;
    public float lightStrength;
    public int heavyPause;
    public float heavyStrength;

    [Header("基本设定")]
    // attack field
    public float interval = 2f;
    private float timer;
    public bool isAttack;
    public bool hasDashAttack;
    public string attackType;
    // move field
    public float moveSpeed;
    public float jumpForce;
    public float blowupSpeed;
    new private Rigidbody2D rigidbody;
    [Header("动感")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("输入")]
    private float input_raw_x; // raw
    private float input_raw_y;
    private float input; // not raw

    [Header("状态")]
    public bool hasSecondJumped = true;
    private Collision coll;
    private Anim_Controller anim;

    [Header("浮空")]
    private float attackAboveOffset = 0.05f;
    public float attackAboveTime;
    private float maxAttackAboveTime_fall = 3f;
    private float maxAttackAboveTime_over = 4f;
    private float coolDownDashAttack = 2f;
    private Vector2 pushdir;

    [Header("挑飞")]
    private float timerBlowUp;
    private bool isBlowUp;

    [Header("敌人检测")]
    private EnemyDetector enemyDetector;


    [Header("air dash")]
    [SerializeField]private float dashSpeed;
    public bool isDashing;
    public bool hasDashed;
    public bool isDashAttack;
    

    [Header("DeBug")]
    [SerializeField]private TextMeshProUGUI debug;
# endregion

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collision>();
        anim = GetComponent<Anim_Controller>();
        Time.timeScale = 0.8f;
        enemyDetector = GetComponentInChildren<EnemyDetector>();
    }

    void Update()
    {
        input_raw_x = Input.GetAxisRaw("Horizontal");
        input_raw_y = Input.GetAxisRaw("Vertical");
        input = Input.GetAxis("Horizontal");

        Move();
        Attack();
        update_debug_UI();
    }

    private void Move()
    {     
        if (!isAttack && !isBlowUp && !isDashing & !isDashAttack){
            rigidbody.velocity = new Vector2(input_raw_x * moveSpeed, rigidbody.velocity.y);
            if(rigidbody.velocity.y < 0)
                rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            else if(rigidbody.velocity.y > 0)
                rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        } 
        else if (!isBlowUp && !isDashing)
        {
            if (attackType == "Light"){
                if(coll.isGround) rigidbody.velocity = new Vector2(transform.localScale.x * lightSpeed, rigidbody.velocity.y);
                else rigidbody.velocity = new Vector2(transform.localScale.x * lightSpeed * 0.05f, rigidbody.velocity.y);
            }
                
            else if (attackType == "Heavy"){
                if(coll.isGround) rigidbody.velocity = new Vector2(transform.localScale.x * heavySpeed, rigidbody.velocity.y);
                else rigidbody.velocity = new Vector2(transform.localScale.x * lightSpeed * 0.05f, rigidbody.velocity.y);
            } 
        }

        // jump
        if (Input.GetButtonDown("Jump") && coll.isGround && !isDashing)
        {     
            rigidbody.velocity = new Vector2(0, jumpForce);
            anim.jump();
        }
        else if(Input.GetButtonDown("Jump") && hasSecondJumped && !isDashing){
            rigidbody.gravityScale = 1f;
            rigidbody.velocity = new Vector2(0, jumpForce -3f);
            hasSecondJumped = false;
            anim.jump();
        }

        // dash
        if (Input.GetButtonDown("Fire3") && !hasDashed){
            Dash(input_raw_x, input_raw_y);
        }

        // rotation
        if (rigidbody.velocity.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (rigidbody.velocity.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
    }


    # region attack
    // when trigger with other enemy
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Enemy>() && Vector2.Distance(other.transform.position, transform.position) < 3f)
        {
            if (attackType == "Light")
            {
                other.GetComponent<Enemy>().GetHit(transform.localScale.x > 0?Vector2.right:Vector2.left);
                AttackSense.Instance.HitPause(lightPause);
                AttackSense.Instance.CameraShake(shakeTime, lightStrength);
            }
            else if (attackType == "Heavy")
            {
                other.GetComponent<Enemy>().GetHit(transform.localScale.x > 0?Vector2.right:Vector2.left);
                AttackSense.Instance.HitPause(heavyPause);
                AttackSense.Instance.CameraShake(shakeTime, heavyStrength);
            }
            else if(attackType == "BlowUp"){
                other.GetComponent<Enemy>().GetBlowUp(transform.localScale.x > 0?Vector2.right:Vector2.left);
                AttackSense.Instance.HitPause(heavyPause);
                AttackSense.Instance.CameraShake(shakeTime, heavyStrength);
                
            }
            else if(attackType == "GetDown"){
                other.GetComponent<Enemy>().getDownForce(heavyPause / 60f);
                rigidbody.drag = 5f;
                DOVirtual.Float(5f,0f,0.5f,RigidBodyDrag);
                // other.GetComponent<Enemy>().GetHit(transform.localScale.x > 0?Vector2.right:Vector2.left, true);
                AttackSense.Instance.ContinousPause(heavyPause + 5);
                AttackSense.Instance.CameraShake(shakeTime, heavyStrength+0.15f);
            }
            else if(attackType == "Push"){
                other.GetComponent<Enemy>().getPushForce(heavyPause / 60f, pushdir);
                rigidbody.drag = 5f;
                DOVirtual.Float(5f,0f,0.5f,RigidBodyDrag);
                // other.GetComponent<Enemy>().GetHit(transform.localScale.x > 0?Vector2.right:Vector2.left, true);
                AttackSense.Instance.ContinousPause(heavyPause + 5);
                AttackSense.Instance.CameraShake(shakeTime, heavyStrength+0.15f);
            }
        }
    }

    // attack
    private void Attack()
    {
        if (Input.GetMouseButtonUp(0) && !isAttack && !isBlowUp)
        {
            isAttack = true;
            attackType = "Light";
            anim.LightAttack();
            timer = interval;
            timerBlowUp = 0f; // refresh the blowup caculator
        }
        if (Input.GetMouseButtonUp(1) && !isAttack && !isBlowUp)
        {
            isAttack = true;
            attackType = coll.isGround?"Heavy": "GetDown";
            anim.HeavyAttack();
            timer = interval;
        }
        if (Input.GetMouseButton(0) && !isBlowUp && coll.isGround){
            BlowUpEffect();
        }
        if(Input.GetKeyDown(KeyCode.R) && !isBlowUp && !coll.isGround && !hasDashAttack){
            DashAttack();
        }
        if(Input.GetKeyDown(KeyCode.F) && !isAttack && !isBlowUp && !coll.isGround){
            pushdir = transform.localScale.x >0 ?new Vector2(0.5f, 0.75f).normalized : new Vector2(-0.5f, 0.75f).normalized;
            isAttack = true;
            attackType = "Push";
            anim.LightAttack();
            timer = interval;        
        }
        // 浮空攻击, for velocity and gravity, 
        if(!coll.isGround && isAttack && !isBlowUp){
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0f);
            attackAboveTime += Time.deltaTime; // timer
            if(attackAboveTime < maxAttackAboveTime_fall) rigidbody.gravityScale = 0.05f;
            else if(attackAboveTime < maxAttackAboveTime_over && attackAboveTime > maxAttackAboveTime_fall) 
                rigidbody.gravityScale = Mathf.Lerp(rigidbody.gravityScale, 1f, Mathf.Min(1f,attackAboveTime - maxAttackAboveTime_fall));
            else if(attackAboveTime > maxAttackAboveTime_over) 
                rigidbody.gravityScale = Mathf.Lerp(rigidbody.gravityScale, 10f, Mathf.Min(1f,attackAboveTime - maxAttackAboveTime_over));
        }

        if(Input.GetKeyDown(KeyCode.E) && !isAttack && !isBlowUp && !isDashing){
            PullBack();
        }
        
        // count the time interval to refresh comboStep and gravity
        if (timer != 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0;
                anim.ComboStep = 0;
                rigidbody.gravityScale = 1f;
            }
        }

        if(hasDashAttack && coolDownDashAttack != 0f){
            coolDownDashAttack -= Time.deltaTime;
            if(coolDownDashAttack < 0f){
                coolDownDashAttack = 0f;
                hasDashAttack = false;
            }
        }
    
    }


    // TODO: kick off the enemy
    private void KickOff(Vector2 dir){

        isAttack = true;
        attackType = "Push";
        anim.HeavyAttack();
        timer = interval;

    }

    // TODO: dash attack
    private void DashAttack(){
        if(!isBlowUp){
            if(enemyDetector.EnemyDetectedList.Count != 0){
                Enemy enemy = enemyDetector.EnemyDetectedList[0].GetComponent<Enemy>();

                // set anim and property
                isDashAttack = true;
                isAttack = false;
                isBlowUp = false;
                hasDashAttack = true;
                anim.dash();
                coolDownDashAttack = 2f;
                rigidbody.drag = 0f;
                FindObjectOfType<GhostTrail>().ShowGhost();
                Vector2 dir = new Vector2(enemy.transform.position.x - transform.position.x, enemy.transform.position.y - transform.position.y);
                // Vector2 dir = new Vector2(enemy.transform.position.x - transform.position.x, 0);
                transform.DOMove(enemy.transform.position + Vector3.up, 0.15f).SetEase(Ease.InOutSine);
                enemy.GetComponent<Rigidbody2D>().gravityScale = 0f;
                if(enemy.transform.localScale.x > 0) transform.localScale = new Vector3(-1,1,1);
                else if(enemy.transform.localScale.x < 0) transform.localScale = new Vector3(1,1,1);
                Invoke("appearEnemyBehind", 0.15f);
            }
        }
    }

    private void appearEnemyBehind(){
        isDashAttack = false;
        anim.LightAttack();
        attackType = "Heavy";
        isAttack = true;
        timerBlowUp = 0f;
        rigidbody.drag = 0f;
        rigidbody.velocity = Vector2.zero;
                            
    }
    

    // TODO: pull back the enemy

    /*
        基本思路：
        1、 按E加鼠标左键，对一定半径内敌人进行检测，取到最近的一名敌人
        2、 将敌人拉过来
    */
    private void PullBack(){
        if(enemyDetector.EnemyDetectedList.Count != 0){
            Enemy enemy = enemyDetector.EnemyDetectedList[0].GetComponent<Enemy>();
            Vector3 enemyPos = enemy.transform.position;
            
            enemy.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            isAttack = true;
            anim.LightAttack();
            attackType = "Heavy";
            enemy.getPullForce((transform.position - enemyPos - Vector3.up*0.7f).normalized, transform.position - Vector3.up*0.7f);
        }
    }


    // TODO: air dash
    private void Dash(float x, float y){
        isAttack = false;
        isBlowUp = false;

        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        // FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));
        
        
        rigidbody.velocity = Vector2.zero;
        // if it's just a normal dash
        hasDashed = true;
        anim.dash();
        if(x<0.1f && y<0.1f){
            rigidbody.velocity = new Vector2(transform.localScale.x * dashSpeed,0);
        }
        else{
            Vector2 dir = new Vector2(x, y);
            rigidbody.velocity += dir.normalized * dashSpeed;
        }
        StartCoroutine(DashWait());
        
    }

    IEnumerator DashWait(){
        FindObjectOfType<GhostTrail>().ShowGhost();
        DOVirtual.Float(14, 0, .8f, RigidBodyDrag);

        rigidbody.gravityScale = 0f;
        isDashing = true;
        yield return new WaitForSeconds(0.5f);
        rigidbody.gravityScale = 1f;
        isDashing = false;
    }


    private void BlowUpEffect(){
        timerBlowUp += Time.deltaTime;
        if(timerBlowUp > 0.2f){
            isAttack = false;
            isBlowUp = true;
            attackType = "BlowUp";    
            anim.BlowUp();
            rigidbody.velocity = new Vector2(0, blowupSpeed);
            rigidbody.drag += 1f;
        } 
    }

    private void AttackOver()
    {
        isAttack = false;
        isBlowUp = false;
        timerBlowUp = 0f;
    }



    private void BlowUpOver(){
        isBlowUp = false;
        timerBlowUp = 0f;
        rigidbody.drag = 0f;
    }

    private void RigidBodyDrag(float drag){
        rigidbody.drag = drag;
    }
    # endregion

    private void update_debug_UI(){
        string debug_UI = $"gravity:{rigidbody.gravityScale}   <br>velocity:{rigidbody.velocity} <br>blowup:{timerBlowUp}" 
        +$"<br>isattack:{isAttack} <br>comboStep:{anim.ComboStep} <br>isBlowUp:{isBlowUp} <br>attackType:{attackType}"+
        $"<br>hasDashed:{hasDashed}";
        debug.text = debug_UI;
    }
}