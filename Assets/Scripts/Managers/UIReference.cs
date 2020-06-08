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

    //[Header("Main Game UI Panels")]
    //public GameObject ConnectionPanel;
    //public GameObject LobbyPanel;
    //public GameObject InGamePanel;

    //[Header("Lobby Elements")]
    //public GameObject startGameButton;
    //public GameObject lobbyNamePanel;
    //public GameObject playerLobbyNamePrefab;
    //public Dropdown startingLayoutDropDown;
    //public Dropdown playerColorDropdown;

    [Header("In Game Elements")]
    public GameObject powerSelectionPanel;
    public GameObject tileEditPanel;
    public GameObject playerHandPanel;
    public GameObject playerStatusPanel;
    public GameObject playerStatusTextPrefab;
    public ObjectSelector ObjectSelector;
}
