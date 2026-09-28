using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TapOrchestrator tapOrchestrator;

    private bool isMenuOpen = false;

    public static MessageMenu Instance { get; private set; }

    public bool IsMenuOpen => isMenuOpen;

    private void OnEnable()
    {
        tapOrchestrator.CheckUITap += HideMessage;
    }

    private void OnDisable()
    {
        tapOrchestrator.CheckUITap -= HideMessage;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (tapOrchestrator == null)
        {
            tapOrchestrator = FindObjectOfType<TapOrchestrator>();
        }

        Instance = this;

        messagePanel.SetActive(false);
    }

    public void DisplayMessage(string message)
    {
        isMenuOpen = true;

        messagePanel.SetActive(true);

        messageText.text = message;
    }

    public void HideMessage()
    {
        if (!isMenuOpen)
        {
            return;
        }

        isMenuOpen = false;
        
        messageText.text = "";

        messagePanel.SetActive(false);
    }
}
