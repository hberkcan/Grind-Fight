using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManage : MonoBehaviour
{
    public static LevelManage instance;

    [SerializeField] GameObject startUI;
    [SerializeField] GameObject passUI;
    [SerializeField] GameObject failUI;
    [SerializeField] GameObject levelUI;
    [SerializeField] TextMeshProUGUI levelText;

    float delayUI = 1f;

    int level;
    public bool gameOn = false;
    public bool pass = false;
    public bool fail = false;

    [SerializeField] PlatformHandler[] platforms;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (PlayerPrefs.HasKey("Level"))
        {
            level = PlayerPrefs.GetInt("Level");
        }
        else
        {
            level = 1;
        }

        levelText.text = "Level " + level.ToString();

        startUI.SetActive(true);
    }

    void Update()
    {
        //if (pass && gameOn)
        //{
        //    foreach (Player p in GameManage.instance.playerList)
        //    {
        //        p.Walking = false;
        //        p.rb.velocity = Vector3.zero;
        //        p.animator.ResetTrigger("isWalking");
        //        p.animator.SetTrigger("Celebrate");
        //    }
        //}
        
        //if (fail && gameOn)
        //{
        //    foreach(Enemy e in GameManage.instance.enemyList)
        //    {
        //        e.Walking = false;
        //        e.rb.velocity = Vector3.zero;
        //        e.animator.ResetTrigger("isWalking");
        //        e.animator.SetTrigger("Celebrate");
        //    }
        //}
    }

    public void GameOn()
    {
        startUI.SetActive(false);

        gameOn = true;
        levelUI.SetActive(true);
    }

    public void Pass()
    {
        pass = true;
        gameOn = false;

        foreach (Player p in GameManage.instance.playerList)
        {
            p.Walking = false;
            p.rb.velocity = Vector3.zero;
            p.animator.ResetTrigger("isWalking");
            p.animator.SetTrigger("Celebrate");
        }

        level++;
        PlayerPrefs.SetInt("Level", level);

        StartCoroutine(PassCoroutine());
    }

    IEnumerator PassCoroutine()
    {
        yield return new WaitForSeconds(delayUI);
        passUI.SetActive(true);
        levelUI.SetActive(false);
    }

    public void Fail()
    {
        fail = true;
        gameOn = false;

        foreach (Enemy e in GameManage.instance.enemyList)
        {
            e.Walking = false;
            e.rb.velocity = Vector3.zero;
            e.animator.ResetTrigger("isWalking");
            e.animator.SetTrigger("Celebrate");
        }

        StartCoroutine(FailCoroutine());
    }

    IEnumerator FailCoroutine()
    {
        yield return new WaitForSeconds(delayUI);
        failUI.SetActive(true);
        levelUI.SetActive(false);
    }

    public void ReloadScene()
    {
        //if(GameManage.instance.playerList.Count > 0)
        //{
        //    foreach (Player p in GameManage.instance.playerList)
        //    {
        //        Destroy(p.gameObject);
        //    }
        //}

        //if (GameManage.instance.enemyList.Count > 0)
        //{
        //    foreach (Enemy e in GameManage.instance.enemyList)
        //    {
        //        Destroy(e.gameObject);
        //    }
        //}

        //GameManage.instance.playerList.Clear();
        //GameManage.instance.enemyList.Clear();

        //for(int i = 0; i < platforms.Length; i++)
        //{
        //    platforms[i].rend.material = GameManage.instance.white;
        //}

        //if (fail)
        //{
        //    fail = false;
        //    failUI.SetActive(false);
        //}

        //if (pass)
        //{
        //    pass = false;
        //    passUI.SetActive(false);
        //}

        //gameOn = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
