using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    protected float movementSpeed = 5f;
    protected float jumpPower = 6f;
    [HideInInspector] public float jumpDuration = 0.5f;

    [HideInInspector] public int maxLevel = 20;
    [HideInInspector] public float trainingTime = 0.5f;
    [HideInInspector] public float minDamage = 10f;

    public virtual void Move(Rigidbody rb, Vector3 direction)
    {
        rb.velocity = movementSpeed * direction;
    }

    public virtual void TrainingEnter(Animator animator)
    {
        animator.SetBool("Training", true);
        
    }

    public virtual void TrainingStay(Unit unit, Animator animator)
    {
        if(unit.level < maxLevel)
        {
            unit.level++;
        }

        unit.damage = minDamage + (unit.level * unit.level) / 10f;
        unit.transform.localScale += new Vector3(0.03f, 0.03f, 0.03f);

        unit.levelUI.gameObject.SetActive(true);
        unit.levelUI.text = "Level " + unit.level.ToString();

        animator.SetInteger("Level", unit.level);

        float blendWeight = unit.rend.GetBlendShapeWeight(0);
        blendWeight += 5f;
        blendWeight = Mathf.Clamp(blendWeight, 0, 100f);
        unit.rend.SetBlendShapeWeight(0, blendWeight);

        if (!unit.leftDumbell.activeSelf)
        {
            unit.leftDumbell.SetActive(true);
            unit.rightDumbell.SetActive(true);
        }
    }

    public virtual void TrainingExit(Unit unit , Animator animator)
    {
        animator.SetBool("Training", false);
        unit.leftDumbell.SetActive(false);
        unit.rightDumbell.SetActive(false);

        unit.levelUI.gameObject.SetActive(false);
    }

    public abstract void JumpToStartPoint();
}
