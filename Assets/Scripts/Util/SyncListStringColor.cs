using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SyncListStringColor : SyncListString
{
    List<Color> colors;

    public SyncListStringColor() : base()
    {
        colors = new List<Color>();
    }

    public void Add(string text, Color color)
    {
        colors.Add(color);
        base.Add(text);
    }

    public void setColor(int index, Color color) 
    {
        colors[index] = color;
    }

    public Color getColor(int index)
    {
        return colors[index];
    }

    protected override void SerializeItem(NetworkWriter writer, string item)
    {
        base.SerializeItem(writer, item);
        int index = this.IndexOf(item);
        writer.WriteColor(colors[index]);
    }

    protected override string DeserializeItem(NetworkReader reader)
    {
        string item = base.DeserializeItem(reader);
        colors.Add(reader.ReadColor());
        return item;

    }
}
