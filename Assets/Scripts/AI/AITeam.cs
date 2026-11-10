using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Linq;

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
    [SerializeField] private ZoneManager zoneManager;
    [SerializeField] private ZoneBuilder zoneBuilder;

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
        turnOrchestrator.MakeAIMoveDecisions += HandleAIDecisions;
    }

    private void OnDisable()
    {
        deploymentSelection.OnRuleApplied -= TeamDeployment;
        turnOrchestrator.ShowDeploymentPositions -= GetDeploymentPositions;
        turnOrchestrator.MakeAIMoveDecisions -= HandleAIDecisions;
    }

    private void Awake()
    {
        battlefieldGenerator = FindObjectOfType<BattlefieldGenerator>();
        deploymentSelection = FindObjectOfType<DeploymentSelection>();
        unitsPool = GameObject.Find("UnitsPool").transform;
        matchManager = FindObjectOfType<MatchManager>();
        turnOrchestrator = FindObjectOfType<TurnOrchestrator>();
        zoneManager = FindObjectOfType<ZoneManager>();
        zoneBuilder = FindObjectOfType<ZoneBuilder>();
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

    private void HandleAIDecisions(int turn)
    {
        if (turn == 1)
        {
            return;
        }

        if (units.Count == 0)
        {
            return;
        }

        BuildingNode targetNode = DecideNextTarget(units[0], turn);

        foreach (UnitManager unit in units)
        {
            if (targetNode == unit.CurrentNode)
            {
                continue;
            }

            unit.SetTargetNode(targetNode);
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

    // TODO
    // Each condition either reduces or increases that weights
    // There are 3 lanes in Phase 9: safety, points boost, and enemy positioning
    // Safety: base is +90 for safe, stages 1-3 -10. stages 4-5 -120.
    // is inside revealed future area and not shrinking +100, shrinking +95
    // Points boost: None is +30, New building +65, Enemy-claimed building +100 (for each 5 turns old, is -5, capped at -35)
    // Enemy positioning: None 100, Just position observation -15, for each +unit obseravtion starting from 2 -15 (for each 1 turn old, is +3, capped at 100)
    // Clamped at 10.
    public BuildingNode DecideNextTarget(UnitManager unit, int currentTurn)
    {
        float earlyStageLoss = 10f;
        float lateStageLoss = 120f;

        Dictionary<BuildingNode, float> possibleTargets = new Dictionary<BuildingNode, float>();

        BuildingNode currentNode = unit.CurrentNode;
        List<BuildingNode> neighbors = battlefieldGenerator.GetNeighbors(currentNode);

        possibleTargets[currentNode] = 0f;

        // First check if any of the neighbors are safe and unclaimed
        foreach (BuildingNode neighbor in neighbors)
        {
            possibleTargets[neighbor] = 0f;
        }

        List<BuildingNode> candidates = new List<BuildingNode>(possibleTargets.Keys);

        List<ClaimedTerritoryObservation> reversedTerritoryObservations = territoryObservations.AsEnumerable().Reverse().ToList();
        List<EnemyPositionObservation> reversedPositionObservations = positionObservations.AsEnumerable().Reverse().ToList();

        foreach (BuildingNode node in candidates)
        {
            float safetyBase = 90f;
            float pointsBase = 30f;
            float enemiesBase = 100f;

            BuildingNodePreview preview = battlefieldGenerator.Nodes[node];

            NodeSafety nodeSafety = preview.gameObject.GetComponent<NodeSafety>();

            if (zoneBuilder.IsInsideRevealedFutureArea(node))
            {
                if (zoneManager.IsShrinking)
                {
                    safetyBase += 5f;
                }
                else
                {
                    safetyBase += 10f;
                }
            }

            else if (!nodeSafety.IsSafe)
            {
                float stagesLoss = 3 * earlyStageLoss + 2 * lateStageLoss;
                switch (zoneManager.CurrentStage)
                {
                    case 5:
                        safetyBase -= stagesLoss;
                        break;
                    
                    case 4:
                        safetyBase -= stagesLoss - lateStageLoss;
                        break;

                    case 3:
                        safetyBase -= stagesLoss - 2 * lateStageLoss;
                        break;

                    case 2:
                        safetyBase -= stagesLoss - 2 * lateStageLoss - earlyStageLoss;
                        break;

                    case 1:
                        safetyBase -= stagesLoss - 2 * lateStageLoss - 2 * earlyStageLoss;
                        break;

                    default:
                        break;
                }
            }

            bool isEnemyClaimed = false;

            foreach (ClaimedTerritoryObservation claimedTerritory in reversedTerritoryObservations)
            {
                if (claimedTerritory.isSuperseded)
                {
                    continue;
                }
                
                if (claimedTerritory.nodeId == node.NodeID)
                {
                    float pointsBonus = 70f;
                    pointsBonus -= Mathf.Min(Mathf.Floor((currentTurn - claimedTerritory.turn) / 5f) * 5, 35f);
                    pointsBase += pointsBonus;
                    isEnemyClaimed = true;
                    break;
                }
            }

            if (!claimedTerritory.Contains(node) && !isEnemyClaimed)
            {
                pointsBase += 35f;
            }

            foreach (EnemyPositionObservation positionObservation in reversedPositionObservations)
            {
                if (positionObservation.isSuperseded)
                {
                    continue;
                }

                if (positionObservation.nodeId == node.NodeID)
                {
                    if (positionObservation.count == -1)
                    {
                        enemiesBase = Mathf.Min(Mathf.Floor(currentTurn - positionObservation.turn) * 3 + enemiesBase - 15f, 100f);
                        break;
                    }

                    enemiesBase = Mathf.Min(Mathf.Floor(currentTurn - positionObservation.turn) * 3 + enemiesBase - positionObservation.count * 15f, 100f);
                    break;
                }
            }

            Debug.Log($"For team {unit.TeamID}\n" +
                      $"Candidate node has row {node.Row} and column {node.Column}\n" +
                      $"Safety bonus is {safetyBase}\n" +
                      $"Points bonus is {pointsBase}\n" +
                      $"Enemies bonus is {enemiesBase}\n");
            possibleTargets[node] += safetyBase + pointsBase + enemiesBase;
        }

        float weightsSum = 0f;

        foreach (BuildingNode node in candidates)
        {
            possibleTargets[node] = Mathf.Max(possibleTargets[node], 10f);

            Debug.Log($"For team {unit.TeamID}\n" +
                      $"Candidate node has row {node.Row} and column {node.Column}\n" +
                      $"Weight is {possibleTargets[node]}\n");

            weightsSum += possibleTargets[node];
        }

        float randomRoll = Random.Range(0f, weightsSum);

        Debug.Log($"For team {unit.TeamID} roll is {randomRoll} from {weightsSum}");

        float cumulativeWeight = 0f;

        BuildingNode decidedNode = null;

        foreach (BuildingNode node in candidates)
        {
            cumulativeWeight += possibleTargets[node];

            if (randomRoll < cumulativeWeight)
            {
                decidedNode = node;
                break;
            }
        }

        if (decidedNode == null)
        {
            decidedNode = candidates[candidates.Count - 1];
        }

        Debug.Log($"For team {unit.TeamID} decided node has row {decidedNode.Row} and column {decidedNode.Column}");
        return decidedNode;
    }

    public void KillUnit(UnitManager unit)
    {
        if (!units.Contains(unit))
        {
            return;
        }

        units.Remove(unit);
    }
}
