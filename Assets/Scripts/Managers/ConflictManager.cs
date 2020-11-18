using System.Collections.Generic;
using UnityEngine;

public class ConflictManager
{

    //A class for organizing and matching players and drones for conflicts.

    public static readonly int TIED = -10;
    public static readonly int NOT_OVER = -99;

    Dictionary<Vector2Int, List<int>> tileConflicts; //master list of all tiles with players or drones and the IDs of all entities
    Queue<Vector2Int> conflictPositions; //the queue of the positions left to check
    int lootId;
    Vector2Int currentFightPos;
    TileFight currentTile;
    List<int> currentFight;
    List<int> leftFighterRoll;
    int leftFighterTotal;
    List<int> rightFighterRoll;
    int rightFighterTotal;

    public ConflictManager()
    {
        tileConflicts = new Dictionary<Vector2Int, List<int>>();
        conflictPositions = new Queue<Vector2Int>();
        currentFight = new List<int>();
        leftFighterRoll = new List<int>();
        rightFighterRoll = new List<int>();
        Clear();
    }

    public void Clear()
    {
        tileConflicts.Clear();
        conflictPositions.Clear();
        lootId = -1;
        currentTile = null;
        currentFight = null;
        leftFighterRoll.Clear();
        rightFighterRoll.Clear();
        leftFighterTotal = 0;
        rightFighterTotal = 0;

    }

    public void addPlayer(int id, Vector2Int pos, bool hasLoot = false)
    {
        if (!tileConflicts.ContainsKey(pos))
            tileConflicts.Add(pos, new List<int>());
        tileConflicts[pos].Add(id);
        if (hasLoot)
            lootId = id;
    }

    public void addDrone(int id, Vector2Int pos)
    {
        if (!tileConflicts.ContainsKey(pos))
            tileConflicts.Add(pos, new List<int>());
        tileConflicts[pos].Add(id);
    }

    public void prepareConflicts()
    {
        conflictPositions = new Queue<Vector2Int>(tileConflicts.Keys);
        nextTile();
    }

    public bool nextTile()
    {
        if (conflictPositions.Count <= 0)
            return false;
        currentFightPos = conflictPositions.Dequeue();
        currentTile = new TileFight(currentFightPos);

        foreach(int id in tileConflicts[currentFightPos])
        {
            if (id < 0)
                currentTile.addDrone(id);
            else
                currentTile.addPlayer(id, id == lootId);
        }
        return true;
    }

    public List<int> nextFight()
    {
        List<int> fight = currentTile.nextFight();
        if (fight == null)
        {
            if (nextTile())
                fight = currentTile.nextFight();
            else
                return null; //No more tiles left
        }
        currentFight = fight;
        leftFighterRoll = new List<int>();
        rightFighterRoll = new List<int>();
        leftFighterTotal = 0;
        rightFighterTotal = 0;
        return fight;
    }

    public List<int> getCurrentFight()
    {
        return currentFight;
    }

    public void fightRollLeft(List<int> roll)
    {
        leftFighterRoll = roll;
        foreach(int val in roll)
        {
            leftFighterTotal += val;
        }
    }

    public void fightRollRight(List<int> roll)
    {
        rightFighterRoll = roll;
        foreach(int val in roll)
        {
            rightFighterTotal += val;
        }
    }

    public List<int> getLeftRoll()
    {
        return leftFighterRoll;
    }

    public List<int> getRightRoll()
    {
        return rightFighterRoll;
    }

    public int getLeftTotal()
    {
        return leftFighterTotal;
    }

    public int getRightTotal()
    {
        return rightFighterTotal;
    }

    public int getWinnerId()
    {
        if (rightFighterRoll.Count <= 0 && leftFighterRoll.Count <= 0)
            return NOT_OVER;

        if (leftFighterTotal > rightFighterTotal)
            return currentFight[0];
        else if (rightFighterTotal > leftFighterTotal)
            return currentFight[1];
        else
            return TIED;
    }

    public int getLoserId()
    {
        if (rightFighterRoll.Count <= 0 && leftFighterRoll.Count <= 0)
            return NOT_OVER;

        if (leftFighterTotal > rightFighterTotal)
            return currentFight[1];
        else if (rightFighterTotal > leftFighterTotal)
            return currentFight[0];
        else
            return TIED;
    }

    public void fightResults()
    {
        if (leftFighterTotal > rightFighterTotal)
        {
            currentTile.fightOutcome(currentFight[0], currentFight[1]);
        }
        else if (rightFighterTotal > leftFighterTotal)
        {
            currentTile.fightOutcome(currentFight[1], currentFight[0]);
        }
        else
        {
            currentTile.fightOutcome(currentFight[0], currentFight[1], true);
        }
        currentFight = null;
        leftFighterRoll.Clear();
        rightFighterRoll.Clear();

    }
}

public class TileFight
{
    public Vector2Int position { get; private set; }
    List<int> players;
    List<int> drones;
    Queue<int> fighters;
    List<int> lost;
    Dictionary<int, List<int>> alreadyFought;
    int lootId;
    int carryOverPlayer;
    int rounds;

