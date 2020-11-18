using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConflictPanel : MonoBehaviour
{
    public static readonly int LEFTSIDE = -1;
    public static readonly int RIGHTSIDE = 1;

    public delegate void OnRollClicked(int side);
    public static event OnRollClicked OnRollClickedDelegate;

    public bool leftAnimComplete = false;
    public bool rightAnimComplete = false;

    [Header("Combatant 1")]
    public Text LeftName;
    public Text[] LeftDiceTexts;
    public Text LeftDiceTotal;
    public GameObject LeftRollButton;

    [Header("Combatant 2")]
    public Text RightName;
    public Text[] RightDiceTexts;
    public Text RightDiceTotal;
    public GameObject RightRollButton;

    [Header("Result Display")]
    public GameObject leftLostImage;
    public GameObject rightLostImage;

    public void setCombatant1(string name, bool isDrone = false)
    {
        LeftName.text = name;
        foreach (Text diceText in LeftDiceTexts)
        {
            diceText.text = "";
        }
        LeftDiceTexts[3].gameObject.SetActive(isDrone);
        LeftDiceTotal.text = "";
        LeftRollButton.SetActive(false);
        leftLostImage.SetActive(false);
    }

    public void setCombatant2(string name, bool isDrone = false)
    {
        RightName.text = name;
        foreach(Text diceText in RightDiceTexts)
        {
            diceText.text = "";
        }
        RightDiceTexts[3].gameObject.SetActive(isDrone);
        RightDiceTotal.text = "";
        RightRollButton.SetActive(false);
        rightLostImage.SetActive(false);
    }

    public void Clear()
    {
        leftAnimComplete = false;
        rightAnimComplete = false;
        leftLostImage.SetActive(false);
        rightLostImage.SetActive(false);
        LeftName.text = "";
        RightName.text = "";
        LeftDiceTotal.text = "";
        RightDiceTotal.text = "";
        leftAnimComplete = false;
        rightAnimComplete = false;
        for(int i = 0; i < LeftDiceTexts.Length; i++)
        {
            LeftDiceTexts[i].text = "";
            RightDiceTexts[i].text = "";
        }
    }

    public void LeftRollClick()
    {
        OnRollClickedDelegate?.Invoke(LEFTSIDE);
    }

    public void RightRollClick()
    {
        OnRollClickedDelegate?.Invoke(RIGHTSIDE);
    }

    public void activateSideRollButton(int side)
    {
        if (side == ConflictPanel.LEFTSIDE)
        {
            LeftRollButton.SetActive(true);

        }
        else
        {
            RightRollButton.SetActive(true);
        }
    }

    public bool bothAnimsComplete()
    {
        return leftAnimComplete && rightAnimComplete;
    }

    public void displayLostImage(int side)
    {
        if (side == LEFTSIDE)
            leftLostImage.SetActive(true);
        else
            rightLostImage.SetActive(true);
    }
}
