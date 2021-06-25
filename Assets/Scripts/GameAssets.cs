using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    private static GameAssets _i;

    public static GameAssets i
    {
        get
        {
            if (_i == null) _i = (Instantiate(Resources.Load("gameassets")) as GameObject).GetComponent<GameAssets>();
            //Debug.Log(_i);
            return _i;
        }
    }

    public GameObject character;
    public GameObject fightEffect;
}
