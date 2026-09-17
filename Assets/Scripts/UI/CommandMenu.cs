using UnityEngine;
using System.Collections.Generic;
using System;

public class CommandMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VerticalSwipeReader verticalSwipeReader;
    [SerializeField] private NodeInteraction nodeInteraction;
    [SerializeField] private GameObject commandMenuPanel;
    
    [Header("Slots")]
    [SerializeField] private List<Transform> unitCards = new List<Transform>();

    [Header("Settings")]
    [SerializeField] private int maxUnitsPerNode = 6;

    private List<UnitManager> selectedUnits = new List<UnitManager>();

    private bool isMenuOpen = false;
    private BuildingNode myNode;

    public event Action<IReadOnlyList<UnitManager>, BuildingNode> OnSelectionConfirmed;

    private void OnEnable()
    {
        verticalSwipeReader.SwipeUpPerformed += OnSwipeUp;
        verticalSwipeReader.SwipeDownPerformed += OnSwipeDown;

        nodeInteraction.OnNodeTapped += OnNodeTapped;
    }

    private void OnDisable()
    {
        verticalSwipeReader.SwipeUpPerformed -= OnSwipeUp;
        verticalSwipeReader.SwipeDownPerformed -= OnSwipeDown;

        nodeInteraction.OnNodeTapped -= OnNodeTapped;
    }

    private void Awake()
    {
        if (verticalSwipeReader == null)
        {
            verticalSwipeReader = FindObjectOfType<VerticalSwipeReader>();
        }

        if (nodeInteraction == null)
        {
            nodeInteraction = FindObjectOfType<NodeInteraction>();
        }

        commandMenuPanel.SetActive(false);
    }

    private void OnSwipeUp()
    {
        ConfirmCommand();
    }

    private void OnSwipeDown()
    {
        CloseCommandMenu();
    }

    private void OnNodeTapped(BuildingNodePreview nodePreview)
    {
        if (nodePreview.IsOccupied && nodePreview.OccupyingTeam == 0)
        {
            OpenCommandMenu(nodePreview);
            nodePreview.SetOriginal(true);
        }
    }

    private void OpenCommandMenu(BuildingNodePreview nodePreview)
    {
        commandMenuPanel.SetActive(true);
        isMenuOpen = true;

        int i = 0;
        List<UnitManager> playerUnits = nodePreview.GetUnitsForTeam(0); 
        myNode = nodePreview.Node;
        foreach (UnitManager unit in playerUnits)
        {
            if (unit != null)
            {
                unitCards[i].gameObject.SetActive(true);
                UnitCard card = unitCards[i].GetComponent<UnitCard>();
                
                if (card != null)
                {
                    card.OnButtonClicked += HandleButtonClicked;
                    card.SetUp(unit, true);
                }
            }
            i++;
        }
    }

    private void CloseCommandMenu()
    {
        commandMenuPanel.SetActive(false);
        isMenuOpen = false;

        foreach (Transform card in unitCards)
        {
            UnitCard unitCard = card.GetComponent<UnitCard>();
            unitCard.OnButtonClicked -= HandleButtonClicked;

            card.gameObject.SetActive(false);
        }
    }

    private void ConfirmCommand()
    {
        if (!isMenuOpen)
        {
            return;
        }

        selectedUnits.Clear();

        foreach (Transform card in unitCards)
        {
            if (!card.gameObject.activeSelf)
            {
                continue;
            }

            UnitCard unitCard = card.GetComponent<UnitCard>();
            if (unitCard.IsSelected)
            {
                selectedUnits.Add(unitCard.MyUnit);
            }
        }

        if (selectedUnits.Count == 0)
        {
            return;
        }

        OnSelectionConfirmed?.Invoke(selectedUnits, myNode);
        CloseCommandMenu();
    }

    private void HandleButtonClicked(UnitCard card)
    {
        card.ToggleBackground();
    }
}
