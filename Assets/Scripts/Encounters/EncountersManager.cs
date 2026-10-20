using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random; // TODO: remove this

public class EncountersManager : MonoBehaviour
{
    public class Encounter
    {
        public Dictionary<int, List<UnitManager>> attackingTeams = new Dictionary<int, List<UnitManager>>();
        public List<UnitManager> defendingTeam = new List<UnitManager>(); // the one already occupying the node
        public BuildingNode node;

        public void Init(UnitManager unit, BuildingNode buildingNode)
        {
            attackingTeams[unit.TeamID] = new List<UnitManager>();
            attackingTeams[unit.TeamID].Add(unit);

            foreach (UnitManager unitManager in buildingNode.OccupyingUnits)
            {
                defendingTeam.Add(unitManager);
            }

            node = buildingNode;
        }
        
        public void AddInfo(UnitManager unit)
        {
            if (!attackingTeams.ContainsKey(unit.TeamID))
            {
                attackingTeams[unit.TeamID] = new List<UnitManager>();
                attackingTeams[unit.TeamID].Add(unit);
            }
            else
            {
                attackingTeams[unit.TeamID].Add(unit);
            }
        }
    }

    [Header("References")]
    [SerializeField] private TurnOrchestrator turnOrchestrator;
    [SerializeField] private MatchManager matchManager;

    List<Encounter> encounters = new List<Encounter>();
    Dictionary<BuildingNode, Encounter> encounterNodes = new Dictionary<BuildingNode, Encounter>();

    private void OnEnable()
    {
        turnOrchestrator.ResolveEncounters += ResolveEncounters;
    }

    private void OnDisable()
    {
        turnOrchestrator.ResolveEncounters -= ResolveEncounters;
    }

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

    public void RegisterEncounter(UnitManager attackerUnit, BuildingNode node)
    {
        if (encounterNodes.ContainsKey(node))
        {
            Encounter encounter = encounterNodes[node];

            encounter.AddInfo(attackerUnit);
        }
        else
        {
            Encounter encounter = new Encounter();

            encounter.Init(
                attackerUnit,
                node
            );

            encounters.Add(encounter);
            encounterNodes[node] = encounter;
        }
    }

    public void ResolveEncounters(int turn)
    {
        foreach (Encounter encounter in encounters)
        {
            List<int> teamIds = new List<int>();
            teamIds.Add(encounter.defendingTeam[0].TeamID);
            foreach (int id in encounter.attackingTeams.Keys)
            {
                teamIds.Add(id);
            }

            int winner = Random.Range(0, teamIds.Count);

            if (winner == 0)
            {
                encounter.node.SetOwnerTeam(teamIds[0]);
                matchManager.MoveWinner(encounter.node, teamIds[winner], encounter.defendingTeam, encounter.attackingTeams, true);
                
            }
            else
            {
                encounter.node.SetOwnerTeam(teamIds[winner]);;
                matchManager.MoveWinner(encounter.node, teamIds[winner], encounter.defendingTeam, encounter.attackingTeams, false);
            }

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
