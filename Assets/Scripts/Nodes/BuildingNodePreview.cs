using UnityEngine;
using System.Collections.Generic;

public class BuildingNodePreview : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private GameObject previewPrefab;
    [SerializeField] private MatchManager matchManager;
    [SerializeField] private SpriteRenderer ownershipFlag;

    [Header("Unit Slots")]
    [SerializeField] private List<Transform> unitSlots = new List<Transform>();
    [SerializeField] private List<Transform> previewSlots = new List<Transform>();

    [Header("Units")]
    [SerializeField] private List<UnitManager> occupyingUnits = new List<UnitManager>();
    [SerializeField] private List<UnitManager> previewUnits = new List<UnitManager>();

    [Header("Settings")]
    [SerializeField] private bool isPreviewed = false;
    [SerializeField] private bool isOriginal = false;
    [SerializeField] private int previewPointer = -1;
    [SerializeField] private int unitPointer = -1;

    private BuildingNode myNode;

    private SpriteRenderer childSpriteRenderer;
    private Color originalColor;
    private Color playerOwnershipColor = new Color(0.6918238f, 0.491016f, 0f, 1f);
    private Color enemyOwnershipColor = new Color(0.1383647f, 0.1009453f, 0.08223559f, 1f);

    public bool IsPreviewed => isPreviewed;
    public bool IsOriginal => isOriginal;
    public BuildingNode Node => myNode;

    private void Awake()
    {
        Transform childTransform = transform.Find("Tile");

        if (childTransform == null)
        {
            return;
        }

        if (unitPrefab == null)
        {
            Debug.LogError("Unit Prefab is not assigned in the inspector.");
            return;
        }

        if (previewPrefab == null)
        {
            Debug.LogError("Preview Prefab is not assigned in the inspector.");
            return;
        }

        if (matchManager == null)
        {
            matchManager = FindObjectOfType<MatchManager>();
        }

        childSpriteRenderer = childTransform.GetComponent<SpriteRenderer>();

        originalColor = childSpriteRenderer.color;

        ownershipFlag.gameObject.SetActive(false);
    }

    public void Initialize(BuildingNode node)
    {
        myNode = node;
    }

    public void SetBlockedColor()
    {
        childSpriteRenderer.color = new Color(0.1383647f, 0.1009453f, 0.08223559f, 1f); 
    }

    public void SetDangerColor()
    {
        childSpriteRenderer.color = new Color(0.5f, 0f, 0f, 1f); 
        originalColor = childSpriteRenderer.color;
    }

    public void HighlightColor()
    {
        childSpriteRenderer.color = new Color(0.6918238f, 0.491016f, 0f, 1f); 
    }

    public void CommandColor()
    {
        childSpriteRenderer.color = new Color(0.05500167f, 0.6477987f, 0.1570017f, 1f); 
    }

    public void ResetColor()
    {
        childSpriteRenderer.color = originalColor;
    }

    // only for initial spawn
    public void SpawnUnits(int teamID)
    {
        foreach (Transform slot in unitSlots)
        {
            GameObject unit = Instantiate(unitPrefab, slot.position, Quaternion.identity);
            UnitManager unitManager = unit.GetComponent<UnitManager>();

            if (unitManager != null)
            {
                matchManager.RegisterUnit(unitManager, teamID);
                unitManager.SetCurrentNode(myNode);
                occupyingUnits.Add(unitManager);
                unitManager.SetObject(unit);
                myNode.AddOccupant(unitManager);
            }

            unit.transform.SetParent(slot, true);
            unitPointer++;

            GiveTeamColor(teamID, unit);
        }

        isPreviewed = false;
    }

    public List<UnitManager> GetPreviewedUnits()
    {
        if (isPreviewed)
        {
            return previewUnits;
        }

        return null;
    }

    public void MoveUnitHere(UnitManager unitManager)
    {
        // put into unitSlots of given node

        unitPointer++;

        if (unitPointer >= unitSlots.Count)
        {
            return;
        }

        Transform slot = unitSlots[unitPointer];

        GameObject unit = unitManager.Obj;

        GiveTeamColor(unitManager.TeamID, unit);

        unit.transform.SetParent(slot, false);

        occupyingUnits.Add(unitManager);

        myNode.AddOccupant(unitManager);
    }

    public void PreviewMove(UnitManager unitManager)
    {
        // put into previewSlots of given node

        previewPointer++;

        if (previewPointer >= previewSlots.Count)
        {
            return;
        }

        Transform slot = previewSlots[previewPointer];

        GameObject preview = Instantiate(previewPrefab, slot.position, Quaternion.identity);

        preview.transform.SetParent(slot, true);

        isPreviewed = true;

        previewUnits.Add(unitManager);
    }

    public void TempRemove(UnitManager unitManager)
    {
        // remove from occupyingUnits of given node
        // put into previewSlots of given node

        if (!occupyingUnits.Contains(unitManager))
        {
            return;
        }

        occupyingUnits.Remove(unitManager);

        GameObject unit = unitManager.Obj;

        SpriteRenderer unitHead = unit.transform.Find("Head").GetComponent<SpriteRenderer>();
        SpriteRenderer unitBody = unit.transform.Find("Body").GetComponent<SpriteRenderer>();

        unitHead.color = Color.white;
        unitBody.color = Color.white;

        unitPointer--;
    }

    public void RemovePreview(UnitManager unitManager)
    {
        if (!previewUnits.Contains(unitManager))
        {
            return;
        }

        previewUnits.Remove(unitManager);

        Transform slot = previewSlots[previewPointer];
            
        if (slot.childCount > 0)
        {
            Destroy(slot.GetChild(0).gameObject);
        }

        previewPointer--;

        if (previewUnits.Count < 1)
        {
            isPreviewed = false;
        }
    }

    private void GiveTeamColor(int teamID, GameObject unit)
    {
        SpriteRenderer unitHead = unit.transform.Find("Head").GetComponent<SpriteRenderer>();
        SpriteRenderer unitBody = unit.transform.Find("Body").GetComponent<SpriteRenderer>();
        switch (teamID)
        {
            case 0:
                unitHead.color = Color.green;
                unitBody.color = Color.green;
                break;
            case 1:
                unitHead.color = Color.blue;
                unitBody.color = Color.blue;
                break;
            case 2:
                unitHead.color = Color.red;
                unitBody.color = Color.red;
                break;
            case 3:
                unitHead.color = Color.yellow;
                unitBody.color = Color.yellow;
                break;
            default:
                unitHead.color = Color.white;
                unitBody.color = Color.white;
                break;
        }
    }

    public void SetOriginal(bool original)
    {
        isOriginal = original;
    }

    public void RemoveUnit(UnitManager unit)
    {
        if (!occupyingUnits.Contains(unit))
        {
            return;
        }

        occupyingUnits.Remove(unit);

        unitPointer--;
    }

    public void ToggleOwnershipFlag(bool show)
    {
        ownershipFlag.gameObject.SetActive(show);

        if (show)
        {
            if (myNode.OwnerTeam == 0)
            {
                ownershipFlag.color = playerOwnershipColor;
            }
            else if (myNode.OwnerTeam < 0)
            {
                ownershipFlag.gameObject.SetActive(false);
            }
            else
            {
                // Later it will be checked if player has this knowledge before coloring it
                ownershipFlag.color = enemyOwnershipColor;
            }
        }
    }
}
