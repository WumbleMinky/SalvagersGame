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

    [Header("In Game Elements")]
    public GameObject powerSelectionPanel;
    public GameObject tileEditPanel;
    public GameObject playerHandPanel;
    public GameObject playerStatusPanel;
    public GameObject playerStatusTextPrefab;
    public ObjectSelector ObjectSelector;
    public ConflictPanel conflictPanel;

    [Header("Ship Events")]
    public ShipEventPanel shipEventPanel;
    public string incomingShipEvent = "Ship Event Incoming";
    public string shipEvent01 = "DOOOM!";
}
