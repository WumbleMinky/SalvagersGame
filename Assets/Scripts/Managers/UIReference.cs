using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIReference : MonoBehaviour
{

    private static UIReference _inst;

    public static UIReference Instance { get 
        {
            if (_inst == null)
                _inst = GameObject.FindObjectOfType<UIReference>();
            return _inst;
        }
    }

    [Header("Main Game UI Panels")]
    public GameObject ConnectionPanel;
    public GameObject LobbyPanel;
    public GameObject InGamePanel;

    [Header("Lobby Elements")]
    public GameObject startGameButton;
    public GameObject lobbyNamePanel;
    public GameObject playerLobbyNamePrefab;
    public Dropdown startingLayoutDropDown;
    public Dropdown playerColorDropdown;

    [Header("In Game Elements")]
    public GameObject powerSelectionPanel;
    public GameObject tileEditPanel;
    public GameObject playerHandPanel;
    public GameObject playerStatusPanel;
    public GameObject playerStatusTextPrefab;
    public ObjectSelector ObjectSelector;

    public void Start()
    {
        initStartingLayouts();
        initPlayerColors();
    }

    private void initStartingLayouts()
    {
        List<string> dropdownAdds = new List<string>();
        foreach (StartingBoardLayout sbl in ResourceContainer.Instance.startingBoardLayouts)
        {
            dropdownAdds.Add(sbl.displayName);
        }
        startingLayoutDropDown.AddOptions(dropdownAdds);
    }

    private void initPlayerColors()
    {
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        foreach(ResourceContainer.PlayerColor pc in ResourceContainer.Instance.playerColors)
        {
            options.Add(new Dropdown.OptionData(pc.name, pc.sprite));
        }
        playerColorDropdown.AddOptions(options);
    }
}
