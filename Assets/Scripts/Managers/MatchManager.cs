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

    private int id = 0;

    private int currentPosition = 4;
    private bool isEndable = false;

    private bool matchEnded = false;
    private List<int> matchResults = new List<int>();

    public int MaxTeams => maxTeams;
    public bool IsEndable => isEndable;
    public bool MatchEnded => matchEnded;
    public IReadOnlyList<UnitManager> AllUnits => allUnits;

    public event Action<int> OnPlayerPointsChanged;

    private void OnEnable()
    {
        turnOrchestrator.CheckEnd += EndMatch;
    }

    private void OnDisable()
    {
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

    public void RegisterUnit(UnitManager unit, int teamID)
    {
        if (!allUnits.Contains(unit))
        {
            allUnits.Add(unit);
            unit.OnDeath += HandleDeath;
            unit.SetTeamID(teamID);
            unit.SetUnitID(id);
            id++;
        }
    }

    public void MoveAIUnit(UnitManager unit, AITeam aiTeam, BuildingNode node)
    {
        if (node.OccupyingUnits.Count > 0)
        {
            if (node.OccupyingUnits[0].TeamID != aiTeam.TeamId)
            {
                encountersManager.RegisterEncounter(unit, node);
                return;
            }
        }

        aiUnits[aiTeam][unit] = node;
        unit.SetCurrentNode(node);
        node.AddOccupant(unit);
    }

    public void MoveWinner(BuildingNode node, int winnerId, List<UnitManager> defenders, Dictionary<int, List<UnitManager>> attackers, bool isOccupantWinner)
    {
        foreach (int i in attackers.Keys)
        {
            if (winnerId == i)
            {
                continue;
            }

            for (int j = attackers[i].Count - 1; j >= 0; j--)
            {
                attackers[i][j].Die();
            }
        }

        if (isOccupantWinner)
        {
            return;
        }

        for (int i = defenders.Count - 1; i >= 0; i--)
        {
            defenders[i].Die();
        }

        foreach (UnitManager unit in attackers[winnerId])
        {
            if (unit.TeamID == 0)
            {
                unit.ConfirmTargetAfterEncounter();
            }
            else
            {
                AITeam aiTeam = aiTeams[unit.TeamID - 1];
                aiUnits[aiTeam][unit] = node;
                unit.SetCurrentNode(node);
                node.AddOccupant(unit);
            }
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
    }

    public void UpdateNodePoints(List<int> points)
    {
        nodePoints = points;

        OnPlayerPointsChanged?.Invoke(nodePoints[0]);
    }

    public void UpdatePositions(int teamID)
    {
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
