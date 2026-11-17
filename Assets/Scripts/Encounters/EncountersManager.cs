using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class EncountersManager : MonoBehaviour
{
    public class Encounter
    {
        public Dictionary<int, List<UnitManager>> teams = new Dictionary<int, List<UnitManager>>();
        public BuildingNode node;

        public void Init(UnitManager unit, BuildingNode buildingNode)
        {
            teams[unit.TeamID] = new List<UnitManager>();
            teams[unit.TeamID].Add(unit);

            node = buildingNode;
        }
        
        public void AddInfo(UnitManager unit)
        {
            if (!teams.ContainsKey(unit.TeamID))
            {
                teams[unit.TeamID] = new List<UnitManager>();
                teams[unit.TeamID].Add(unit);
            }
            else
            {
                teams[unit.TeamID].Add(unit);
            }
        }
    }

    [Header("References")]
    [SerializeField] private TurnOrchestrator turnOrchestrator;
    [SerializeField] private MatchManager matchManager;

    Queue<Encounter> encounters = new Queue<Encounter>();
    Dictionary<BuildingNode, Encounter> encounterNodes = new Dictionary<BuildingNode, Encounter>();

    public bool IsQueueEmpty => encounters.Count == 0;

    public event Action<int> OnEncountersResolved;

    private void Awake()
    {
        if (turnOrchestrator == null)
        {
            turnOrchestrator = FindObjectOfType<TurnOrchestrator>();
        }

        if (matchManager == null)
        {
            matchManager = FindObjectOfType<MatchManager>();
        }
    }

    public void RegisterEncounter(UnitManager unit, BuildingNode node)
    {
        if (encounterNodes.ContainsKey(node))
        {
            Encounter encounter = encounterNodes[node];

            encounter.AddInfo(unit);
        }
        else
        {
            Encounter encounter = new Encounter();

            encounter.Init(
                unit,
                node
            );

            encounters.Enqueue(encounter);
            encounterNodes[node] = encounter;
        }
    }

    // TODO 
    // Dequeue encounters and either rapidly resolve ai to ai encounters
    // Or set up player to ai encounter scene
    // Make an event of EncountersResolved, when the queue is emptied. 
    // This will allow Turn Orchestrator to move further the turn resolution
    // Or end the turn after hot drops resolved 
    public void ResolveEncounters(int turn)
    {
        while (encounters.Count > 0)
        {
            Encounter currentEncounter = encounters.Dequeue();
            encounterNodes.Remove(currentEncounter.node);
            /* if (!currentEncounter.teams.ContainsKey(0))
            { */
                List<int> teamIds = new List<int>();
                foreach (int id in currentEncounter.teams.Keys)
                {
                    teamIds.Add(id);
                }

                int roll = Random.Range(0, teamIds.Count);

                int winnerTeam = teamIds[roll];

                List<int> loserTeams = new List<int>();

                foreach (int team in teamIds)
                {
                    if (team == winnerTeam)
                    {
                        continue;
                    }

                    loserTeams.Add(team);
                }

                matchManager.MoveWinner(currentEncounter.node, winnerTeam, loserTeams);
            /* } */
        }

        OnEncountersResolved?.Invoke(turn);
    }
}
