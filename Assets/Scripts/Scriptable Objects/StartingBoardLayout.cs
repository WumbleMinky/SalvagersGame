using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Layouts/Starting Board Layout")]
public class StartingBoardLayout : ScriptableObject
{

    [System.Serializable]
    public struct placement
    {
        public TileLayout layout;
        public int rotations;

        public placement(TileLayout l, int r=0)
        {
            layout = l;
            rotations = r;
        }
    }

    public string displayName;

    public placement forwardLeft;
    public placement forward;
    public placement forwardRight;
    public placement left;
    public placement right;
    public placement backLeft;
    public placement back;
    public placement backRight;

    public TileLayout GetLayout(Vector2Int dir)
    {
        return getPlacement(dir).layout;
    }

    public int getRotations(Vector2Int dir)
    {
        return getPlacement(dir).rotations;
    }

    public placement getPlacement(Vector2Int dir)
    {
        if (dir.x == 1)
        {
            if (dir.y == 0)
                return right;
            else if (dir.y == 1)
                return forwardRight;
            else
                return backRight;
        }
        else if (dir.x == -1)
        {
            if (dir.y == 0)
                return left;
            else if (dir.y == 1)
                return forwardLeft;
            else
                return backLeft;
        }
        else
        {
            if (dir.y == 1)
                return forward;
            else
                return back;
        }
    }
}
