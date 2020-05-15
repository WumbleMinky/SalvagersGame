using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HostOnly : NetworkBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (!isClient || !isServer)
        {
            gameObject.SetActive(false);
        }
    }
}
