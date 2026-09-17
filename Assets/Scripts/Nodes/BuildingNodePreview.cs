using UnityEngine;
using System.Collections.Generic;

public class BuildingNodePreview : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private GameObject previewPrefab;
    [SerializeField] private MatchManager matchManager;

    [Header("Unit Slots")]
    [SerializeField] private List<Transform> unitSlots = new List<Transform>();
    [SerializeField] private List<Transform> previewSlots = new List<Transform>();

    [Header("Units")]
    [SerializeField] private List<UnitManager> occupyingUnits = new List<UnitManager>();
    [SerializeField] private List<UnitManager> previewUnits = new List<UnitManager>();

    [Header("Settings")]
    [SerializeField] private bool isOccupied = false;
    [SerializeField] private bool isPreviewed = false;
    [SerializeField] private bool isOriginal = false;
    [SerializeField] private int previewPointer = -1;
    [SerializeField] private int unitPointer = -1;

    private int occupyingTeam = -1;

    private BuildingNode myNode;

    private SpriteRenderer childSpriteRenderer;
    private Color originalColor;

    

    public bool IsOccupied => isOccupied;
    public bool IsPreviewed => isPreviewed;
    public bool IsOriginal => isOriginal;
    public int OccupyingTeam => occupyingTeam;
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
    }

    public void Initialize(BuildingNode node)
    {
        myNode = node;
    }

    public void SetBlockedColor()
    {
        childSpriteRenderer.color = new Color(0.1383647f, 0.1009453f, 0.08223559f, 1f); 
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
                unitManager.SetCurrentNode(myNode);
                matchManager.RegisterUnit(unitManager, teamID);
                occupyingUnits.Add(unitManager);
                unitManager.SetObject(unit);
            }

            unit.transform.SetParent(slot, true);
            unitPointer++;

            GiveTeamColor(teamID, unit);
        }

        isOccupied = true;
        occupyingTeam = teamID;
        isPreviewed = false;
    }

    public List<UnitManager> GetUnitsForTeam(int teamID)
    {
        if (occupyingTeam == teamID)
        {
            return occupyingUnits;
        }

        return null;
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

        isOccupied = true;
        occupyingTeam = unitManager.TeamID;
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

        Transform slot = unitSlots[unitPointer];

        Transform obj = slot.transform.GetChild(0);
        UnitManager objUnitManager = obj.GetComponent<UnitManager>();

        SpriteRenderer unitHead = unit.transform.Find("Head").GetComponent<SpriteRenderer>();
        SpriteRenderer unitBody = unit.transform.Find("Body").GetComponent<SpriteRenderer>();

        unitHead.color = Color.white;
        unitBody.color = Color.white;

        unitPointer--;

        if (occupyingUnits.Count < 1)
        {
            isOccupied = false;
            occupyingTeam = -1;
        }
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
}
