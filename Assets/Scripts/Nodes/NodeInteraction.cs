using UnityEngine;
using System;

public class NodeInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TapOrchestrator tapOrchestrator;
    [SerializeField] private LayerMask nodeLayerMask;
    [SerializeField] private Camera mainCamera;

    public event Action<BuildingNodePreview> OnNodeTapped;

    private void OnEnable()
    {
        tapOrchestrator.CheckNodeTap += HandleTap;
    }

    private void OnDisable()
    {
        tapOrchestrator.CheckNodeTap -= HandleTap;
    }

    private void Awake()
    {
        if (tapOrchestrator == null)
        {
            tapOrchestrator = FindObjectOfType<TapOrchestrator>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void HandleTap(Vector2 touchPosition)
    {
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(touchPosition);

        Collider2D hit = Physics2D.OverlapPoint(worldPosition, nodeLayerMask);

        if (hit == null)
        {
            return;
        }

        BuildingNodePreview nodePreview = hit.GetComponent<BuildingNodePreview>();

        if (nodePreview == null)
        {
            return;
        }

        OnNodeTapped?.Invoke(nodePreview);
    }
}
