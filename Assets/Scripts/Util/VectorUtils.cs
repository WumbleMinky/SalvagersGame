using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorUtils
{

    public static readonly Vector2Int[] cardinalDirections = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

    public static Vector2Int rotate90CW(Vector2Int vec)
    {
        return new Vector2Int(vec.y, -vec.x);
    }

    public static Vector2Int rotate90counterCW(Vector2Int vec)
    {
        return new Vector2Int(-vec.y, vec.x);
    }

    public static Vector2Int rotate180(Vector2Int vec)
    {
        return new Vector2Int(-vec.x, -vec.y);
    }
}
