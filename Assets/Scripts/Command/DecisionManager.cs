using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class DecisionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CommandMenu commandMenu;
    [SerializeField] private BattlefieldGenerator battlefieldGenerator;
    [SerializeField] private NodeInteraction nodeInteraction;
    [SerializeField] private DecisionMenu decisionMenu;
    [SerializeField] private Button decisionButton;

    private List<BuildingNode> neighbors = new List<BuildingNode>();
    private List<BuildingNodePreview> neighborsPreview = new List<BuildingNodePreview>();

    private List<UnitManager> selectedUnits = new List<UnitManager>();

    private BuildingNodePreview tappedNodePreview;

    private bool isDeciding = false;

    private void OnEnable()
    {
        commandMenu.OnSelectionConfirmed += StartDecision;
        nodeInteraction.OnNodeTapped += OnNodeTapped;
        decisionMenu.OnSelectionConfirmed += CancelDecision;

    }

    private void OnDisable()
    {
        commandMenu.OnSelectionConfirmed -= StartDecision;
        nodeInteraction.OnNodeTapped -= OnNodeTapped;
        decisionMenu.OnSelectionConfirmed -= CancelDecision;
    }

    private void Awake()
    {
        if (commandMenu == null)
        {
            commandMenu = FindObjectOfType<CommandMenu>();
        }

        if (battlefieldGenerator == null)
        {
            battlefieldGenerator = FindObjectOfType<BattlefieldGenerator>();
        }

        if (nodeInteraction == null)
        {
            nodeInteraction = FindObjectOfType<NodeInteraction>();
        }

        decisionButton.gameObject.SetActive(false); 
    }

    public void StartDecision(IReadOnlyList<UnitManager> units, BuildingNode node)
    {
        decisionButton.gameObject.SetActive(true);

        isDeciding = true;

        int nodeColumn = node.Column;
        int nodeRow = node.Row;

        FillSelectedUnits(units);

        //nodeRow = Mathf.Abs(nodeRow - battlefieldGenerator.Height + 1);

        if (!(nodeColumn-1 < 0))
        {
            neighbors.Add(battlefieldGenerator.GetNode(nodeRow, nodeColumn-1));
        }
        if (!(nodeColumn+1 > battlefieldGenerator.Width-1))
        {
            neighbors.Add(battlefieldGenerator.GetNode(nodeRow, nodeColumn+1));
        }
        if (!(nodeRow-1 < 0))
        {
            neighbors.Add(battlefieldGenerator.GetNode(nodeRow-1, nodeColumn));
        }
        if (!(nodeRow+1 > battlefieldGenerator.Height-1))
        {
            neighbors.Add(battlefieldGenerator.GetNode(nodeRow+1, nodeColumn));
        }

        foreach (BuildingNode neighbor in neighbors)
        {
            BuildingNodePreview nodePreview 
                = battlefieldGenerator.Nodes[neighbor];

            nodePreview.CommandColor();
            neighborsPreview.Add(nodePreview);
        }
        // Show adjacent nodes                                                                          +
        // Subscribe to shown adjacent nodes' clicks                                                    +
        // If node selected, change selectedUnits target nodes                                          +
        // Later turn end will actually move units
        // Visually show "ghosts" of units on the target nodes                                          +
        // Make DecisionMenu (clone of CommandMenu), which will be shown on clicking target nodes       +
        // In DecisionMenu, on target nodes, those units who have it as a target
        // are shown there. On swiping up, all unselected units are cancelled to go there,
        // on swipe down, cancels any changes.                                                          +
    }

    private void OnNodeTapped(BuildingNodePreview nodePreview)
    {
        if (neighborsPreview.Contains(nodePreview) && isDeciding)
        {
            ConfirmDecision(nodePreview);
        }
    }

    private void ConfirmDecision(BuildingNodePreview nodePreview)
    {
        // TODO

        isDeciding = false;

        foreach (BuildingNodePreview neighborPreview in neighborsPreview)
        {
            neighborPreview.ResetColor();
        }

        foreach (UnitManager unit in selectedUnits)
        {
            unit.SetTargetNode(nodePreview.Node);
        }

        Reset();
    }

    public void StopDecision()
    {
        isDeciding = false;

        foreach (BuildingNodePreview neighborPreview in neighborsPreview)
        {
            neighborPreview.ResetColor();
        }

        Reset();
    }

    private void FillSelectedUnits(IReadOnlyList<UnitManager> units)
    {
        foreach (UnitManager unit in units)
        {
            selectedUnits.Add(unit);
        }
    }

    private void CancelDecision(IReadOnlyList<UnitManager> units, BuildingNode node)
    {
        foreach (UnitManager unit in units)
        {
            unit.CancelTargetNode(node);
        }
    }

    private void Reset()
    {
        selectedUnits.Clear();
        neighbors.Clear();
        neighborsPreview.Clear();

        decisionButton.gameObject.SetActive(false);
    }
}
