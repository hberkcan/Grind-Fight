using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Player : Entity
{
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Unit unit;

    [HideInInspector] public bool isTraining;

    RaycastHit hit;

    public float healthValue;

    [SerializeField]
    bool _fighting;
    [SerializeField]
    bool _walking;

    public bool Walking
    {
        get { return _walking; }
        set
        {
            if (value == _walking) return;
            _walking = value;

            if(_walking)
                animator.SetTrigger("isWalking");
        }
    }

    public bool Fighting
    {
        get { return _fighting; }
        set
        {
            if (value == _fighting) return;
            _fighting = value;
            animator.SetBool("isFighting", _fighting);
            

            if (_fighting)
            {
                GameManage.instance.Fight(this, hit.transform.GetComponent<Enemy>());
                unit.healthBar.SetActive(true);
            }
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        unit = GetComponent<Unit>();

        isTraining = true;

        healthValue = unit.maxHealth;
        unit.damage = minDamage;
    }

    void Update()
    {
        unit.health = healthValue;
    }

    void FixedUpdate()
    {
        if (!isTraining && Walking)
        {
            Move(rb, Vector3.forward);
        }

        if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Die"))
            CastRays();
    }

    public override void JumpToStartPoint()
    {
        StartCoroutine(JumpCoroutine());
    }

    IEnumerator JumpCoroutine()
    {
        transform.DOJump(GameManage.instance.playerJumpPoint.position, jumpPower, 1, jumpDuration);
        animator.SetTrigger("Jump");

        transform.DORotate(Vector3.zero, jumpDuration);

        yield return new WaitForSeconds(jumpDuration);
        GetComponent<Rigidbody>().isKinematic = false;
        isTraining = false;
        Walking = true;
    }

    void CastRays()
    {
        float distance = 2f;
        Ray ray = new Ray(transform.position + new Vector3(0, transform.localScale.y / 2f, 0), transform.forward);
        //RaycastHit hit;

        if(Physics.Raycast(ray,out hit, distance))
        {
            if (hit.transform.CompareTag("Enemy"))
            {
                //canMove = false;
                Walking = false;
                rb.velocity = Vector3.zero;

                Fighting = true;

                //StartCoroutine(Fight(hit.transform.GetComponent<Enemy>()));
            }

            if (hit.transform.CompareTag("Player"))
            {
                //canMove = false;
                Walking = false;
                animator.Play("Idle", 0);
                rb.velocity = Vector3.zero;
            }
        }
        else
        {
            if (!isTraining && !LevelManage.instance.pass)
            {
                Walking = true;
            }
            //canMove = true;
            //Walking = true;
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.collider.CompareTag("Enemy"))
    //    {
    //        canMove = false;
    //        rb.velocity = Vector3.zero;
    //        rb.constraints = RigidbodyConstraints.FreezeAll;

    //        isFighting = true;
    //        animator.SetBool("isFighting", true);

    //        StartCoroutine(Fight(collision.transform.GetComponent<Enemy>()));
    //    }

    //    if (collision.collider.CompareTag("Player"))
    //    {
    //        canMove = false;
    //        rb.velocity = Vector3.zero;
    //        rb.constraints = RigidbodyConstraints.FreezeAll;

    //        if (!isFighting)
    //        {
    //            //animator.Play("Idle", 0);
    //        }
    //    }
    //}


    //IEnumerator Fight(Enemy enemy)
    //{
    //    if (enemy == null)
    //        yield break;

    //    transform.GetChild(3).gameObject.SetActive(true);
    //    yield return new WaitForSeconds(0.6f);

    //    //enemy.unit.health -= unit.damage;

    //    float to;

    //    enemy.healthValue = enemy.unit.health;
    //    to = enemy.unit.health -= unit.damage;
    //    DOTween.To(() => enemy.healthValue, x => enemy.healthValue = x, to, 0.5f);

    //    if (enemy.unit.health <= 0)
    //    {
    //        //win
    //        GameManage.instance.enemyList.Remove(enemy);

    //        if(enemy!=null)
    //            Destroy(enemy.gameObject);

    //        //foreach(Player p in GameManage.instance.playerList)
    //        //{
    //        //    p.canMove = true;
    //        //    p.rb.constraints = RigidbodyConstraints.None;
    //        //    p.rb.constraints = RigidbodyConstraints.FreezeRotation;

    //        //    isFighting = false;
    //        //    animator.SetBool("isFighting", false);
    //        //    animator.SetTrigger("isWalking");
    //        //}

    //        transform.GetChild(3).gameObject.SetActive(false);
    //        yield return new WaitForSeconds(0.5f);
    //        Fighting = false;
    //        enemy.Fighting = false;

    //        yield break;
    //    }
        
    //    healthValue = unit.health;
    //    to = unit.health -= enemy.unit.damage;
    //    DOTween.To(() => healthValue, x => healthValue = x, to, 0.5f);

    //    if (unit.health <= 0)
    //    {
    //        //lose
    //        GameManage.instance.playerList.Remove(this);
    //        Destroy(gameObject);

    //        //foreach (Enemy e in GameManage.instance.enemyList)
    //        //{
    //        //    e.canMove = true;
    //        //    e.rb.constraints = RigidbodyConstraints.None;
    //        //    e.rb.constraints = RigidbodyConstraints.FreezeRotation;

    //        //    e.isFighting = false;
    //        //    e.animator.SetBool("isFighting", false);
    //        //    e.animator.SetTrigger("isWalking");
    //        //}

    //        transform.GetChild(3).gameObject.SetActive(false);
    //        yield return new WaitForSeconds(0.5f);
    //        enemy.Fighting = false;
    //        Fighting = false;

    //        yield break;
    //    }

    //    transform.GetChild(3).gameObject.SetActive(false);
    //    yield return new WaitForSeconds(0.6f);

    //    StartCoroutine(Fight(enemy));
    //}
}
