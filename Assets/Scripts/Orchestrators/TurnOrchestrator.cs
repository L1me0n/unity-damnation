using UnityEngine;
using System;

public class TurnOrchestrator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private MatchManager matchManager;

    public event Action<int> ConfirmTeamDecisions;
    public event Action<int> ResolveEncounters;
    public event Action<int> UnitZoneDamage;
    public event Action<int> ZoneStage;
    public event Action<int> CheckOwners;
    public event Action<int> CountNodePoints;
    public event Action<int> HUDdisplay;
    public event Action<int> CheckEnd;

    private void OnEnable()
    {
        turnManager.OnTurnEnded += Orchestrate;
    }

    private void OnDisable()
    {
        turnManager.OnTurnEnded -= Orchestrate;
    }

    private void Awake()
    {
        if (turnManager == null)
        {
            turnManager = FindObjectOfType<TurnManager>();
        }

        if (matchManager == null)
        {
            matchManager = FindObjectOfType<MatchManager>();
        }
    }

    private void Orchestrate(int turn)
    {
        if (matchManager.MatchEnded)
        {
            return;
        }

        ConfirmTeamDecisions?.Invoke(turn);

        ResolveEncounters?.Invoke(turn);
        if (matchManager.IsEndable)
        {
            CheckOwners?.Invoke(turn);
            CountNodePoints?.Invoke(turn);
            HUDdisplay?.Invoke(turn); 
            CheckEnd?.Invoke(turn);
            return;
        }

        UnitZoneDamage?.Invoke(turn);
        if (matchManager.IsEndable)
        {
            CheckOwners?.Invoke(turn);
            CountNodePoints?.Invoke(turn);
            HUDdisplay?.Invoke(turn); 
            CheckEnd?.Invoke(turn);
            return;
        }

        ZoneStage?.Invoke(turn);

        CheckOwners?.Invoke(turn);

        CountNodePoints?.Invoke(turn);

        HUDdisplay?.Invoke(turn); 
    }
}
