using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

using InputTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using InputTouchPhase = UnityEngine.InputSystem.TouchPhase;

[DisallowMultipleComponent]
public class MobileInputReader : MonoBehaviour
{
    [Header("Tap Recognition")]
    [SerializeField, Min(0f)] private float maximumTapDuration = 0.3f;
    [SerializeField, Min(0f)] private float dragThresholdInches = 0.08f;
    [SerializeField, Min(0f)] private float fallbackDragThresholdPixels = 20f;

    [Header("Editor Testing")]
    [SerializeField, Min(0f)] private float mouseWheelZoomSensitivity = 0.001f;

    // All positions and pan deltas are in screen pixels.
    // Zoom delta is normalized by screen height. Positive means zoom in.
    public event Action<Vector2> TapPerformed;
    public event Action<Vector2> PanPerformed;
    public event Action<float, Vector2> ZoomPerformed;

    private enum SingleTouchState
    {
        None,
        TapCandidate,
        Panning
    }

    private SingleTouchState singleTouchState;

    private Vector2 touchStartPosition;
    private Vector2 previousTouchPosition;
    private float touchStartTime;

    private bool isPinching;
    private float previousPinchDistance;
    private bool ignoreTouchesUntilRelease;

#if UNITY_EDITOR || UNITY_STANDALONE
    private Vector2 mouseStartPosition;
    private Vector2 previousMousePosition;
    private float mouseStartTime;
    private bool isMousePressed;
    private bool isMousePanning;
    private bool ignoreMouseUntilRelease;
#endif

