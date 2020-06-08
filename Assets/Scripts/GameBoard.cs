using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class GameBoard : NetworkBehaviour
{
    public GameObject compass;
    public float tileSize = 10f;
    public GameObject tilePrefab;
    public GameObject ghostTilePrefab;
    public TileLayout startingLayout;
    public Dictionary<Vector2Int, GameTile> grid = new Dictionary<Vector2Int, GameTile>();
    public List<Vector2Int> spaceCoords = new List<Vector2Int>();
    Dictionary<GameObject, Vector2Int> ghostTiles = new Dictionary<GameObject, Vector2Int>();
    public List<Token> tokens = new List<Token>();
    public Token LootToken;

    [SerializeField] public GameTile unconfirmedTile;

    [SyncVar]
    public bool editingTile = false;

    private Dictionary<Vector2Int, Vector2Int> directionMap = new Dictionary<Vector2Int, Vector2Int>();

    public List<Vector2Int> docks = new List<Vector2Int>();

    void Start()
    {
        //grid = new Dictionary<Vector2Int, GameTile>();
        //ghostTiles = new Dictionary<GameObject, Vector2Int>();
        directionMap.Add(Vector2Int.up, Vector2Int.up); //Forward
        directionMap.Add(Vector2Int.right, Vector2Int.right); //Right
        directionMap.Add(Vector2Int.down, Vector2Int.down); //Back
        directionMap.Add(Vector2Int.left, Vector2Int.left); //Left

    }

    #region Board Creation

    public void setUnconfirmedTile(GameTile tile)
    {
        unconfirmedTile = tile;
        if (tile == null)
        {
            editingTile = false;
        }
        else
        {
            editingTile = true;
        }
    }

    [TargetRpc]
    private void TargetSetTarget(GameObject obj)
    {
        UIReference.Instance.ObjectSelector.selectObject(obj);
    }

    [ClientRpc]
    private void RpcUnsetTarget()
    {
        UIReference.Instance.ObjectSelector.deselect();
    }

    [Server]
    public void addStartingTiles(StartingBoardLayout startingLayouts)
    {
        TileLayout layout;
        foreach(Vector2Int vert in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.zero})
        {
            foreach(Vector2Int horiz in new Vector2Int[] { Vector2Int.right, Vector2Int.left, Vector2Int.zero})
            {
                if (vert + horiz == Vector2Int.zero)
                    layout = startingLayout;
                else
                    layout = startingLayouts.GetLayout(vert + horiz);
                GameTile tile = spawnTile(vert + horiz, layout);
                grid.Add(vert + horiz, tile);
                for(int i = 0; i < startingLayouts.getRotations(vert + horiz); i++)
                {
                    tile.rotate90CW(false);
                }
                tile.invalid = false;
            }
        }

        foreach(Vector2Int pos in grid.Keys)
        {
            addGhostTiles(pos);
        }
        setUnconfirmedTile(null);
    }

    [Server]
    public void addTile(Vector2Int gridPos, TileLayout layout)
    {        
        GameTile tile = spawnTile(gridPos, layout);
        grid.Add(gridPos, tile);
        tile.invalid = !isTileValid(tile);
    }

    [Server]
    private GameTile spawnTile(Vector2Int gridPos, TileLayout layout)
    {
        Vector3 pos = new Vector3(gridPos.x * tileSize, 0, gridPos.y * tileSize);
        GameObject tileGO = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
        GameTile tile = tileGO.GetComponent<GameTile>();
        tile.layout = layout;
        tile.gridPos = gridPos;
        setUnconfirmedTile(tile);
        NetworkServer.Spawn(tileGO);
        return tile;
    }

    [Client]
    public void clientAddTile(GameTile tile)
    {
        grid.Add(tile.gridPos, tile);
        setUnconfirmedTile(tile);
    }

    [Server]
    public void removeTile(Vector2Int gridPos)
    {
        if (grid.ContainsKey(gridPos))
            grid.Remove(gridPos);
        NetworkServer.UnSpawn(unconfirmedTile.gameObject);
        Destroy(unconfirmedTile.gameObject);
        setUnconfirmedTile(null);
    }

    [Client]
    public void clientRemoveTile(Vector2Int gridPos)
    {
        if (grid.ContainsKey(gridPos))
            grid.Remove(gridPos);
        setUnconfirmedTile(null);
    }

    [Server]
    public void cancelUnconfirmedTile()
    {
        if (unconfirmedTile != null)
        {
            Vector2Int pos = unconfirmedTile.gridPos;
            removeTile(pos);
            createGhostTile(pos);
        }
    }

    [Server]
    public void placeTileOnGhost(GameObject ghostTile, TileLayout newLayout)
    {
        if (unconfirmedTile != null)
        {
            Vector2Int oldpos = unconfirmedTile.gridPos;
            removeTile(unconfirmedTile.gridPos);
            createGhostTile(oldpos);
        }
        Vector2Int pos = removeGhostTile(ghostTile);
        if (pos != null)
        {
            addTile(pos, newLayout);
        }
            
    }

    [Server]
    public void addGhostTiles(Vector2Int pos)
    {
        foreach(Vector2Int dir in VectorUtils.cardinalDirections)
        {
            if (grid[pos].isSideADoor(dir))
                createGhostTile(pos + dir);
        }
    }

    [Server]
    public void createGhostTile(Vector2Int pos)
    {
        if (!grid.ContainsKey(pos) && !ghostTiles.ContainsValue(pos))
        {
            GameObject go = Instantiate(ghostTilePrefab, new Vector3(pos.x * tileSize, 0, pos.y * tileSize), Quaternion.identity, transform);
            NetworkServer.Spawn(go);
            ghostTiles.Add(go, pos);
        }
    }

    [Server]
    public Vector2Int removeGhostTile(GameObject tile)
    {
        Vector2Int tilePos;
        if (ghostTiles.TryGetValue(tile, out tilePos))
        {
            ghostTiles.Remove(tile);
            NetworkServer.UnSpawn(tile);
            Destroy(tile);
        }
        return tilePos;
    }

    [Server]
    public void removeAllGhosts()
    {
        foreach(GameObject go in ghostTiles.Keys)
        {
            NetworkServer.UnSpawn(go);
            Destroy(go);
        }
        ghostTiles.Clear();
    }

    [Server]
    public void rotateUnconfirmedTile()
    {
        if (unconfirmedTile != null)
        {
            unconfirmedTile.rotate90CW();
            unconfirmedTile.invalid = !isTileValid(unconfirmedTile);
        }
    }

    [Server]
    public bool confirmTile()
    {
        if (unconfirmedTile.invalid)
            return false;
        //grid.Add(unconfirmedTile.gridPos, unconfirmedTile);
        addGhostTiles(unconfirmedTile.gridPos);
        setUnconfirmedTile(null);
        return true;
    }

    public bool isTileValid(GameTile tile)
    {
        bool atLeastOneDoor = false;
        foreach (Vector2Int dir in tile.tileSides.Keys)
        {
            TileWall side = tile.tileSides[dir];
            Vector2Int neighborPos = tile.gridPos + dir;
            if (grid.ContainsKey(neighborPos))
            {
                if (side.isDoor())
                {
                    if (grid[neighborPos].tileSides[-dir].isDoor())
                        atLeastOneDoor = true;
                    else
                        return false;
                }
                else
                {
                    if (grid[neighborPos].tileSides[-dir].isDoor())
                        return false;
                }
            }
        }

        return atLeastOneDoor;
    }

    #endregion

    public GameObject getOpenTokenPosition(Vector2Int pos, bool randomPosition = false)
    {
        if (!hasTileAt(pos))
            return null;
        GameTile tile = grid[pos];
        int totalPosition = tile.tokenPositions.Count;
        int index;
        if (randomPosition)
            index = UnityEngine.Random.Range(0, totalPosition);
        else
            index = 0;
        GameObject tokenPosGO = tile.tokenPositions[index];
        int attempts = 1;
        bool validPos;

        while (true)
        {
            if (attempts > totalPosition)
            {
                tokenPosGO = null;
                break;
            }
            validPos = true;
            foreach (Token token in tokens)
            {
                if (token.tileTokenPos == tokenPosGO)
                {
                    validPos = false;
                    break;
                }
            }
            if (validPos)
                break;
            tokenPosGO = tile.tokenPositions[(index + 1) % totalPosition];
            attempts++;
        }

        return tokenPosGO;
    }

    public void addToken(Token token, bool isLoot = false)
    {
        tokens.Add(token);
        if (isLoot)
            LootToken = token;
    }

    public GameTile getTileAt(Vector2Int pos)
    {
        if (!hasTileAt(pos))
            return null;
        return grid[pos];
    }

    public Vector2Int getBoardDirection(Vector2Int dir)
    {
        return directionMap[dir];
    }

    public void rotateCompass90CW()
    {
        rotateCompass(90);
    }

    public void rotateCompass90CounterCW()
    {
        rotateCompass(-90);
    }

    public void rotateCompass180()
    {
        rotateCompass(180);
    }

    private void rotateCompass(int angle)
    {
        //TODO: rotate the compass object

        foreach (Vector2Int key in directionMap.Keys)
        {
            if (angle == 180)
                directionMap[key] = VectorUtils.rotate180(directionMap[key]);
            else if (angle == 90)
                directionMap[key] = VectorUtils.rotate90CW(directionMap[key]);
            else if (angle == -90)
                directionMap[key] = VectorUtils.rotate90counterCW(directionMap[key]);
        }
    }

    public bool hasTileAt(Vector2Int pos)
    {
        return grid.ContainsKey(pos);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            
        }
    }

    public void setCheckTileMouseDown(bool enable)
    {
        foreach(Vector2Int tilePos in grid.Keys)
        {
            if (distanceToTile(tilePos, LootToken.boardPosition) < 4)
                continue;
            GameTile gt = grid[tilePos];
            if (enable)
                gt.RpcEnableWallMouseDown();
            else
                gt.RpcDisableWallMouseDown();
        }

    }

    public int distanceToTile(Vector2Int fromCoords, Vector2Int toCoords)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> visited = new Dictionary<Vector2Int, int>();

        int dist = 0;
        visited.Add(fromCoords, dist);
        q.Enqueue(fromCoords);
        while(q.Count > 0)
        {
            Vector2Int current = q.Dequeue();
            dist = visited[current];
            if (current.Equals(toCoords))
            {
                return dist;
            }

            foreach(Vector2Int dir in VectorUtils.cardinalDirections)
            {
                if (grid.ContainsKey(current) && grid[current].isSideADoor(dir) && !visited.ContainsKey(current+dir))
                {
                    visited.Add(current + dir, dist+1);
                    q.Enqueue(current + dir);
                }
            }
        }
        return -1;
    }

    [ClientRpc]
    public void RpcColorExteriorWalls(Color color)
    {
        foreach(Vector2Int tilePos in grid.Keys)
        {
            GameTile gt = grid[tilePos];
            foreach(TileWall tw in gt.tileSides.Values)
            {
                if (tw.isExterior && distanceToTile(LootToken.boardPosition, tilePos) >= 4)
                {
                    tw.changeColor(color);
                }
            }
        }
    }

    [ClientRpc]
    public void RpcResetExteriorWallsColor()
    {
        foreach (GameTile gt in grid.Values)
        {
            foreach (TileWall tw in gt.tileSides.Values)
            {
                if (tw.isExterior)
                {
                    tw.resetColor();
                }
            }
        }
    }

    public bool tryAddDock(Vector2Int tilePos, Vector2Int wallDir)
    {
        GameTile tile;
        if (grid.TryGetValue(tilePos, out tile))
        {
            if (distanceToTile(tilePos, LootToken.boardPosition) >= 4 && !tile.dock.activeSelf)
            {
                docks.Add(tilePos);
                tile.setDock(wallDir);
                return true;
            }
        }
        return false;
    }

    [ClientRpc]
    public void RpcAddDock(Vector2Int tilePos, Vector2Int wallDir)
    {
        grid[tilePos].setDock(wallDir);
    }

    #region calculating Space Tiles & Exterior Walls

    [Server]
    public void findSpaceTiles()
    {
        List<Vector2Int> toCheck = new List<Vector2Int>();
        Dictionary<Vector2Int, bool> alreadyChecked = new Dictionary<Vector2Int, bool>();
        List<Vector2Int> recursiveChain = new List<Vector2Int>();
        Vector2Int minPoint = Vector2Int.zero;
        Vector2Int maxPoint = Vector2Int.zero;

        Vector2Int up = Vector2Int.up;
        Vector2Int down = Vector2Int.down;
        Vector2Int right = Vector2Int.right;
        Vector2Int left = Vector2Int.left;

        foreach (Vector2Int pos in grid.Keys)
        {
            maxPoint.x = Math.Max(pos.x, maxPoint.x);
            maxPoint.y = Math.Max(pos.y, maxPoint.y);
            minPoint.x = Math.Min(pos.x, minPoint.x);
            minPoint.y = Math.Min(pos.y, minPoint.y);

            if (!toCheck.Contains(pos + up))
                toCheck.Add(pos + up);
            if (!toCheck.Contains(pos + right))
                toCheck.Add(pos + right);
            if (!toCheck.Contains(pos + down))
                toCheck.Add(pos + down);
            if (!toCheck.Contains(pos + left))
                toCheck.Add(pos + left);
        }

        foreach(Vector2Int pos in toCheck)
        {
            if (isExternalSpaceTile(pos, minPoint, maxPoint, ref alreadyChecked, ref recursiveChain))
            {
                foreach(Vector2Int dir in new Vector2Int[] { up, right, down, left })
                {
                    if (grid.ContainsKey(pos + dir))
                    {
                        grid[pos + dir].RpcSetWallAsExterior(-dir, true);
                    }
                }
            }
            if (!grid.ContainsKey(pos))
            {
                spaceCoords.Add(pos);
            }
        }
    }

    private bool isExternalSpaceTile(Vector2Int toCheck, Vector2Int minPoint, Vector2Int maxPoint, ref Dictionary<Vector2Int, bool> alreadyChecked, ref List<Vector2Int> recursiveChain)
    {
        bool isSpace = false;
        if (grid.ContainsKey(toCheck))
        {
            return false;
        }
        if (alreadyChecked.ContainsKey(toCheck))
            return alreadyChecked[toCheck];

        foreach (Vector2Int dir in new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left})
        {
            if (alreadyChecked.ContainsKey(toCheck + dir))
                return alreadyChecked[toCheck + dir];

            if (recursiveChain.Contains(toCheck + dir))
                continue; //This direction was already checked and is waiting for results

            if (!hasTileInDirection(toCheck, dir, minPoint, maxPoint))
            {
                alreadyChecked.Add(toCheck, true);  //No tiles found in the direction, which means it is a space tile
                return true;
            }
            else if (!grid.ContainsKey(toCheck + dir)) // The adjacent Tile is an empty space
            {
                if (!recursiveChain.Contains(toCheck))
                    recursiveChain.Add(toCheck);
                isSpace = isExternalSpaceTile(toCheck + dir, minPoint, maxPoint, ref alreadyChecked, ref recursiveChain);
            }
            if (isSpace)
                break;
        }

        //We have returned to the first link in the recursive chain and there are no more options to check
        // Update all the links in the chain with the results of the check.
        if (recursiveChain.Count > 0 && recursiveChain[0] == toCheck)
        {
            foreach(Vector2Int pos in recursiveChain)
            {
                alreadyChecked.Add(pos, isSpace);
            }
            recursiveChain.Clear();
        }
        return isSpace;
    }

    private bool hasTileInDirection(Vector2Int pos, Vector2Int dir, Vector2Int min, Vector2Int max)
    {
        Vector2Int check = pos + dir;
        if (grid.ContainsKey(check))
            return true;
        if (check.x < min.x || check.y < min.y || check.x > max.x || check.y > max.y)
            return false;
        return hasTileInDirection(check, dir, min, max);
    }

    #endregion
}
