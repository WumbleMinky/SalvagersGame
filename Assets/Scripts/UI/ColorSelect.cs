using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ColorSelect : MonoBehaviour
{

    PlayerData player;

    void Awake()
    {
        PlayerData.startLocalPlayerCallback += localPlayerStarted;
    }

    private void OnDestroy()
    {
        PlayerData.startLocalPlayerCallback -= localPlayerStarted;
    }

    public void localPlayerStarted(PlayerData pd)
    {
        player = pd;
    }

    public void onColorSelected()
    {
        int index = UIReference.Instance.playerColorDropdown.value;
        Color c = ResourceContainer.Instance.playerColors[index].color;
        player.CmdChangeColor(c);
    }
}
