using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class AITeam : MonoBehaviour
{
    [System.Serializable]
    public class EnemyPositionObservation
    {
        public bool isSuperseded = false;
        public int teamId;
        public int nodeId;
        public int count;
        public int turn;
        public KnowledgeSource source;  
    }

    [System.Serializable]
    public class ClaimedTerritoryObservation
    {
        public bool isSuperseded = false;
        public int teamId;
        public int nodeId;
        public int turn;
        public KnowledgeSource source;
    }

    [System.Serializable]
    public class UnitSnapshotObservation
    {
        public int teamId;
        public int nodeId;
        public int unitId;
        public int resistanceSnapshot;
        public int strengthSnapshot;
        public int intelligenceSnapshot;
        public int agilitySnapshot;
        public int turn;
        public KnowledgeSource source;
    }

    [Header("References")]
    [SerializeField] private BattlefieldGenerator battlefieldGenerator;
    [SerializeField] private DeploymentSelection deploymentSelection;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Transform unitsPool;
    [SerializeField] private MatchManager matchManager;
    [SerializeField] private TurnOrchestrator turnOrchestrator;

    [Header("Knowledge")]
    [SerializeField] private List<BuildingNode> claimedTerritory = new List<BuildingNode>();
    [SerializeField] private List<EnemyPositionObservation> positionObservations = new List<EnemyPositionObservation>();
    [SerializeField] private List<ClaimedTerritoryObservation> territoryObservations = new List<ClaimedTerritoryObservation>();
    [SerializeField] private List<UnitSnapshotObservation> snapshotObservations = new List<UnitSnapshotObservation>();
    // IsSafe from NodeSafety
    // IsShrinking from ZoneManager
    // StageCounter from ZoneManager to get how many turns till shrinking starts
    // ZoneDamage from ZoneManager
    // GetTeamNodePoints from MatchManager for the team's node points

    [Header("Settings")]
    [SerializeField] private int unitsPerTeam = 6;
    [SerializeField] private List<UnitManager> units = new List<UnitManager>();

    private int teamId;

    public int TeamId => teamId;
    public IReadOnlyList<UnitManager> Units => units;

    private void OnEnable()
    {
        deploymentSelection.OnRuleApplied += TeamDeployment;
        turnOrchestrator.ShowDeploymentPositions += GetDeploymentPositions;
    }

    private void OnDisable()
    {
        deploymentSelection.OnRuleApplied -= TeamDeployment;
        turnOrchestrator.ShowDeploymentPositions -= GetDeploymentPositions;
    }

    private void Awake()
    {
        battlefieldGenerator = FindObjectOfType<BattlefieldGenerator>();
        deploymentSelection = FindObjectOfType<DeploymentSelection>();
        unitsPool = GameObject.Find("UnitsPool").transform;
        matchManager = FindObjectOfType<MatchManager>();
        turnOrchestrator = FindObjectOfType<TurnOrchestrator>();
    }

    public void GenerateTeam(int id)
    {
        teamId = id;

        GenerateUnits();
    }

    private void GenerateUnits()
    {
        for (int i = 0; i < unitsPerTeam; i++)
        {
            GameObject unitObject = Instantiate(unitPrefab, unitsPool.position, Quaternion.identity);
            UnitManager unit = unitObject.GetComponent<UnitManager>();
            
            units.Add(unit);

            matchManager.RegisterUnit(unit, teamId);
        }
    }

    private void TeamDeployment(List<BuildingNode> blockedNodes)
    {
        BuildingNode node = RollNodes(blockedNodes);

        ConfirmDeployment(node);
    }

    private BuildingNode RollNodes(List<BuildingNode> blockedNodes)
    {
        List<BuildingNode> remainingNodes = new List<BuildingNode>();

        foreach (BuildingNode node in battlefieldGenerator.Nodes.Keys)
        {
            if (blockedNodes.Contains(node))
            {
                continue;
            }

            remainingNodes.Add(node);
        }

        int generatedIdx = Random.Range(0, remainingNodes.Count);

        return remainingNodes[generatedIdx];
    }

    private void ConfirmDeployment(BuildingNode node)
    {
        matchManager.RecordDeploymentPositions(node, teamId);
        foreach (UnitManager unit in units)
        {
            matchManager.MoveAIUnit(unit, this, node);
        }
    }

    private void GetDeploymentPositions(int turn)
    {
        for (int i = 0; i < matchManager.DeploymentPositions.Count; i++)
        {
            if (i == teamId)
            {
                continue;
            }

            BuildingNode node = matchManager.DeploymentPositions[i];
            if (node != null)
            {
                EnemyPositionObservation newObservation = new EnemyPositionObservation
                {
                    teamId = i,
                    nodeId = node.NodeID,
                    count = 6,
                    turn = turn,
                    source = KnowledgeSource.Deployment
                };

                positionObservations.Add(newObservation);
            }
        }
    }

    public void AddClaimedTerritory(BuildingNode node)
    {
        if (!claimedTerritory.Contains(node))
        {
            claimedTerritory.Add(node);
            foreach (ClaimedTerritoryObservation observation in territoryObservations)
            {
                if (observation.nodeId == node.NodeID)
                {
                    observation.isSuperseded = true;
                }
            }

            foreach (EnemyPositionObservation observation in positionObservations)
            {
                if (observation.nodeId == node.NodeID)
                {
                    observation.isSuperseded = true;
                }
            }
        }
    }

    public void TakeClaimedTerritory(BuildingNode node)
    {
        if (claimedTerritory.Contains(node))
        {
            claimedTerritory.Remove(node);
        }
    }

    public void OnSuccessfulTeamIntelligence(BuildingNode node, int teamId, int currentTurn)
    {
        foreach (EnemyPositionObservation observation in positionObservations)
        {
            if (observation.turn == currentTurn && observation.nodeId == node.NodeID && 
                observation.source == KnowledgeSource.Intelligence && !observation.isSuperseded)
            {
                if (teamId == observation.teamId)
                {
                    return;
                }
                else
                {
                    observation.teamId = teamId;
                    observation.source = KnowledgeSource.Intelligence;
                    UpdateClaimedTerritoryObservation(node, teamId, currentTurn);
                    return;
                }
            }
        }

        EnemyPositionObservation newObservation = new EnemyPositionObservation
        {
            teamId = teamId,
            nodeId = node.NodeID,
            count = -1,
            turn = currentTurn,
            source = KnowledgeSource.Intelligence
        };

        positionObservations.Add(newObservation);
        UpdateClaimedTerritoryObservation(node, teamId, currentTurn);
    }

    public void OnSuccessfulCountIntelligence(BuildingNode node, int count, int currentTurn)
    {
        foreach (EnemyPositionObservation observation in positionObservations)
        {
            if (observation.turn == currentTurn && observation.nodeId == node.NodeID && 
                observation.source == KnowledgeSource.Intelligence && !observation.isSuperseded)
            {
                if (observation.count != -1)
                {
                    return;
                }
                else
                {
                    observation.count = count;
                    observation.source = KnowledgeSource.Intelligence;
                    return;
                }
            }
        }

        EnemyPositionObservation newObservation = new EnemyPositionObservation
        {
            teamId = -1,
            nodeId = node.NodeID,
            count = count,
            turn = currentTurn,
            source = KnowledgeSource.Intelligence
        };

        positionObservations.Add(newObservation);
    }

    public void UpdateClaimedTerritoryObservation(BuildingNode node, int teamId, int currentTurn, KnowledgeSource source = KnowledgeSource.Intelligence)
    {
        foreach (ClaimedTerritoryObservation observation in territoryObservations)
        {
            if (observation.turn == currentTurn && observation.nodeId == node.NodeID && !observation.isSuperseded)
            {
                if (teamId == observation.teamId)
                {
                    return;
                }
                else
                {
                    observation.teamId = teamId;
                    observation.source = source;
                    return;
                }
            }
        }

        ClaimedTerritoryObservation newObservation = new ClaimedTerritoryObservation
        {
            teamId = teamId,
            nodeId = node.NodeID,
            turn = currentTurn,
            source = source
        };

        territoryObservations.Add(newObservation);
    }

    public void OnEncounterObservation(BuildingNode node, List<UnitManager> units, int currentTurn, int winnerId)
    {
        Dictionary<int, int> teamCounts = new Dictionary<int, int>();
        foreach (UnitManager unit in units)
        {
            int teamId = unit.TeamID;

            if (!teamCounts.ContainsKey(teamId))
            {
                teamCounts[teamId] = 0;
                teamCounts[teamId]++;
            }
            else
            {
                teamCounts[teamId]++;
            }

            UnitSnapshotObservation snapshotObservation = new UnitSnapshotObservation
            {
                teamId = teamId,
                nodeId = node.NodeID,
                unitId = unit.UnitID,
                resistanceSnapshot = unit.CurrentResistance,
                strengthSnapshot = unit.CurrentStrength,
                intelligenceSnapshot = unit.CurrentIntelligence,
                agilitySnapshot = unit.CurrentAgility,
                turn = currentTurn,
                source = KnowledgeSource.Encounter
            };

            snapshotObservations.Add(snapshotObservation);
        }

        foreach (var teamCount in teamCounts)
        {
            EnemyPositionObservation newObservation = new EnemyPositionObservation
            {
                teamId = teamCount.Key,
                nodeId = node.NodeID,
                count = teamCount.Value,
                turn = currentTurn,
                source = KnowledgeSource.Encounter
            };
            positionObservations.Add(newObservation);
        }

        if (winnerId == -1)
        {
            return;
        }

        UpdateClaimedTerritoryObservation(node, winnerId, currentTurn, KnowledgeSource.Encounter);
    }
}
