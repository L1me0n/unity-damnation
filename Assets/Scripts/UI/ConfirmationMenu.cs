using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmationMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private void Awake()
    {
        if (confirmationPanel == null)
        {
            Debug.LogWarning("No reference for Confirmation Panel");
        }

        if (confirmationText == null)
        {
            Debug.LogWarning("No reference for Confirmation Text");
        }

        if (confirmButton == null)
        {
            Debug.LogWarning("No reference for Confirm Button");
        }

        if (cancelButton == null)
        {
            Debug.LogWarning("No reference for Cancel Button");
        }

        confirmationPanel.SetActive(false); 
    }

    public void TogglePanel(bool isActive)
    {
        confirmationPanel.SetActive(isActive);
    }

    public void FillConfirmationText(string text)
    {
        confirmationText.text = text;
    }

    public void RemoveButtonListeners()
    {
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
    }

    public void AddConfirmListener(Action method)
    {

        confirmButton.onClick.AddListener(() => method());
    }

    public void AddCancelListener(Action method)
    {
        cancelButton.onClick.AddListener(() => method());
    }
}
