using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random; // TODO: remove this

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

    List<Encounter> encounters = new List<Encounter>();
    Dictionary<BuildingNode, Encounter> encounterNodes = new Dictionary<BuildingNode, Encounter>();

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

            encounters.Add(encounter);
            encounterNodes[node] = encounter;
        }
    }

    public void ResolveEncounters()
    {
        foreach (Encounter encounter in encounters)
        {
            List<int> teamIds = new List<int>();
            foreach (int id in encounter.teams.Keys)
            {
                teamIds.Add(id);
            }

            int winner = Random.Range(0, teamIds.Count);

            List<UnitManager> losers = new List<UnitManager>();
            List<UnitManager> winners = new List<UnitManager>();
            List<UnitManager> participatedTeams = new List<UnitManager>();

            foreach (var pair in encounter.teams)
            {
                foreach (UnitManager unit in pair.Value)
                {
                    participatedTeams.Add(unit);
                }
            }

            foreach (UnitManager unit in participatedTeams)
            {
                if (unit.TeamID == teamIds[winner])
                {
                    winners.Add(unit);
                }
                else
                {
                    losers.Add(unit);
                }
            }

            encounter.node.SetOwnerTeam(teamIds[winner]);;
            matchManager.MoveWinner(encounter.node, winners, losers);

            if (matchManager.IsEndable)
            {
                encounters.Clear();
                encounterNodes.Clear();
                return;
            }
        }

        encounters.Clear();
        encounterNodes.Clear();
    }
}
