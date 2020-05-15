using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Tilemaps;

public class GameBoard : NetworkBehaviour
{
    public GameObject compass;
    public float tileSize = 10f;
    public GameObject tilePrefab;
    public GameObject ghostTilePrefab;
    public TileLayout startingLayout;
    public Dictionary<Vector2Int, GameTile> grid;
    Dictionary<GameObject, Vector2Int> ghostTiles;
    
    [SerializeField]    private GameTile unconfirmedTile;

    [SyncVar]
    public bool editingTile = false;

    void Start()
    {
        grid = new Dictionary<Vector2Int, GameTile>();
        ghostTiles = new Dictionary<GameObject, Vector2Int>();
    }

    #region Board Creation

    private void setUnconfirmedTile(GameTile tile)
    {
        unconfirmedTile = tile;
        if (tile == null)
            editingTile = false;
        else
            editingTile = true;
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
        setUnconfirmedTile(tile);
        tile.invalid = !isTileValid(tile);
    }

    [Server]
    private GameTile spawnTile(Vector2Int gridPos, TileLayout layout)
    {
        Vector3 pos = new Vector3(gridPos.x * tileSize, 0, gridPos.y * tileSize);
        GameObject tileGO = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
        NetworkServer.Spawn(tileGO);
        GameTile tile = tileGO.GetComponent<GameTile>();
        tile.RpcUpdateParent(gameObject);
        tile.layout = layout;
        tile.gridPos = gridPos;
        return tile;
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
            addTile(pos, newLayout);
    }

    [Server]
    public void addGhostTiles(Vector2Int pos)
    {
        foreach(Vector2Int dir in new Vector2Int []{ Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left })
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
        grid.Add(unconfirmedTile.gridPos, unconfirmedTile);
        addGhostTiles(unconfirmedTile.gridPos);
        setUnconfirmedTile(null);
        return true;
    }

    public bool isTileValid(GameTile tile)
    {
        bool atLeastOneDoor = false;
        foreach (Vector2Int dir in tile.tileSides.Keys)
        {
            GameTile.Side side = tile.tileSides[dir];
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

    public GameObject getOpenTokenPosition(Vector2Int pos)
    {
        GameTile tile = grid[pos];
        int totalPosition = tile.tokenPositions.Count;
        int index = Random.Range(0, totalPosition);
        GameObject tokenPosGO = tile.tokenPositions[index];
        int attempts = 1;

        while (tokenPosGO.transform.childCount > 0)
        {
            tokenPosGO = tile.tokenPositions[(index + 1) % totalPosition];
            attempts++;
            if (attempts > totalPosition)
            {
                tokenPosGO = null;
                break;
            }
        }
        return tokenPosGO;
    }
}