    private float DragThresholdPixels
    {
        get
        {
            if (Screen.dpi > 0f)
            {
                return dragThresholdInches * Screen.dpi;
            }

            return fallbackDragThresholdPixels;
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
        ResetReader();
    }

    private void Update()
    {
        int touchCount = InputTouch.activeTouches.Count;
        ReadTouches(touchCount);

        #if UNITY_EDITOR || UNITY_STANDALONE
            // A real or Unity Remote touch takes priority over the mouse fallback.
            if (touchCount == 0)
            {
                ReadMouse();
            }
        #endif
    }

    private void ReadTouches(int touchCount)
    {
        if (touchCount == 0)
        {
            ResetTouchGesture();
            ignoreTouchesUntilRelease = false;
            return;
        }

        if (ignoreTouchesUntilRelease)
        {
            return;
        }

        if (touchCount > 2 || AnyNewTouchStartedOverUI(touchCount))
        {
            ResetTouchGesture();
            ignoreTouchesUntilRelease = true;
            return;
        }

        if (touchCount == 2)
        {
            ReadPinch(InputTouch.activeTouches[0], InputTouch.activeTouches[1]);
            return;
        }

        // Do not turn the remaining finger of a completed pinch into a pan/tap.
        if (isPinching)
        {
            ResetTouchGesture();
            ignoreTouchesUntilRelease = true;
            return;
        }

        ReadSingleTouch(InputTouch.activeTouches[0]);
    }

    private void ReadSingleTouch(InputTouch touch)
    {
        Vector2 position = touch.screenPosition;

        if (touch.phase == InputTouchPhase.Began)
        {
            touchStartPosition = position;
            previousTouchPosition = position;
            touchStartTime = Time.unscaledTime;
            singleTouchState = SingleTouchState.TapCandidate;
            return;
        }

        if (touch.phase == InputTouchPhase.Canceled)
        {
            ResetTouchGesture();
            return;
        }

        if (touch.phase == InputTouchPhase.Ended)
        {
            FinishSingleTouch(position);
            return;
        }

        if (singleTouchState == SingleTouchState.None)
        {
            return;
        }

        float threshold = DragThresholdPixels;
        bool passedDragThreshold = (position - touchStartPosition).sqrMagnitude >= threshold * threshold;

        if (singleTouchState == SingleTouchState.TapCandidate && passedDragThreshold)
        {
            singleTouchState = SingleTouchState.Panning;
        }

        Vector2 screenDelta = position - previousTouchPosition;

        if (singleTouchState == SingleTouchState.Panning && screenDelta != Vector2.zero)
        {
            PanPerformed?.Invoke(screenDelta);
        }

        previousTouchPosition = position;
    }

    private void FinishSingleTouch(Vector2 endPosition)
    {
        float threshold = DragThresholdPixels;
        bool stayedInsideTapArea = (endPosition - touchStartPosition).sqrMagnitude < threshold * threshold;
        bool endedInTime = Time.unscaledTime - touchStartTime <= maximumTapDuration;

        if (singleTouchState == SingleTouchState.TapCandidate && stayedInsideTapArea && endedInTime)
        {
            TapPerformed?.Invoke(endPosition);
        }

        ResetTouchGesture();
    }

    private void ReadPinch(InputTouch firstTouch, InputTouch secondTouch)
    {
        if (TouchHasEnded(firstTouch) || TouchHasEnded(secondTouch))
        {
            ResetTouchGesture();
            ignoreTouchesUntilRelease = true;
            return;
        }

        Vector2 firstPosition = firstTouch.screenPosition;
        Vector2 secondPosition = secondTouch.screenPosition;
        float currentDistance = Vector2.Distance(firstPosition, secondPosition);

        // First frame establishes a baseline; there is no delta yet.
        if (!isPinching)
        {
            isPinching = true;
            singleTouchState = SingleTouchState.None;
            previousPinchDistance = currentDistance;
            return;
        }

        float screenHeight = Mathf.Max(1f, Screen.height);
        float normalizedZoomDelta = (currentDistance - previousPinchDistance) / screenHeight;
        Vector2 pinchCenter = (firstPosition + secondPosition) * 0.5f;

        if (!Mathf.Approximately(normalizedZoomDelta, 0f))
        {
            ZoomPerformed?.Invoke(normalizedZoomDelta, pinchCenter);
        }

        previousPinchDistance = currentDistance;
    }

    private bool AnyNewTouchStartedOverUI(int touchCount)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        for (int i = 0; i < touchCount; i++)
        {
            InputTouch touch = InputTouch.activeTouches[i];

            if (touch.phase == InputTouchPhase.Began && EventSystem.current.IsPointerOverGameObject(touch.touchId))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TouchHasEnded(InputTouch touch)
    {
        return touch.phase == InputTouchPhase.Ended ||
               touch.phase == InputTouchPhase.Canceled;
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    private void ReadMouse()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
        {
            return;
        }

        Vector2 position = mouse.position.ReadValue();

        if (mouse.leftButton.wasPressedThisFrame)
        {
            ignoreMouseUntilRelease = IsMouseOverUI();
            isMousePressed = true;
            isMousePanning = false;
            mouseStartPosition = position;
            previousMousePosition = position;
            mouseStartTime = Time.unscaledTime;
        }

        if (isMousePressed && mouse.leftButton.isPressed && !ignoreMouseUntilRelease)
        {
            float threshold = DragThresholdPixels;

            if (!isMousePanning &&
                (position - mouseStartPosition).sqrMagnitude >= threshold * threshold)
            {
                isMousePanning = true;
            }

            Vector2 screenDelta = position - previousMousePosition;

            if (isMousePanning && screenDelta != Vector2.zero)
            {
                PanPerformed?.Invoke(screenDelta);
            }

            previousMousePosition = position;
        }

        if (mouse.leftButton.wasReleasedThisFrame && isMousePressed)
        {
            if (!ignoreMouseUntilRelease && !isMousePanning &&
                Time.unscaledTime - mouseStartTime <= maximumTapDuration)
            {
                TapPerformed?.Invoke(position);
            }

            isMousePressed = false;
            isMousePanning = false;
            ignoreMouseUntilRelease = false;
        }

        Vector2 scroll = mouse.scroll.ReadValue();

        if (!Mathf.Approximately(scroll.y, 0f) && !IsMouseOverUI())
        {
            ZoomPerformed?.Invoke(
                scroll.y * mouseWheelZoomSensitivity,
                position
            );
        }
    }

    private static bool IsMouseOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }
#endif

    private void ResetTouchGesture()
    {
        singleTouchState = SingleTouchState.None;
        isPinching = false;
        previousPinchDistance = 0f;
    }

    private void ResetReader()
    {
        ResetTouchGesture();
        ignoreTouchesUntilRelease = false;

#if UNITY_EDITOR || UNITY_STANDALONE
        isMousePressed = false;
        isMousePanning = false;
        ignoreMouseUntilRelease = false;
#endif
    }
}