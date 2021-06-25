using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Enemy : Entity
{
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Unit unit;

    public bool isTraining;
    [SerializeField]
    bool _fighting;
    [SerializeField]
    bool _walking;

    public float healthValue;

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
                unit.healthBar.SetActive(_fighting);
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
            Move(rb, -Vector3.forward);
        }

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Die"))
            CastRays();
    }

    public override void JumpToStartPoint()
    {
        StartCoroutine(JumpCoroutine());
    }

    IEnumerator JumpCoroutine()
    {
        transform.DOJump(GameManage.instance.enemyJumpPoint.position, jumpPower, 1, jumpDuration);
        animator.SetTrigger("Jump");

        yield return new WaitForSeconds(jumpDuration);
        GetComponent<Rigidbody>().isKinematic = false;
        isTraining = false;
        Walking = true;
    }

    void CastRays()
    {
        float distance = 2f;
        Ray ray = new Ray(transform.position + new Vector3(0, transform.localScale.y / 2f, 0), transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distance))
        {
            if (hit.transform.CompareTag("Player"))
            {
                //canMove = false;
                Walking = false;
                rb.velocity = Vector3.zero;

                Fighting = true;
            }

            if (hit.transform.CompareTag("Enemy"))
            {
                //canMove = false;
                Walking = false;
                animator.Play("Idle", 0);
                rb.velocity = Vector3.zero;
            }
        }
        else
        {
            if (!isTraining && !LevelManage.instance.fail)
            {
                Walking = true;
            }
            //canMove = true;
            //Walking = true;
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.collider.CompareTag("Player"))
    //    {
    //        canMove = false;
    //        rb.velocity = Vector3.zero;
    //        rb.constraints = RigidbodyConstraints.FreezeAll;

    //        isFighting = true;
    //        animator.SetBool("isFighting", true);
    //    }

    //    if (collision.collider.CompareTag("Enemy"))
    //    {
    //        canMove = false;
    //        rb.velocity = Vector3.zero;
    //        rb.constraints = RigidbodyConstraints.FreezeAll;

    //        if (!isFighting)
    //        {
    //            animator.Play("Idle", 0);
    //        }
    //    }
    //}
}
