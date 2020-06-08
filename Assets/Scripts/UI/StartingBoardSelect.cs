using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartingBoardSelect : MonoBehaviour
{
    public LobbyManager lm;
    Dropdown dropdown;
    ResourceContainer rc;

    // Start is called before the first frame update
    void Start()
    {
        dropdown = GetComponent<Dropdown>();
        rc = ResourceContainer.Instance;
        List<string> options = new List<string>();
        foreach(StartingBoardLayout sbl in rc.startingBoardLayouts)
        {
            options.Add(sbl.displayName);
        }
        dropdown.AddOptions(options);
    }

    public void layoutSelected()
    {
        lm.startingLayoutIndex = dropdown.value - 1;
    }
}
