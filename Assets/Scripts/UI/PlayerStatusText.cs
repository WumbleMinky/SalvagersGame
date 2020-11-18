using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusText : MonoBehaviour
{
    public Image imageObj;
    public Text textObj;

    public string text 
    {
        get { return textObj.text;  }
        set { textObj.text = value; }
    }

    public Color color
    {
        get { return imageObj.color;  }
        set { setColor(value);  }
    }

    private void setColor(Color c)
    {
        Color newC = ResourceContainer.Instance.getTextColor(c);
        textObj.color = newC;
        imageObj.color = c;
    }
}
