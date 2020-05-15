using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Layouts/Tile Layout")]
public class TileLayout : ScriptableObject
{
    public new string name;
    public Transform prefab;
    public int total;
    public Vector2Int[] doorDirections;

    public bool matches(TileLayout other)
    {
        return other.name.Equals(name) && other.prefab.Equals(other.prefab);
    }

    public GameObject createPrefab(Transform parent = null)
    {
        return Instantiate(prefab, parent).gameObject;
    }
}

public static class TileLayoutSerializer
{
    public static void WriteLayout(this NetworkWriter writer, TileLayout tile)
    {
        if (tile != null)
            writer.WriteString(tile.name);
        else
            writer.WriteString("null");
    }

    public static TileLayout ReadLayout(this NetworkReader reader)
    {
        string n = reader.ReadString();
        if (n.Equals("null"))
            return null;
        else
            return ResourceContainer.Instance.getTileLayoutByName(n);
    }
}
