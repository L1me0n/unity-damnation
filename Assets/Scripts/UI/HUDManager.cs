using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("TurnHUD")]
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TurnOrchestrator turnOrchestrator;
    [SerializeField] private Button turnButton;
    
    [Header("ZoneHUD")]
    [SerializeField] private Image zoneFillBar;
    [SerializeField] private TextMeshProUGUI zoneCounter;
    [SerializeField] private ZoneManager zoneManager;
    [SerializeField] private TextMeshProUGUI zoneStageText;

    private void OnEnable()
    {
        turnOrchestrator.HUDdisplay += UpdateTurnHUD;
        turnOrchestrator.HUDdisplay += UpdateZoneHUD;
    }

    private void OnDisable()
    {
        turnOrchestrator.HUDdisplay -= UpdateTurnHUD;
        turnOrchestrator.HUDdisplay -= UpdateZoneHUD;
    }

    private void Awake()
    {
        if (turnOrchestrator == null)
        {
            turnOrchestrator = FindObjectOfType<TurnOrchestrator>();
        }

        if (zoneManager == null)
        {
            zoneManager = FindObjectOfType<ZoneManager>();
        }

        turnText.text = $"Turn: 0";

        turnButton.gameObject.SetActive(false);

        UpdateZoneHUD(0);
    }

    private void UpdateTurnHUD(int turn)
    {
        turnText.text = $"Turn: {turn}";

        if (turn > 0)
        {
            turnButton.gameObject.SetActive(true);
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
}
