using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class DeploymentSelection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattlefieldGenerator battlefieldGenerator;
    [SerializeField] private NodeInteraction nodeInteraction;
    [SerializeField] private ConfirmationMenu confirmationMenu;

    private int[] randomLineType = new int[2]; // 0 - row, 1 - column
    private int[] randomLineIndex = new int[2];

    private List<BuildingNode> blockedNodes = new List<BuildingNode>();

    private BuildingNode selectedNode;
    private BuildingNode confirmedNode;

    private int width;
    private int height;

    private bool deploymentStarted = false;

    public BuildingNode ConfirmedNode => confirmedNode;

    public event Action OnDeploymentBlocked;

    private void OnEnable()
    {
        nodeInteraction.OnNodeTapped += SelectNode;
    }

    private void OnDisable()
    {
        nodeInteraction.OnNodeTapped -= SelectNode;
    }

    private void Awake()
    {
        if (battlefieldGenerator == null)
        {
            battlefieldGenerator = FindObjectOfType<BattlefieldGenerator>();
        }

        if (nodeInteraction == null)
        {
            nodeInteraction = FindObjectOfType<NodeInteraction>();
        }

        if (confirmationMenu == null)
        {
            confirmationMenu = FindObjectOfType<ConfirmationMenu>(); 
        }

        width = battlefieldGenerator.Width;
        height = battlefieldGenerator.Height;
    }

    private void Start()
    {
        BeginDeployment();
    }

    public void BeginDeployment()
    {
        if (deploymentStarted)
        {
            return;
        }

        RollLines();
        
        ApplyRule();

        deploymentStarted = true;
    }

    private void RollLines()
    {
        for (int i = 0; i < 2; i++)
        {
            randomLineType[i] = Random.Range(0, 2);
            if (randomLineType[i] == 0)
            {
                randomLineIndex[i] = Random.Range(0, height);
            }
            else
            {
                randomLineIndex[i] = Random.Range(0, width);
            }
        }
    }

    private void ApplyRule()
    {
        foreach (var (node, nodePreview) in battlefieldGenerator.Nodes)
        {
            for (int i = 0; i < 2; i++)
            {
                switch (randomLineType[i])
                {
                    case 0:
                        if (node.Row == randomLineIndex[i])
                        {
                            node.BlockDeployment();
                            nodePreview.SetBlockedColor();
                            blockedNodes.Add(node);
                        }
                        break;
                    
                    case 1:
                        if (node.Column == randomLineIndex[i])
                        {
                            node.BlockDeployment();
                            nodePreview.SetBlockedColor();
                            blockedNodes.Add(node);
                        }
                        break;
                    
                    default:
                        break;
                }
            }
        }
    }

    private void SelectNode(BuildingNodePreview nodePreview)
    {
        if (!deploymentStarted || selectedNode != null)
        {
            return;
        }
        
        BuildingNode node = nodePreview.Node;

        if (node.IsDeploymentBlocked)
        {
            OnDeploymentBlocked?.Invoke();
            return;
        } 

        selectedNode = node;
        RequestConfirmation();
    } 

    private void RequestConfirmation()
    {
        BuildingNodePreview nodePreview = battlefieldGenerator.Nodes[selectedNode];
        nodePreview.HighlightColor();

        confirmationMenu.TogglePanel(true);

        confirmationMenu.FillConfirmationText("Confirm drop building");

        confirmationMenu.AddConfirmListener(ConfirmSelection);

        confirmationMenu.AddCancelListener(CancelSelection);
    }

    private void ConfirmSelection()
    {
        UnblockLines();

        HidePanel();

        confirmedNode = selectedNode;

        selectedNode = null;

        deploymentStarted = false;

        FinishDeployment();
    }

    private void CancelSelection()
    {
        ResetNodeColor(selectedNode);

        HidePanel();

        selectedNode = null;
    }

    private void UnblockLines()
    {
        foreach (BuildingNode node in blockedNodes)
        {
            node.UnblockDeployment();
            ResetNodeColor(node);
        }

        blockedNodes.Clear();
    }

    private void HidePanel()
    {
        confirmationMenu.FillConfirmationText("");
        confirmationMenu.RemoveButtonListeners();
        confirmationMenu.TogglePanel(false);
    }

    private void ResetNodeColor(BuildingNode node)
    {
        BuildingNodePreview nodePreview = battlefieldGenerator.Nodes[node];
        nodePreview.ResetColor();
    }

    private void FinishDeployment()
    {
        BuildingNodePreview nodePreview = battlefieldGenerator.Nodes[confirmedNode];
        nodePreview.ResetColor();
        nodePreview.SpawnUnits(0); // player team is 0
    }
}
