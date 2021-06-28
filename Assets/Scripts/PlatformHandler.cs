using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformHandler : MonoBehaviour
{
    [HideInInspector] public Renderer rend;
    public int triggerCount = 0;
    string triggerTag;
    List<Collider> triggers = new List<Collider>();

    enum WinCond {Win, Lose, None};
    [SerializeField] WinCond winCond = default;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerCount == 0)
        {
            triggerCount++;
            triggerTag = other.tag;
            triggers.Add(other);
        }

        if (triggerCount == 1 && triggerTag != other.tag)
        {
            triggerCount++;
            triggers.Add(other);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        for (var i = triggers.Count - 1; i > -1; i--)
        {
            if (triggers[i] == null || !triggers[i].enabled)
            {
                triggers.RemoveAt(i);
                triggerCount--;
            }
        }

        if (triggerCount == 1)
        {
            if (other.CompareTag("Player"))
            {
                rend.material = GameManage.instance.red;

                if (winCond == WinCond.Win && LevelManage.instance.gameOn && Vector3.Distance(other.transform.position,transform.position) < 0.2f)
                {
                    //win
                    LevelManage.instance.Pass();
                }
            }

            if (other.CompareTag("Enemy"))
            {
                rend.material = GameManage.instance.blue;

                if (winCond == WinCond.Lose && LevelManage.instance.gameOn && Vector3.Distance(other.transform.position, transform.position) < 0.2f)
                {
                    //lose
                    LevelManage.instance.Fail();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (triggers.Contains(other))
        {
            triggers.Remove(other);
            triggerCount--;
        }
    }
}
