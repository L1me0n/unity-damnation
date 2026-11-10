using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random; // TODO: remove this
using System.Linq;

public class MatchManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject aiBrainPrefab;
    [SerializeField] private DeploymentSelection deploymentSelection;
    [SerializeField] private EncountersManager encountersManager;
    [SerializeField] private TurnOrchestrator turnOrchestrator;
    [SerializeField] private DeploymentPositions deploymentPositionsInformation;

    [Header("Units")]
    [SerializeField] private int maxTeams = 4;
    [SerializeField] private List<UnitManager> allUnits = new List<UnitManager>();
    [SerializeField] private List<int> aliveUnits = new List<int>(new int[4]);

    [Header("Teams")]
    [SerializeField] private List<AITeam> aiTeams = new List<AITeam>();
    [SerializeField] private Dictionary<AITeam, Dictionary<UnitManager, BuildingNode>> aiUnits 
        = new Dictionary<AITeam, Dictionary<UnitManager, BuildingNode>>();

    [Header("Points")]
    [SerializeField] private List<int> nodePoints = new List<int>();
    [SerializeField] private List<int> survivalPoints = new List<int>();
    [SerializeField] private List<int> positions = new List<int>();
    [SerializeField] private List<int> survivalPool = new List<int> {5, 2, 1, 0};

    [Header("Deployment Positions")]
    [SerializeField] private List<BuildingNode> deploymentPositions = new List<BuildingNode>(new BuildingNode[4]);

    [Header("Settings")]
    [SerializeField] private int matchSeed = 12345;

    private int id = 0;

    private int currentPosition = 4;
    private bool isEndable = false;

    private bool matchEnded = false;
    private List<int> matchResults = new List<int>();

    private int deploymentCount = 0;

    public int MaxTeams => maxTeams;
    public bool IsEndable => isEndable;
    public bool MatchEnded => matchEnded;
    public IReadOnlyList<UnitManager> AllUnits => allUnits;
    public IReadOnlyList<AITeam> AITeams => aiTeams;
    public IReadOnlyList<BuildingNode> DeploymentPositions => deploymentPositions;

    public event Action<int> OnPlayerPointsChanged;

    private void OnEnable()
    {
        turnOrchestrator.ResolveEncounters += ResolveEncounters;
        turnOrchestrator.CheckEnd += EndMatch;
    }

    private void OnDisable()
    {
        turnOrchestrator.ResolveEncounters -= ResolveEncounters;
        turnOrchestrator.CheckEnd -= EndMatch;

        foreach (UnitManager unit in allUnits)
        {
            unit.OnDeath -= HandleDeath;
        }

        allUnits.Clear();
    }

    private void Awake()
    {
        if (deploymentSelection == null)
        {
            deploymentSelection = FindObjectOfType<DeploymentSelection>();
        }

        if (encountersManager == null)
        {
            encountersManager = FindObjectOfType<EncountersManager>();
        }

        if (turnOrchestrator == null)
        {
            turnOrchestrator = FindObjectOfType<TurnOrchestrator>();
        }

        if (deploymentPositionsInformation == null)
        {
            deploymentPositionsInformation = FindObjectOfType<DeploymentPositions>();
        }

        for (int i = 0; i < maxTeams; i++)
        {
            aliveUnits[i] = 6;
            survivalPoints.Add(-1);
            positions.Add(-1);
        }
    }

    private void Start()
    {
        BeginMatch();
    }

    public void BeginMatch()
    {
        Random.InitState(matchSeed);
        Debug.Log($"Match seed: {matchSeed}");

        MessageMenu.Instance.DisplayMessage("Battle starts! Please choose drop location");

        InitPointsList();

        InitAITeams();

        deploymentSelection.BeginDeployment();
    }

    private void InitPointsList()
    {
        for (int i = 0; i < maxTeams; i++)
        {
            nodePoints.Add(0);
        }
    }

    private void InitAITeams()
    {
        for (int i = 1; i < maxTeams; i++)
        {
            GameObject aiBrain = Instantiate(aiBrainPrefab, new Vector3(0, 0, 0), Quaternion.identity);

            AITeam aiTeam = aiBrain.GetComponent<AITeam>();
            aiTeams.Add(aiTeam);
            aiTeam.GenerateTeam(i);
            aiUnits[aiTeam] = new Dictionary<UnitManager, BuildingNode>();
        }
    }

    public void RecordDeploymentPositions(BuildingNode node, int teamId)
    {
        deploymentPositions[teamId] = node;
        deploymentCount++;

        if (deploymentCount == maxTeams)
        {
            ResolveHotDrops();
        }
    }

    public void RegisterUnit(UnitManager unit, int teamID)
    {
        if (!allUnits.Contains(unit))
        {
            allUnits.Add(unit);
            unit.OnDeath += HandleDeath;
            unit.SetTeamID(teamID);
            unit.SetUnitID(id);
            id++;

            if (teamID != 0)
            {
                AITeam aiTeam = aiTeams[teamID - 1];
                unit.SetAITeam(aiTeam);
            }
        }
    }

    public void ResolveHotDrops()
    {
        List<int> freeTeams = new List<int>();
        HashSet<BuildingNode> nodes = new HashSet<BuildingNode>();
        Dictionary<BuildingNode, int> defenderTeam = new Dictionary<BuildingNode, int>();

        for (int i = 0; i < deploymentPositions.Count; i++)
        {
            BuildingNode node = deploymentPositions[i];

            if (nodes.Add(node))
            {
                defenderTeam[node] = i;
            }
            else
            {
                if (defenderTeam.ContainsKey(node))
                {
                    if (defenderTeam[node] == i)
                    {
                        continue;
                    }
                    else
                    {
                        foreach (UnitManager unit in allUnits)
                        {
                            if (unit.TeamID == i || unit.TeamID == defenderTeam[node])
                            {
                                encountersManager.RegisterEncounter(unit, node);
                            }
                        }

                        defenderTeam.Remove(node);
                    }
                }
                else
                {
                    foreach (UnitManager unit in allUnits)
                    {
                        if (unit.TeamID == i)
                        {
                            encountersManager.RegisterEncounter(unit, node);
                        }
                    }
                }
            }
            
        }

        foreach (var pair in defenderTeam)
        {
            freeTeams.Add(pair.Value);
        }

        encountersManager.ResolveEncounters();

        foreach (UnitManager unit in allUnits)
        {
            if (freeTeams.Contains(unit.TeamID))
            {
                unit.Move(deploymentPositions[unit.TeamID]);
            }
        }
    }

    private void ResolveEncounters(int turn)
    {
        List<UnitManager> freeUnits = new List<UnitManager>();

        HashSet<BuildingNode> nodes = new HashSet<BuildingNode>();
        Dictionary<BuildingNode, List<UnitManager>> defenderUnits = new Dictionary<BuildingNode, List<UnitManager>>();

        foreach (UnitManager unit in allUnits)
        {
            BuildingNode node = unit.TargetNode == null ? unit.CurrentNode : unit.TargetNode;

            if (nodes.Add(node))
            {
                defenderUnits[node] = new List<UnitManager>();
                defenderUnits[node].Add(unit);
            }
            else
            {
                if (defenderUnits.ContainsKey(node))
                {
                    if (defenderUnits[node][0].TeamID == unit.TeamID)
                    {
                        defenderUnits[node].Add(unit);
                    }
                    else
                    {
                        encountersManager.RegisterEncounter(unit, node);
                        foreach (UnitManager defender in defenderUnits[node])
                        {
                            encountersManager.RegisterEncounter(defender, node);
                        }

                        defenderUnits.Remove(node);
                    }
                }
                else
                {
                    encountersManager.RegisterEncounter(unit, node);
                }
            }
        }

        foreach (var pair in defenderUnits)
        {
            foreach (UnitManager defender in pair.Value)
            {
                freeUnits.Add(defender);
            }
        }

        encountersManager.ResolveEncounters();

        foreach (UnitManager unit in freeUnits)
        {
            unit.Move();
        }
    }

    public void MoveAIUnit(UnitManager unit, AITeam aiTeam, BuildingNode node)
    {
        aiUnits[aiTeam][unit] = node;
        unit.SetCurrentNode(node);
        node.AddOccupant(unit);
    }

    public void MoveWinner(BuildingNode node, List<UnitManager> winners, List<UnitManager> losers)
    {
        if (winners[0].TeamID == 0)
        {
            deploymentPositionsInformation.HideAt(node);
        }

        foreach (UnitManager unit in losers)
        {
            unit.Die();
        }

        foreach (UnitManager unit in winners)
        {
            unit.Move(node);
        }
    }
    private void HandleDeath(UnitManager unit)
    {
        aliveUnits[unit.TeamID]--;

        if (aliveUnits[unit.TeamID] <= 0)
        {
            UpdatePositions(unit.TeamID);
        }

        allUnits.Remove(unit);

        if (unit.TeamID == 0)
        {
            return;
        }
        
        AITeam aiTeam = aiTeams[unit.TeamID - 1];
        aiUnits[aiTeam].Remove(unit);
        aiTeam.KillUnit(unit);
    }

    public void UpdateNodePoints(List<int> points)
    {
        nodePoints = points;

        OnPlayerPointsChanged?.Invoke(nodePoints[0]);
    }

    public int GetTeamNodePoints(int teamId)
    {
        return nodePoints[teamId];
    }

    public void UpdatePositions(int teamID)
    {
        Debug.Log($"Team {teamID} has been eliminated!");

        if (positions[teamID] != -1)
        {
            return;
        }

        positions[teamID] = currentPosition;
        currentPosition--;

        if ((currentPosition <= 1  || teamID == 0) && !isEndable)
        {
            DecidePositions();
        }
    }

    private void DecidePositions()
    {
        if (currentPosition == 1)
        {
            for (int i = 0; i < maxTeams; i++)
            {
                if (positions[i] == -1)
                {
                    positions[i] = currentPosition;
                    currentPosition--;
                    isEndable = true;
                    return;
                }
            }
        }

        Dictionary<int, int> leftovers = new Dictionary<int, int>();

        for (int i = 1; i < maxTeams; i++)
        {
            if (positions[i] == -1)
            {
                leftovers[i] = aliveUnits[i];
            }
        }

        var sortedLeftovers = leftovers.OrderBy(pair => pair.Value);
        List<List<int>> equals = new List<List<int>>();

        int idx = 0;
        equals.Add(new List<int>());
        foreach (var pair in sortedLeftovers)
        {
            if (equals[idx].Count == 0)
            {
                equals[idx].Add(pair.Key);
            }
            else
            {
                if (leftovers[equals[idx][0]] == leftovers[pair.Key])
                {
                    equals[idx].Add(pair.Key);
                }
                else
                {
                    equals.Add(new List<int>());
                    idx++;
                    equals[idx].Add(pair.Key);
                }
            }
        }

        for (int i = 0; i < equals.Count; i++)
        {
            while (equals[i].Count > 0)
            {
                int randomIdx = Random.Range(0, equals[i].Count);
                positions[equals[i][randomIdx]] = currentPosition;
                currentPosition--;

                equals[i].RemoveAt(randomIdx);
            }
        }

        isEndable = true;
    }

    private void EndMatch(int turn)
    {
        if (matchEnded)
        {
            return;
        }
        
        AllocateSurvivalPoints();

        Dictionary<int, int> finalScores = new Dictionary<int, int>();

        for (int i = 0; i < maxTeams; i++)
        {
            finalScores[i] = nodePoints[i] + survivalPoints[i];
        }

        for (int i = 0; i < maxTeams; i++)
        {
            Debug.Log($"T{i}: alive={aliveUnits[i]}, place={positions[i]}, " + $"nodes={nodePoints[i]}, survival={survivalPoints[i]}, score={finalScores[i]}");
        }

        var sortedScores = finalScores.OrderByDescending(pair => pair.Value);

        foreach (var pair in sortedScores)
        {
            matchResults.Add(pair.Key);
        }

        List<int> tiedTeams = new List<int>();

        tiedTeams.Add(matchResults[0]);
        for (int i = 1; i < maxTeams; i++)
        {
            if (finalScores[matchResults[i]] == finalScores[matchResults[0]])
            {
                tiedTeams.Add(matchResults[i]);
            }
        }

        if (tiedTeams.Count > 1)
        {
            Debug.Log("It is tie!");
            MessageMenu.Instance.DisplayMessage("It is tie!");
        }
        else
        {
            Debug.Log($"Winner is team {matchResults[0]}");
            MessageMenu.Instance.DisplayMessage($"Winner is team {matchResults[0]}");
        }

        matchEnded = true;
    }

    private void AllocateSurvivalPoints()
    {
        for (int i = 0; i < maxTeams; i++)
        {
            survivalPoints[i] = survivalPool[positions[i] - 1];
        }
    }
}
