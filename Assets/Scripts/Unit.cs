using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Unit : MonoBehaviour
{
    public int level = 1;
    public float maxHealth = 100f;
    public float health = default;
    public float damage = default;
    public SkinnedMeshRenderer rend = default;
    public Collider col = default;
    public GameObject healthBar = default;
    public TextMeshProUGUI levelUI = default;
    public GameObject leftDumbell, rightDumbell = default;

    private void Start()
    {
        health = maxHealth;
    }
}
