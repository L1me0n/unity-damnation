using UnityEngine;
using System;

public class UnitManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattlefieldGenerator battlefieldGenerator;
    [SerializeField] private TurnOrchestrator turnOrchestrator;
    [SerializeField] private ZoneManager zoneManager;
    [SerializeField] private EncountersManager encountersManager;

    [Header("Unit Data")]
    [SerializeField] private int unitID;
    [SerializeField] private int teamID;
    [SerializeField] private bool isAlive;
    [SerializeField] private string unitName;
    [SerializeField] private GameObject me;

    [Header("Unit Location")]
    [SerializeField] private BuildingNode currentNode;
    [SerializeField] private BuildingNode previousNode;
    [SerializeField] private BuildingNode targetNode;

    [Header("Unit Resistance")]
    [SerializeField] private int maxResistance = 10;
    [SerializeField] private int currentResistance;

    [Header("Unit Strength")]
    [SerializeField] private int maxStrength;
    [SerializeField] private int currentStrength;

    [Header("Unit Agility")]
    [SerializeField] private int maxAgility;
    [SerializeField] private int currentAgility;

    [Header("Unit Intelligence")]
    [SerializeField] private int maxIntelligence;
    [SerializeField] private int currentIntelligence;

    public string UnitName => unitName;
    public int UnitID => unitID;
    public int TeamID => teamID;
    public BuildingNode CurrentNode => currentNode;
    public GameObject Obj => me;

    public int MaxResistance => maxResistance;
    public int CurrentResistance => currentResistance;

    public int MaxStrength => maxStrength;
    public int CurrentStrength => currentStrength;

    public int MaxAgility => maxAgility;
    public int CurrentAgility => currentAgility;

    public int MaxIntelligence => maxIntelligence;
    public int CurrentIntelligence => currentIntelligence;

    public event Action<UnitManager> OnDeath;

    private void OnEnable()
    {
        turnOrchestrator.ConfirmTeamDecisions += ConfirmTarget;
        turnOrchestrator.UnitZoneDamage += CheckNode;
    }

    private void OnDisable()
    {
        turnOrchestrator.ConfirmTeamDecisions -= ConfirmTarget;
        turnOrchestrator.UnitZoneDamage -= CheckNode;
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

        if (zoneManager == null)
        {
            zoneManager = FindObjectOfType<ZoneManager>();
        }

        if (encountersManager == null)
        {
            encountersManager = FindObjectOfType<EncountersManager>();
        }

        currentResistance = maxResistance;
        currentStrength = maxStrength;
        currentAgility = maxAgility;
        currentIntelligence = maxIntelligence;

        isAlive = true;
    }

    public void SetCurrentResistance(int amount)
    {
        currentResistance = Mathf.Clamp(currentResistance + amount, 0, maxResistance);
        if (currentResistance <= 0)
        {
            Die();
        }
    } 
        
    public void SetCurrentStrength(int amount) => 
        currentStrength = Mathf.Clamp(currentStrength + amount, 0, maxStrength);
    public void SetCurrentAgility(int amount) => 
        currentAgility = Mathf.Clamp(currentAgility + amount, 0, maxAgility);
    public void SetCurrentIntelligence(int amount) => 
        currentIntelligence = Mathf.Clamp(currentIntelligence + amount, 0, maxIntelligence);

    public void SetObject(GameObject obj)
    {
        me = obj;
    }

    public void SetUnitID(int unitID)
    {
        this.unitID = unitID;
    }

    public void SetTeamID(int teamID)
    {
        this.teamID = teamID;
    }

    public void SetCurrentNode(BuildingNode node)
    {
        if (previousNode != null)
        {
            previousNode.RemoveOccupant(this);
        }

        currentNode = node;
        previousNode = null;
    }

    public void SetTargetNode(BuildingNode node)
    {
        targetNode = node;
        BuildingNodePreview targetNodePreview = battlefieldGenerator.Nodes[targetNode];
        BuildingNodePreview currentNodePreview = battlefieldGenerator.Nodes[currentNode];

        currentNodePreview.TempRemove(this);
        targetNodePreview.PreviewMove(this);
    }

    public void CancelTargetNode(BuildingNode node)
    {
        targetNode = null;

        BuildingNodePreview targetNodePreview = battlefieldGenerator.Nodes[node];
        BuildingNodePreview currentNodePreview = battlefieldGenerator.Nodes[currentNode];

        targetNodePreview.RemovePreview(this);
        currentNodePreview.MoveUnitHere(this);
    }

    public void ConfirmTarget(int turn)
    {
        if (teamID != 0)
        {
            return;
        }

        if (targetNode == null)
        {
            return;
        }

        if (targetNode.OccupyingUnits.Count > 0)
        {
            if (targetNode.OccupyingUnits[0].TeamID != teamID)
            {
                encountersManager.RegisterEncounter(this, targetNode);
                return;
            }
        }

        BuildingNodePreview currentNodePreview = battlefieldGenerator.Nodes[currentNode];

        currentNodePreview.SetOriginal(false);

        BuildingNodePreview targetNodePreview = battlefieldGenerator.Nodes[targetNode];

        targetNodePreview.RemovePreview(this);
        targetNodePreview.MoveUnitHere(this);

        previousNode = currentNode;
        SetCurrentNode(targetNode);
        targetNode = null;
    }

    public void ConfirmTargetAfterEncounter()
    {
        BuildingNodePreview currentNodePreview = battlefieldGenerator.Nodes[currentNode];

        currentNodePreview.SetOriginal(false);

        BuildingNodePreview targetNodePreview = battlefieldGenerator.Nodes[targetNode];

        targetNodePreview.RemovePreview(this);
        targetNodePreview.MoveUnitHere(this);

        previousNode = currentNode;
        SetCurrentNode(targetNode);
        targetNode = null;
    }

    private void CheckNode(int turn)
    {
        if (teamID != 0)
        {
            return;
        }

        BuildingNodePreview currentNodePreview = battlefieldGenerator.Nodes[currentNode];

        NodeSafety nodeSafety = currentNodePreview.gameObject.GetComponent<NodeSafety>();

        if (!nodeSafety.IsSafe)
        {
            Debug.Log($"taking {zoneManager.ZoneDamage} damage");
            SetCurrentResistance(-zoneManager.ZoneDamage);
        }
    }

    public void Die()
    {
        if (!isAlive)
        {
            return;
        }
        
        isAlive = false;

        if (currentNode != null)
        {
            BuildingNodePreview currentNodePreview = battlefieldGenerator.Nodes[currentNode];

            currentNodePreview.RemoveUnit(this);
            currentNode.RemoveOccupant(this);
        }

        Debug.Log("Unit is dead");

        OnDeath?.Invoke(this);

        Destroy(gameObject);
    }
}
