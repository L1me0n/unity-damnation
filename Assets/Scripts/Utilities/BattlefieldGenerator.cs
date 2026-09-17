using UnityEngine;
using System.Collections.Generic;

public class BattlefieldGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject buildingNodePrefab;
    [SerializeField] private Transform battlefieldParent;

    [Header("Battlefield Settings")]
    [SerializeField] private int battlefieldWidth = 8;
    [SerializeField] private int battlefieldHeight = 8;

    private float startX => -(battlefieldWidth / 2f - 0.5f);
    private float startY => -(battlefieldHeight / 2f - 0.5f);

    private readonly Dictionary<BuildingNode, BuildingNodePreview> nodes 
        = new Dictionary<BuildingNode, BuildingNodePreview>();

    private List<List<BuildingNode>> allNodes
        = new List<List<BuildingNode>>();

    public IReadOnlyDictionary<BuildingNode, BuildingNodePreview> Nodes => nodes;

    public int Width => battlefieldWidth;
    public int Height => battlefieldHeight;

    private void Awake()
    {
        if (buildingNodePrefab == null)
        {
            Debug.LogError("Building Node Prefab is not assigned in the inspector.");
            return;
        }

        GenerateBattlefield();
    }

    private void GenerateBattlefield()
    {
        for (int i = 0; i < battlefieldHeight; i++)
        {
            List<BuildingNode> iList = new List<BuildingNode>();
            for (int j = 0; j < battlefieldWidth; j++)
            {
                float spawnY = startY + i;
                float spawnX = startX + j;

                Vector3 position = new Vector3(spawnX, spawnY, 0);
                GameObject nodeObject = Instantiate(buildingNodePrefab, battlefieldParent);
                nodeObject.transform.localPosition = position;

                BuildingNode node = new BuildingNode
                (
                    nodes.Count,
                    i,
                    j
                );

                BuildingNodePreview nodePreview = nodeObject.GetComponent<BuildingNodePreview>();
                if (nodePreview != null)
                {
                    nodePreview.Initialize(node);
                }

                nodes.Add(node, nodePreview);
                iList.Add(node);
            }
            allNodes.Add(iList);
        }
    }

    public BuildingNode GetNode(int row, int column)
    {
        //row = Mathf.Abs(row - battlefieldHeight + 1);
        return allNodes[row][column];
    }
}
