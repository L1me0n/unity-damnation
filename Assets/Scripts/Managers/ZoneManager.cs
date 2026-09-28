using UnityEngine;
using System.Collections.Generic;

public class ZoneManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnOrchestrator turnOrchestrator;
    [SerializeField] private ZoneBuilder zoneBuilder;

    [Header("Settings")]
    [SerializeField] private List<int> stageTurns = new List<int> {9, 16, 22, 28};
    [SerializeField] private List<int> shrinkingTurns = new List<int> {5, 12, 18, 24, 30};

    private int stageIterator = 0;
    private int shrinkingIterator = 0;
    private int currentTurn = 0;
    private bool isShrinking = false;

    public int ZoneDamage => stageIterator + 1;
    public bool IsShrinking => isShrinking;
    public float ShrinkingFill => stageIterator == stageTurns.Count 
        ? (float)(currentTurn - shrinkingTurns[shrinkingIterator - 1] + 1) 
        / (float)(34 - shrinkingTurns[shrinkingIterator - 1]) 
        : (float)(currentTurn - shrinkingTurns[shrinkingIterator - 1] + 1) 
        / (float)(stageTurns[stageIterator] - shrinkingTurns[shrinkingIterator - 1]);

    public int CurrentStage => currentTurn == 0 ? 0 : stageIterator + 1;

    public int StageCounter => shrinkingIterator == shrinkingTurns.Count 
        || currentTurn == 0
        ? 0 : shrinkingTurns[shrinkingIterator] - currentTurn;

    public int ShrinkingCounter => stageIterator == stageTurns.Count 
        ? Mathf.Max(34 - currentTurn, 0) : stageTurns[stageIterator] - currentTurn;


    private void OnEnable()
    {
        turnOrchestrator.ZoneStage += HandleTurnEnded;
    }

    private void OnDisable()
    {
        turnOrchestrator.ZoneStage -= HandleTurnEnded;
    }

    private void Awake()
    {
        if (turnOrchestrator == null)
        {
            turnOrchestrator = FindObjectOfType<TurnOrchestrator>();
        }

        if (zoneBuilder == null)
        {
            zoneBuilder = FindObjectOfType<ZoneBuilder>();
        }
    }

    private void HandleTurnEnded(int turn)
    {
        currentTurn = turn;

        if (turn == 1)
        {
            zoneBuilder.TransitionStage();
            isShrinking = false;
            MessageMenu.Instance.DisplayMessage($"Stage {CurrentStage} is starting! Zone shrink in {StageCounter}");
        }
        
        if (!(stageIterator == stageTurns.Count))
        {
            if (turn == stageTurns[stageIterator])
            {
                zoneBuilder.TransitionStage();
                isShrinking = false;
                stageIterator++;
                MessageMenu.Instance.DisplayMessage($"Stage {CurrentStage} is starting! Zone shrink in {StageCounter}");
            }
        }

        if (zoneBuilder.IsShrinking)
        {
            zoneBuilder.TransitionShrinking();
            isShrinking = true;
            return;
        }

        if (!(shrinkingIterator == shrinkingTurns.Count))
        {
            if (turn == shrinkingTurns[shrinkingIterator])
            {
                zoneBuilder.TransitionShrinking();
                isShrinking = true;
                shrinkingIterator++;
                MessageMenu.Instance.DisplayMessage($"Zone is shrinking!");
            }
        } 
    }
}
