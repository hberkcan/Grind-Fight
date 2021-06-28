using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using MoreMountains.NiceVibrations;

public class GameManage : MonoBehaviour
{
    public static GameManage instance;

    public GameObject player;
    public Vector3 playerSpawnPoint = new Vector3(0, 0, -36f);
    public Transform playerJumpPoint;
    public List<Player> playerList = new List<Player>();

    public GameObject enemy;
    public Vector3 enemySpawnPoint = new Vector3(0, 0, 36f);
    public Transform enemyJumpPoint;
    public List<Enemy> enemyList = new List<Enemy>();

    Player currentPlayer = null;
    float tempTime;
    float spawnRate = 0.2f;

    bool hasEnteredTraining;

    public Material red;
    public Material blue;
    public Material white;

    float rate = 1f;
    Coroutine rateCoroutine;
    Coroutine hapticCoroutine;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //currentPlayer = SpawnPlayer();
        hasEnteredTraining = false;
    }

    void Update()
    {
        if (currentPlayer == null && playerList.Count == 0 && LevelManage.instance.gameOn)
        {
            currentPlayer = SpawnPlayer();
        }
        else if (currentPlayer == null && playerList.Count > 0)
        {
            float distance1 = Mathf.Abs(playerList[playerList.Count - 1].transform.position.z - playerSpawnPoint.z);

            if (distance1 >= 12f)
            {
                currentPlayer = SpawnPlayer();
            }
        }

        if (currentPlayer && LevelManage.instance.gameOn)
        {
            float distance2 = Mathf.Abs(enemyList[0].transform.position.z - playerSpawnPoint.z);

            if (currentPlayer.isTraining)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    hasEnteredTraining = true;
                    currentPlayer.TrainingEnter(currentPlayer.animator);

                    MMVibrationManager.Haptic(HapticTypes.LightImpact);
                    //hapticCoroutine = StartCoroutine(HapticCoroutine());
                }

                if (Input.GetMouseButton(0))
                {
                    if(hasEnteredTraining == false)
                    {
                        hasEnteredTraining = true;
                        currentPlayer.TrainingEnter(currentPlayer.animator);

                        MMVibrationManager.Haptic(HapticTypes.LightImpact);
                        //hapticCoroutine = StartCoroutine(HapticCoroutine());
                    }

                    if (tempTime > 0)
                    {
                        tempTime -= Time.deltaTime;
                    }
                    else
                    {
                        tempTime = currentPlayer.trainingTime;
                        currentPlayer.TrainingStay(currentPlayer.unit, currentPlayer.animator);
                        
                        if(currentPlayer.unit.level < currentPlayer.maxLevel)
                        {
                            MMVibrationManager.Haptic(HapticTypes.LightImpact);
                            Debug.Log("haptic");
                        }
                    }
                }

                if (Input.GetMouseButtonUp(0) && distance2 >= 16f)
                {
                    hasEnteredTraining = false;
                    currentPlayer.TrainingExit(currentPlayer.unit, currentPlayer.animator);
                    currentPlayer.isTraining = false;

                    currentPlayer.JumpToStartPoint();
                    //StartCoroutine(SpawnPlayerCoroutine());
                    currentPlayer = null;

                    //StopCoroutine(hapticCoroutine);
                    //StopCoroutine(rateCoroutine);
                    //rate = 1f;
                }
            }
        }

        //if (!LevelManage.instance.gameOn && hapticCoroutine != null && rateCoroutine != null)
        //{
        //    StopCoroutine(hapticCoroutine);
        //    StopCoroutine(rateCoroutine);
        //    rate = 1f;
        //}
    }

    IEnumerator HapticCoroutine()
    {
        rateCoroutine = StartCoroutine(HapticRate());
        yield return new WaitForSeconds(1.8f);
        StopCoroutine(rateCoroutine);

        if(rate > 0.6f)
            rate -= 0.33f;

        hapticCoroutine = StartCoroutine(HapticCoroutine());
    }

    IEnumerator HapticRate()
    {
        MMVibrationManager.Haptic(HapticTypes.LightImpact);
        Debug.Log(rate);
        yield return new WaitForSeconds(rate);
        rateCoroutine = StartCoroutine(HapticRate());
    }

    Player SpawnPlayer()
    {
        Player p = Instantiate(player, playerSpawnPoint, player.transform.rotation).GetComponent<Player>();
        playerList.Add(p);

        tempTime = p.trainingTime;

        return p;
    }

    IEnumerator SpawnPlayerCoroutine()
    {
        yield return new WaitForSeconds(currentPlayer.jumpDuration + spawnRate);

        currentPlayer = SpawnPlayer();
    }

    public void ChangePlatformColor(Renderer renderer, Collider other)
    {
        if (other.CompareTag("Player") && renderer.material != red)
        {
            renderer.material = red;
        }

        if (other.CompareTag("Enemy") && renderer.material != blue)
        {
            renderer.material = blue;
        }
    }

    public void SetAlpha(Renderer rend,float alpha)
    {
        Material[] materials = rend.materials;
        Color newColor;

        foreach (Material m in materials)
        {
            newColor = m.color;
            newColor.a = alpha;
            m.color = newColor;
        }
    }

    public void Fight(Player player, Enemy enemy)
    {
        StartCoroutine(FightCoroutine(player, enemy));
    }

    IEnumerator FightCoroutine(Player player, Enemy enemy)
    {
        if (enemy == null)
            yield break;

        GameObject fightEffect = player.transform.GetChild(3).gameObject;
        fightEffect.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        float to;

        enemy.healthValue = enemy.unit.health;
        to = enemy.unit.health -= player.unit.damage;
        DOTween.To(() => enemy.healthValue, x => enemy.healthValue = x, to, 0.5f);

        if (enemy.unit.health <= 0)
        {
            //win
            player.Fighting = false;
            //player.unit.healthBar.SetActive(false);
            StartCoroutine(UIPopper(player.unit.healthBar, false, 0.3f));

            enemyList.Remove(enemy);

            enemy.unit.col.enabled = false;
            enemy.animator.SetTrigger("Die");
            StartCoroutine(UIPopper(enemy.unit.healthBar, false, 0.3f));
            SetAlpha(enemy.unit.rend, 0.5f);

            fightEffect.SetActive(false);
            yield return new WaitForSeconds(1f);

            if (enemy != null)
                Destroy(enemy.gameObject);

            yield break;
        }

        player.healthValue = player.unit.health;
        to = player.unit.health -= enemy.unit.damage;
        DOTween.To(() => player.healthValue, x => player.healthValue = x, to, 0.5f);

        if (player.unit.health <= 0)
        {
            //lose
            enemy.Fighting = false;
            StartCoroutine(UIPopper(enemy.unit.healthBar, false, 0.3f));

            playerList.Remove(player);

            player.unit.col.enabled = false;
            player.animator.SetTrigger("Die");
            StartCoroutine(UIPopper(player.unit.healthBar, false, 0.3f));
            SetAlpha(player.unit.rend, 0.5f);

            fightEffect.SetActive(false);
            yield return new WaitForSeconds(1);

            if (player != null)
                Destroy(player.gameObject);

            yield break;
        }

        //yield return new WaitForSeconds(0.5f);
        fightEffect.SetActive(false);

        StartCoroutine(FightCoroutine(player, enemy));
    }

    public IEnumerator UIPopper(GameObject obj, bool active, float duration)
    {
        if (active)
        {
            obj.SetActive(active);
            obj.transform.DOScale(new Vector3(1, 1, 1), duration);
        }
        else
        {
            obj.transform.DOScale(Vector3.zero, duration);
            yield return new WaitForSeconds(duration);
            obj.SetActive(active);
        }
    }
}
