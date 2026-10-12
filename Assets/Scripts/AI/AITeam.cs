using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class AITeam : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattlefieldGenerator battlefieldGenerator;
    [SerializeField] private DeploymentSelection deploymentSelection;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Transform unitsPool;
    [SerializeField] private MatchManager matchManager;

    [Header("Settings")]
    [SerializeField] private int unitsPerTeam = 6;
    [SerializeField] private List<UnitManager> units = new List<UnitManager>();

    private int teamId;

    public int TeamId => teamId;

    private void OnEnable()
    {
        deploymentSelection.OnRuleApplied += TeamDeployment;
    }

    private void OnDisable()
    {
        deploymentSelection.OnRuleApplied -= TeamDeployment;
    }

    private void Awake()
    {
        battlefieldGenerator = FindObjectOfType<BattlefieldGenerator>();
        deploymentSelection = FindObjectOfType<DeploymentSelection>();
        unitsPool = GameObject.Find("UnitsPool").transform;
        matchManager = FindObjectOfType<MatchManager>();
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
        foreach (UnitManager unit in units)
        {
            matchManager.MoveAIUnit(unit, this, node);
        }
    }
}
