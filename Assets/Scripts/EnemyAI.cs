using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum State
    {
        none,
        trainingStarted,
        isTraining,
        trainingEnded
    }

    Enemy currentEnemy;
    private State state;
    private State previousState;
    float tempTime;

    void Start()
    {
        //currentEnemy = SpawnEnemy();
        //StartCoroutine(StateHandler(2.5f));
        previousState = State.none;
        //tempTime = currentEnemy.trainingTime;
    }

    void Update()
    {
        switch (state)
        {
            case State.trainingStarted:
                currentEnemy.TrainingEnter(currentEnemy.animator);
                state = State.isTraining;
                break;
            case State.isTraining:
                if (tempTime > 0)
                {
                    tempTime -= Time.deltaTime;
                }
                else
                {
                    tempTime = currentEnemy.trainingTime;

                    currentEnemy.TrainingStay(currentEnemy.unit, currentEnemy.animator);
                }
                break;
            case State.trainingEnded:
                if(previousState != state && !LevelManage.instance.pass)
                {
                    currentEnemy.TrainingExit(currentEnemy.unit, currentEnemy.animator);
                    currentEnemy.isTraining = false;
                    state = State.none;
                    currentEnemy.JumpToStartPoint();
                    //StartCoroutine(SpawnEnemyCoroutine());
                    currentEnemy = null;
                }
                break;
        }

        if (currentEnemy == null && GameManage.instance.enemyList.Count == 0 && LevelManage.instance.gameOn)
        {
            currentEnemy = SpawnEnemy();

            StartCoroutine(StateHandler());
        }
        else if (currentEnemy == null && GameManage.instance.enemyList.Count > 0 && LevelManage.instance.gameOn)
        {
            float distance = Mathf.Abs(GameManage.instance.enemyList[GameManage.instance.enemyList.Count - 1].transform.position.z - GameManage.instance.enemySpawnPoint.z);

            if (distance >= 12f)
            {
                currentEnemy = SpawnEnemy();

                StartCoroutine(StateHandler());
            }
        }
    }

    Enemy SpawnEnemy()
    {
        Enemy e = Instantiate(GameManage.instance.enemy, GameManage.instance.enemySpawnPoint, GameManage.instance.enemy.transform.rotation).GetComponent<Enemy>();
        GameManage.instance.enemyList.Add(e);

        tempTime = e.trainingTime;

        return e;
    }

    IEnumerator SpawnEnemyCoroutine()
    {
        yield return new WaitForSeconds(currentEnemy.jumpDuration);

        currentEnemy = SpawnEnemy();

        StartCoroutine(StateHandler());
    }

    IEnumerator StateHandler()
    {
        float trainDelay = Random.Range(0, 1.5f);
        float trainTime = Random.Range(0.1f, 4.5f);

        yield return new WaitForSeconds(trainDelay);
        state = State.trainingStarted;
        yield return new WaitForSeconds(trainTime);
        state = State.trainingEnded;
        yield return new WaitForSeconds(currentEnemy.jumpDuration);
    }
}
