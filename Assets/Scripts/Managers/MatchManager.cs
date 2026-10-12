using UnityEngine;
using System.Collections.Generic;

public class MatchManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject aiBrainPrefab;
    [SerializeField] private DeploymentSelection deploymentSelection;

    [Header("Units")]
    [SerializeField] private int maxTeams = 4;
    [SerializeField] private List<UnitManager> allUnits = new List<UnitManager>();

    [Header("Teams")]
    [SerializeField] private List<AITeam> aiTeams = new List<AITeam>();
    [SerializeField] private Dictionary<AITeam, Dictionary<UnitManager, BuildingNode>> aiUnits 
        = new Dictionary<AITeam, Dictionary<UnitManager, BuildingNode>>();
    [SerializeField] private Dictionary<UnitManager, BuildingNode> team1 = new Dictionary<UnitManager, BuildingNode>();
    [SerializeField] private Dictionary<UnitManager, BuildingNode> team2 = new Dictionary<UnitManager, BuildingNode>();
    [SerializeField] private Dictionary<UnitManager, BuildingNode> team3 = new Dictionary<UnitManager, BuildingNode>();

    private int id = 0;

    public IReadOnlyList<UnitManager> AllUnits => allUnits;

    private void OnDisable()
    {
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
    }

    private void Start()
    {
        BeginMatch();
    }

    public void BeginMatch()
    {
        MessageMenu.Instance.DisplayMessage("Battle starts! Please choose drop location");

        InitAITeams();

        deploymentSelection.BeginDeployment();
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
        aiUnits[aiTeam][unit] = node;
        unit.SetCurrentNode(node);
    }

    private void HandleDeath(UnitManager unit)
    {
        allUnits.Remove(unit);
    }
}
