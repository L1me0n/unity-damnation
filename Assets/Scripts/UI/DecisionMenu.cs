using UnityEngine;
using System.Collections.Generic;
using System;

public class DecisionMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VerticalSwipeReader verticalSwipeReader;
    [SerializeField] private NodeInteraction nodeInteraction;
    [SerializeField] private GameObject decisionMenuPanel;
    
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

        decisionMenuPanel.SetActive(false);
    }

    private void OnSwipeUp()
    {
        ConfirmDecision();
    }

    private void OnSwipeDown()
    {
        CloseDecisionMenu();
    }

    private void OnNodeTapped(BuildingNodePreview nodePreview)
    {
        if (nodePreview.IsPreviewed && !nodePreview.IsOccupied && !nodePreview.IsOriginal)
        {
            OpenDecisionMenu(nodePreview);
        }
    }

    private void OpenDecisionMenu(BuildingNodePreview nodePreview)
    {
        decisionMenuPanel.SetActive(true);
        isMenuOpen = true;

        foreach(Transform card in unitCards)
        {
            card.gameObject.SetActive(false);
        }

        int i = 0;
        List<UnitManager> playerUnits = nodePreview.GetPreviewedUnits(); 
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

    private void CloseDecisionMenu()
    {
        decisionMenuPanel.SetActive(false);
        isMenuOpen = false;

        foreach (Transform card in unitCards)
        {
            UnitCard unitCard = card.GetComponent<UnitCard>();
            unitCard.OnButtonClicked -= HandleButtonClicked;

            card.gameObject.SetActive(false);
        }
    }

    private void ConfirmDecision()
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
            if (!unitCard.IsSelected)
            {
                selectedUnits.Add(unitCard.MyUnit);
            }
        }

        if (selectedUnits.Count == 0)
        {
            return;
        }

        OnSelectionConfirmed?.Invoke(selectedUnits, myNode);
        CloseDecisionMenu();
    }

    private void HandleButtonClicked(UnitCard card)
    {
        card.ToggleBackground();
    }
}