    public TileFight(Vector2Int pos)
    {
        position = pos;
        players = new List<int>();
        drones = new List<int>();
        fighters = new Queue<int>();
        lost = new List<int>();
        alreadyFought = new Dictionary<int, List<int>>();
        carryOverPlayer = -1;
        lootId = -1;
        rounds = 0;
    }

    public void addPlayer(int id, bool hasLoot = false)
    {
        players.Add(id);
        alreadyFought.Add(id, new List<int>());
        if (hasLoot)
            lootId = id;
    }

    public void addDrone(int id)
    {
        drones.Add(id);
    }

    public void fightOutcome(int winner, int loser, bool isTied = false)
    {
        if (winner >= 0)
            alreadyFought[winner].Add(loser);

        if (loser >= 0)
            alreadyFought[loser].Add(winner);

        if (isTied)
        {
            if (winner < 0)
                drones.Add(winner);
            else
                players.Add(winner);

            if (loser < 0)
                drones.Add(loser);
            else
                players.Add(loser);
            return;
        }

        if (winner < 0)
            drones.Add(winner);
        else
            players.Add(winner);
        lost.Add(loser);
        if (loser == lootId)
            lootId = -1;
    }

    public List<int> nextFight()
    {
        if (fighters.Count <= 1)
        {
            setupMatches();
        }
            
        if (fighters.Count > 1)
            return new List<int> { fighters.Dequeue(), fighters.Dequeue() };
        return null;
    }

    private void setupMatches()
    {
        
        

        List<int> randoPlayers = new List<int>();
        while(players.Count > 0)
        {
            int index = Random.Range(0, players.Count);
            randoPlayers.Add(players[index]);
            players.RemoveAt(index);
        }

        if (carryOverPlayer >= 0)
            randoPlayers.Insert(0, carryOverPlayer);

        players = randoPlayers;
        int playerCount = players.Count;
        // If CarryOver: they fight next
        // If there is a drone


        //match up the drones
        while(drones.Count > 0)
        {
            int dId = drones[0];
            drones.RemoveAt(0);
            if (lootId >= 0 && players.Contains(lootId) && !alreadyFought[lootId].Contains(dId))
            {
                players.Remove(lootId);
                fighters.Enqueue(dId);
                fighters.Enqueue(lootId);
            }
            else
            {
                foreach(int pId in players)
                {
                    if (!alreadyFought[pId].Contains(dId))
                    {
                        players.Remove(pId);
                        fighters.Enqueue(dId);
                        fighters.Enqueue(pId);
                        break;
                    }
                }
            }
            if (drones.Count > 0 && players.Count == 0)
            {
                break;
            }
        }

        while(players.Count > 1)
        {
            int p1 = players[0];
            players.RemoveAt(0);
            foreach(int p2 in players)
            {
                if (!alreadyFought[p1].Contains(p2))
                {
                    players.Remove(p2);
                    fighters.Enqueue(p1);
                    fighters.Enqueue(p2);
                    break;
                }
            }
        }

        //while (playerCount > 1 || (playerCount > 0 && drones.Count > 0))
        //{
        //    int id1;
        //    int id2;
        //    bool nextLoop = false;
        //    if (drones.Count > 0)
        //    {
        //        id1 = drones[0];
        //        drones.RemoveAt(0);
        //        if (lootId >= 0 && players.Contains(lootId) && !alreadyFought[lootId].Contains(id1))
        //        {
        //            id2 = lootId;
        //            players.Remove(lootId);
        //            fighters.Enqueue(id1);
        //            fighters.Enqueue(id2);
        //            nextLoop = true;
        //        }
        //        //else if (carryOverPlayer >= 0 && !alreadyFought[carryOverPlayer].Contains(id1))
        //        //{
        //        //    id2 = carryOverPlayer;
        //        //    carryOverPlayer = -1;
        //        //    drones.RemoveAt(0);
        //        //    fighters.Enqueue(id1);
        //        //    fighters.Enqueue(id2);
        //        //    nextLoop = true;
        //        //}
        //        else
        //        {
        //            for(int j = 0; j < players.Count; j++)
        //            {
        //                id2 = players[j];
        //                if (!alreadyFought[id2].Contains(id1))
        //                {
        //                    players.Remove(id2);
        //                    fighters.Enqueue(id1);
        //                    fighters.Enqueue(id2);
        //                    nextLoop = true;
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    if (nextLoop)
        //        continue;

        //    int i = 0;
        //    while (i < playerCount - 1)
        //    {
        //        id1 = players[i];
        //        id2 = players[i + 1];

        //        if (!alreadyFought[id1].Contains(id2))
        //        {

        //        }

        //    }
            

        //    if (carryOverPlayer >= 0)
        //    {
        //        id1 = carryOverPlayer;
        //        carryOverPlayer = -1;
        //    }
        //    else
        //    {
        //        id1 = players[Random.Range(0, players.Count)];
        //        players.Remove(id1);
        //    }
        //    id2 = players[Random.Range(0, players.Count)];
        //    players.Remove(id2);
        //    playerCount -= 2;
        //    fighters.Enqueue(id1);
        //    fighters.Enqueue(id2);
        //}
        if (players.Count == 1)
        {
            carryOverPlayer = players[0];
            players.Clear();
        }
        rounds += 1;
    }
}
