using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Logic/Game Phase")]
public class GamePhase : ScriptableObject
{
    public string phaseName;
    public GamePhase nextPhase;

    public delegate void StartPhaseAction();
    public event StartPhaseAction startPhase;

    public delegate void EndPhaseAction();
    public event EndPhaseAction endPhase;


    public GamePhase gotoNextPhase()
    {
        if (endPhase != null)
            endPhase();

        if (nextPhase.startPhase != null)
            nextPhase.startPhase();

        return nextPhase;
    }
}
