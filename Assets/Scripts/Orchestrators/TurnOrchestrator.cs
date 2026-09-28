using UnityEngine;
using System;

public class TurnOrchestrator : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;

    public event Action<int> HUDdisplay;
    public event Action<int> ConfirmTeamDecisions;
    public event Action<int> UnitZoneDamage;
    public event Action<int> ZoneStage;

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
    }

    private void Orchestrate(int turn)
    {
        ConfirmTeamDecisions?.Invoke(turn);

        UnitZoneDamage?.Invoke(turn);

        ZoneStage?.Invoke(turn);

        HUDdisplay?.Invoke(turn);
    }
}
