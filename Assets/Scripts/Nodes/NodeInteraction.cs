using UnityEngine;
using System;

public class NodeInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MobileInputReader mobileInputReader;
    [SerializeField] private LayerMask nodeLayerMask;
    [SerializeField] private Camera mainCamera;

    public event Action<BuildingNodePreview> OnNodeTapped;

    private void OnEnable()
    {
        mobileInputReader.TapPerformed += HandleTap;
    }

    private void OnDisable()
    {
        mobileInputReader.TapPerformed -= HandleTap;
    }

    private void Awake()
    {
        if (mobileInputReader == null)
        {
            mobileInputReader = FindObjectOfType<MobileInputReader>();
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
