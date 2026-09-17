using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class VerticalSwipeReader : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("References")]
    [SerializeField] private RectTransform movingPanel;
    [SerializeField] private Canvas canvas;

    [Header("Settings")]
    [SerializeField, Range(0.1f, 0.5f)]
    private float swipeThreshold = 0.25f;

    [SerializeField] private float maximumTravelMultiplier = 1f;

    private Vector2 pointerStartPosition;
    private Vector2 panelStartPosition;

    private int activePointerID = int.MinValue;

    public event Action SwipeUpPerformed;
    public event Action SwipeDownPerformed;

    private void Awake()
    {
        if (movingPanel == null)
        {
            movingPanel = transform as RectTransform;
        }

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Prevent a second finger from controlling the same panel.
        if (activePointerID != int.MinValue)
        {
            return;
        }

        activePointerID = eventData.pointerId;
        pointerStartPosition = eventData.position;
        panelStartPosition = movingPanel.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerID)
        {
            return;
        }

        Vector2 pointerDifference =
            eventData.position - pointerStartPosition;

        float canvasScale = canvas != null
            ? canvas.scaleFactor
            : 1f;

        float verticalMovement =
            pointerDifference.y / canvasScale;

        float maximumTravel =
            movingPanel.rect.height * maximumTravelMultiplier;

        verticalMovement = Mathf.Clamp(
            verticalMovement,
            -maximumTravel,
            maximumTravel
        );

        movingPanel.anchoredPosition = new Vector2(
            panelStartPosition.x,
            panelStartPosition.y + verticalMovement
        );
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerID)
        {
            return;
        }

        Vector2 pointerDifference =
            eventData.position - pointerStartPosition;

        float verticalDistance =
            movingPanel.anchoredPosition.y - panelStartPosition.y;

        float requiredDistance =
            movingPanel.rect.height * swipeThreshold;

        bool mainlyVertical =
            Mathf.Abs(pointerDifference.y) >
            Mathf.Abs(pointerDifference.x);

        // Reset before firing the event. This ensures the panel
        // starts correctly next time even if the event closes it.
        movingPanel.anchoredPosition = panelStartPosition;
        activePointerID = int.MinValue;

        if (!mainlyVertical)
        {
            return;
        }

        if (verticalDistance >= requiredDistance)
        {
            SwipeUpPerformed?.Invoke();
        }
        else if (verticalDistance <= -requiredDistance)
        {
            SwipeDownPerformed?.Invoke();
        }
    }

    private void OnDisable()
    {
        // Handles the panel being closed halfway through a drag.
        if (activePointerID != int.MinValue)
        {
            movingPanel.anchoredPosition = panelStartPosition;
            activePointerID = int.MinValue;
        }
    }
}