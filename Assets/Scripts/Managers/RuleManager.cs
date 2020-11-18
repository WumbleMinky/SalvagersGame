using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuleManager : MonoBehaviour
{

    private static RuleManager _inst;

    public static RuleManager Instance { 
        get 
        {
            if (_inst == null)
                _inst = GameObject.FindObjectOfType<RuleManager>();
            return _inst;
        }
    }

    public int distanceToLoot = 4;
    public int minPlayersForLoot = 3;
    public int minNumberOfDrones = 2;
    public int minPlayersForExtraDrone = 5;

    public void Awake()
    {
        if (RuleManager.Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }
}
