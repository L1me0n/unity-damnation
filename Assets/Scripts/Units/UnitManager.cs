using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattlefieldGenerator battlefieldGenerator;
    [SerializeField] private TurnManager turnManager;

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
    [SerializeField] private int maxResistance;
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
    public GameObject Obj => me;

    public int MaxResistance => maxResistance;
    public int CurrentResistance => currentResistance;

    public int MaxStrength => maxStrength;
    public int CurrentStrength => currentStrength;

    public int MaxAgility => maxAgility;
    public int CurrentAgility => currentAgility;

    public int MaxIntelligence => maxIntelligence;
    public int CurrentIntelligence => currentIntelligence;

    private void OnEnable()
    {
        turnManager.OnTurnEnded += ConfirmTarget;
    }

    private void OnDisable()
    {
        turnManager.OnTurnEnded -= ConfirmTarget;
    }

    private void Awake()
    {
        if (battlefieldGenerator == null)
        {
            battlefieldGenerator = FindObjectOfType<BattlefieldGenerator>();
        }

        if (turnManager == null)
        {
            turnManager = FindObjectOfType<TurnManager>();
        }

        isAlive = true;
    }

    public void SetCurrentResistance(int amount) => 
        currentResistance = Mathf.Clamp(currentResistance + amount, 0, maxResistance);
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

    public void ConfirmTarget()
    {
        BuildingNodePreview currentNodePreview = battlefieldGenerator.Nodes[currentNode];

        currentNodePreview.SetOriginal(false);
        
        if (targetNode == null)
        {
            return;
        }

        BuildingNodePreview targetNodePreview = battlefieldGenerator.Nodes[targetNode];

        targetNodePreview.RemovePreview(this);
        targetNodePreview.MoveUnitHere(this);

        previousNode = currentNode;
        currentNode = targetNode;
        targetNode = null;
    }
}
