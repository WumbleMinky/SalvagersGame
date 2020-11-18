using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipEventPanel : MonoBehaviour
{
    public Text displayText;
    public CenteredLayoutController diceLayoutController;

    public string text { get
        {
            return displayText.text;
        } 
        set
        {
            displayText.gameObject.SetActive(!(value == ""));
            displayText.text = value;
        }
    }

    public void activate()
    {
        gameObject.SetActive(true);
    }

    public void deactivate()
    {
        foreach(Transform child in diceLayoutController.transform)
        {
            Destroy(child.gameObject);
        }
        gameObject.SetActive(false);
    }
}
