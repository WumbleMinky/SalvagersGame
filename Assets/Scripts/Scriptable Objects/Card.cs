using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card")]
public class Card : ScriptableObject
{
    public Transform prefab;
    
    public string title;
    public string description;
    public int cooldown;
    public bool choice;

    public CardLogic logic;

    public GameObject createPrefab(Transform parent = null)
    {
        GameObject go = MonoBehaviour.Instantiate(prefab).gameObject;
        PlayerHandLayoutController phlc = parent.GetComponent<PlayerHandLayoutController>();
        if (phlc != null)
        {
            phlc.addItem(go);
        }
        else
        {
            go.transform.SetParent(parent);
        }
        go.GetComponent<CardObject>().card = this;
        return go;
    }
}

public static class CardSerializer
{
    public static void WriteCard(this NetworkWriter writer, Card card)
    {
        writer.WriteString(card.title);
    }

    public static Card ReadCard(this NetworkReader reader)
    {
        return ResourceContainer.Instance.getCardByTitle(reader.ReadString());
    }
}