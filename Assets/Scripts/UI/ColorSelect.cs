using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ColorSelect : MonoBehaviour
{
    public LobbyManager lm;
    Dropdown dropdown;
    int currentSelected = 0;
    private ResourceContainer rc;

    private void Start()
    {
        dropdown = GetComponent<Dropdown>();
        rc = ResourceContainer.Instance;
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        foreach(ResourceContainer.PlayerColor pc in rc.playerColors)
        {
            options.Add(new Dropdown.OptionData(pc.name, pc.sprite));
        }
        dropdown.AddOptions(options);
    }

    public void setColor(Color color)
    {
        for(int i = 0; i < rc.playerColors.Length; i++)
        {
            if (rc.playerColors[i].color == color)
            {
                dropdown.value = i;
                currentSelected = i;
                return;
            }
        }
    }

    public void onColorSelected()
    {
        Color c = rc.playerColors[dropdown.value].color;
        if (lm.localPlayer.color == c)
            return;
        if (!lm.isColorTaken(c))
        {
            lm.localPlayer.CmdChangeColor(c);
            currentSelected = dropdown.value;
        }
        else
        {
            dropdown.value = currentSelected;
        }
        
    }
}
