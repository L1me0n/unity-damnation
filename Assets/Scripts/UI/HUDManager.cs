using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("TurnHUD")]
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TurnOrchestrator turnOrchestrator;
    [SerializeField] private MatchManager matchManager;
    [SerializeField] private Button turnButton;
    [SerializeField] private Button ownershipButton;
    
    [Header("ZoneHUD")]
    [SerializeField] private Image zoneFillBar;
    [SerializeField] private TextMeshProUGUI zoneCounter;
    [SerializeField] private ZoneManager zoneManager;
    [SerializeField] private TextMeshProUGUI zoneStageText;

    [Header("StateHUD")]
    [SerializeField] private TextMeshProUGUI playerPointsText;

    private void OnEnable()
    {
        turnOrchestrator.HUDdisplay += UpdateTurnHUD;
        turnOrchestrator.HUDdisplay += UpdateZoneHUD;
        matchManager.OnPlayerPointsChanged += UpdateStateHUD;
    }

    private void OnDisable()
    {
        turnOrchestrator.HUDdisplay -= UpdateTurnHUD;
        turnOrchestrator.HUDdisplay -= UpdateZoneHUD;
        matchManager.OnPlayerPointsChanged -= UpdateStateHUD;
    }

    private void Awake()
    {
        if (turnOrchestrator == null)
        {
            turnOrchestrator = FindObjectOfType<TurnOrchestrator>();
        }

        if (matchManager == null)
        {
            matchManager = FindObjectOfType<MatchManager>();
        }

        if (zoneManager == null)
        {
            zoneManager = FindObjectOfType<ZoneManager>();
        }

        turnText.text = "Turn: 0";

        playerPointsText.text = "Points: 0";

        turnButton.gameObject.SetActive(false);

        ownershipButton.gameObject.SetActive(false);

        UpdateZoneHUD(0);
    }

    private void UpdateTurnHUD(int turn)
    {
        turnText.text = $"Turn: {turn}";

        if (turn > 0)
        {
            turnButton.gameObject.SetActive(true);
            ownershipButton.gameObject.SetActive(true);
        }
    }

    private void UpdateZoneHUD(int turn)
    {
        zoneStageText.text = $"Stage: {zoneManager.CurrentStage.ToString()}";

        if (zoneManager.IsShrinking)
        {
            zoneFillBar.fillAmount = zoneManager.ShrinkingFill;
            zoneCounter.text = zoneManager.ShrinkingCounter.ToString();
        }
        else
        {
            zoneCounter.text = zoneManager.StageCounter.ToString();
        }
    }

    private void UpdateStateHUD(int points)
    {
        playerPointsText.text = $"Points = {points}";
    }
}
