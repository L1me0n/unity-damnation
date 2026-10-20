using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class OwnershipChecker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattlefieldGenerator battlefieldGenerator;
    [SerializeField] private Image buttonBackground;
    [SerializeField] private TurnOrchestrator turnOrchestrator;
    [SerializeField] private MatchManager matchManager;

    [Header("Settings")]
    [SerializeField] private List<BuildingNode> knownNodes = new List<BuildingNode>(); // nodes whose ownership player knows

    private bool isOn = false;
    private int playerOwnedNodes = 0;

    public int PlayerNodes => playerOwnedNodes;

    private void OnEnable()
    {
        turnOrchestrator.CheckOwners += CheckOwners;
        turnOrchestrator.CountNodePoints += CountNodePoints;
    }

    private void OnDisable()
    {
        turnOrchestrator.CheckOwners -= CheckOwners;
        turnOrchestrator.CountNodePoints -= CountNodePoints;
    }

    private void Awake()
    {
        if (battlefieldGenerator == null)
        {
            battlefieldGenerator = FindObjectOfType<BattlefieldGenerator>();
        }

        if (turnOrchestrator == null)
        {
            turnOrchestrator = FindObjectOfType<TurnOrchestrator>();
        }

        if (matchManager == null)
        {
            matchManager = FindObjectOfType<MatchManager>();
        }

        isOn = false;
        buttonBackground.color = Color.red;
    }

    private void Update()
    {
        CheckOwnership();
    }

    public void AddKnownNode(BuildingNode node)
    {
        knownNodes.Add(node);
    }

    private void CheckOwnership()
    {
        if (!isOn)
        {
            return;
        }

        foreach(var (node, nodePreview) in battlefieldGenerator.Nodes)
        {
            if (node.OwnerTeam == 0 || (knownNodes.Contains(node) && node.OwnerTeam > 0))
            {
                nodePreview.ToggleOwnershipFlag(true);
            }
            else
            {
                nodePreview.ToggleOwnershipFlag(false);
            }
        }
    }

    public void ToggleChecker()
    {
        isOn = !isOn;

        buttonBackground.color = isOn ? Color.green : Color.red;

        if (!isOn)
        {
            foreach(var (node, nodePreview) in battlefieldGenerator.Nodes)
            {
                nodePreview.ToggleOwnershipFlag(false);
            }
        }
    }

    private void CheckOwners(int turn)
    {
        foreach(BuildingNode node in battlefieldGenerator.Nodes.Keys)
        {
            if (node.OccupyingUnits.Count > 0)
            {
                node.SetOwnerTeam(node.OccupyingUnits[0].TeamID);
            }
        }
    }

    private void CountNodePoints(int turn)
    {
        List<int> points = new List<int>(new int[matchManager.MaxTeams]);

        foreach(BuildingNode node in battlefieldGenerator.Nodes.Keys)
        {
            if (node.OwnerTeam >= 0)
            {
                points[node.OwnerTeam] += 1;
            }
        }

        matchManager.UpdateNodePoints(points);
    }
}