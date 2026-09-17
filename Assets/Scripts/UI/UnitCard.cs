using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class UnitCard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private List<Image> unitIconImages;
    [SerializeField] private TextMeshProUGUI unitStatsText;
    [SerializeField] private Image imageBackground;
    [SerializeField] private Button myButton;

    private bool isSelected = false;
    private UnitManager myUnit;

    public bool IsSelected => isSelected;
    public UnitManager MyUnit => myUnit;

    public event Action<UnitCard> OnButtonClicked;

    private void Awake()
    {
        if (unitNameText == null)
        {
            Debug.LogWarning("Unit Name Text reference is missing!");
        }

        if (unitIconImages.Count == 0)
        {
            Debug.LogWarning("Unit Icon Images reference is missing!");
        }

        if (unitStatsText == null)
        {
            Debug.LogWarning("Unit Stats Text reference is missing!");
        }

        if (imageBackground == null)
        {
            Debug.LogWarning("Image Background reference is missing!");
        }

        if (myButton == null)
        {
            Debug.LogWarning("Button reference is missing!");
        }
    }

    public void SetUp(UnitManager unit, bool defaultSelected = true)
    {
        if (unit == null)
        {
            Debug.LogWarning("Unit reference is null!");
            return;
        }

        myUnit = unit;

        unitNameText.text = myUnit.UnitName;

        switch (myUnit.TeamID)
        {
            case 0:
                for (int i = 0; i < unitIconImages.Count; i++)
                {
                    unitIconImages[i].color = Color.green;
                }
                break;
            case 1:
                for (int i = 0; i < unitIconImages.Count; i++)
                {
                    unitIconImages[i].color = Color.blue;
                }
                break;
            case 2:
                for (int i = 0; i < unitIconImages.Count; i++)
                {
                    unitIconImages[i].color = Color.red;
                }
                break;
            case 3:
                for (int i = 0; i < unitIconImages.Count; i++)
                {
                    unitIconImages[i].color = Color.yellow;
                }
                break;
            default:
                for (int i = 0; i < unitIconImages.Count; i++)
                {
                    unitIconImages[i].color = Color.white;
                }
                break;
        }

        unitStatsText.text = $"R: {myUnit.CurrentResistance} A: {myUnit.CurrentAgility} S: {myUnit.CurrentStrength} I: {myUnit.CurrentIntelligence}";

        SetBackground(defaultSelected);

        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(() => OnButtonClicked?.Invoke(this));
    }

    private void SetBackground(bool selected)
    {
        isSelected = selected;

        imageBackground.color = isSelected ? 
            new Color(imageBackground.color.r, imageBackground.color.g, imageBackground.color.b, 1f) : 
            new Color(imageBackground.color.r, imageBackground.color.g, imageBackground.color.b, 0f);
    }

    public void ToggleBackground()
    {
        isSelected = !isSelected;

        imageBackground.color = isSelected ? 
            new Color(imageBackground.color.r, imageBackground.color.g, imageBackground.color.b, 1f) : 
            new Color(imageBackground.color.r, imageBackground.color.g, imageBackground.color.b, 0f);
    }
}
