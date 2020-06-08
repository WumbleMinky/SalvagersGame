using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionPanel : MonoBehaviour
{
    SalvagerNetworkManager manager;

    private void Awake()
    {
        manager = GameObject.FindObjectOfType<SalvagerNetworkManager>();
    }

    public void HostButtonClicked()
    {
        manager.StartHost();
    }

    public void ClientButtonClicked()
    {
        manager.StartClient();
    }

    public void PlayerNameChanged(string value)
    {
        manager.playerName = value;
    }

    public void IPAddressChanged(string value)
    {
        manager.ipaddress = value;
    }
}
