using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UnitUI : MonoBehaviour
{
    float fixedScale;
    GameObject parent;
    Unit unit;
    [SerializeField] Image fill = default;

    void Start()
    {
        fixedScale = transform.lossyScale.x;
        parent = transform.parent.gameObject;

        unit = parent.GetComponent<Unit>();
    }

    void Update()
    {
        transform.localScale = new Vector3(fixedScale / parent.transform.localScale.x, fixedScale / parent.transform.localScale.y, fixedScale / parent.transform.localScale.z);

        fill.fillAmount = unit.health / unit.maxHealth;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}
