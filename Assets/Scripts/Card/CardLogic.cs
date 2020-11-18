using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class CardLogic : MonoBehaviour
{
    public abstract void performAction(GameManager gm, PlayerData pd, Card choiceCard = null);
}
