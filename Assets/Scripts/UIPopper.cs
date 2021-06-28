using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIPopper : MonoBehaviour
{
    Vector3 targetScale = new Vector3(1, 1, 1);
    [SerializeField] float duration = 0.3f;

    private void OnEnable()
    {
        //transform.DOScale(targetScale, duration);
    }

    private void OnDisable()
    {
        transform.DOScale(Vector3.zero, duration);
    }
}
