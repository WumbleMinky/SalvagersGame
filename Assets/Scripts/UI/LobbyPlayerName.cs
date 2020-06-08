using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerName : MonoBehaviour
{
    public Image colorIndicator;
    public Text textComponent;

    public string text { get { return textComponent.text; } set { textComponent.text = value; } }
    public Color color { get {return colorIndicator.color; } set {colorIndicator.color = value; } }

}
