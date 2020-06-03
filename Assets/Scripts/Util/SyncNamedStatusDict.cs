using Mirror;
using Mirror.Examples.Basic;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SyncNamedStatusDict : SyncDictionary<int, string>
{
    private Dictionary<int, string> playerNames;
    private string separator = ": ";

    public SyncNamedStatusDict() : base() 
    {
        playerNames = new Dictionary<int, string>();
    }

    public void Add(int key, string name, string value )
    {
        playerNames.Add(key, name);
        base.Add(key, name + separator + value);
    }

    public new string this[int key]
    {
        get
        {
            return base[key];
        }
        set
        {
            base[key] = playerNames[key] + separator + value;
        }
    }

    
}
