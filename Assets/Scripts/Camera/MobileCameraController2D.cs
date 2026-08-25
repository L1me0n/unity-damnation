using UnityEngine;
using System;

[RequireComponent(typeof(Camera))]
public class MobileCameraController2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MobileInputReader mobileInputReader;

    [Header("Settings")]
    [SerializeField, Min(0f)] private float panSensitivity = 4f;
    [SerializeField, Min(0f)] private float zoomSpeed = 75f;
    [SerializeField, Min(0.1f)] private float minimumSize = 8f;
    [SerializeField, Min(0.1f)] private float maximumSize = 33f;
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Rect worldBounds = new Rect(-32f, -32f, 64f, 64f);

    private Camera cameraComponent;

    private void OnEnable()
    {
        mobileInputReader.PanPerformed += MoveCamera;
        mobileInputReader.ZoomPerformed += ZoomCamera;
    }

    private void OnDisable()
    {
        mobileInputReader.PanPerformed -= MoveCamera;
        mobileInputReader.ZoomPerformed -= ZoomCamera;
    }

    private void Awake()
    {
        if (mobileInputReader == null)
        {
            mobileInputReader = FindObjectOfType<MobileInputReader>();
        }

        cameraComponent = GetComponent<Camera>();
        cameraComponent.orthographic = true;
        if (minimumSize > maximumSize) maximumSize = minimumSize;

        cameraComponent.orthographicSize = maximumSize;

        if (useBounds)
        {
            ClampToBounds();
        }
    }

    private void MoveCamera(Vector2 screenDelta)
    {
        float visibleWorldHeight = cameraComponent.orthographicSize * 2f; 
        float worldUnitsPerPixel = visibleWorldHeight / Screen.height;

        Vector2 worldDelta = screenDelta * worldUnitsPerPixel * panSensitivity;

        transform.position -= (Vector3)worldDelta;

        if (useBounds)
        {
            ClampToBounds();
        }
    }

    private void ZoomCamera(float normalizedZoomDelta, Vector2 pinchCenter)
    {
        Vector3 worldPointBeforeZoom = cameraComponent.ScreenToWorldPoint(pinchCenter);

        cameraComponent.orthographicSize = Mathf.Clamp(
            cameraComponent.orthographicSize - normalizedZoomDelta * zoomSpeed,
            minimumSize,
            maximumSize
        );

        Vector3 worldPointAfterZoom = cameraComponent.ScreenToWorldPoint(pinchCenter);

        Vector3 correction = worldPointBeforeZoom - worldPointAfterZoom;
        correction.z = 0f;

        transform.position += correction;

        if (useBounds)
        {
            ClampToBounds();
        }
    }

    private void ClampToBounds()
    {
        Vector3 position = transform.position;
        float halfHeight = cameraComponent.orthographicSize;
        float halfWidth = halfHeight * cameraComponent.aspect;
        float minX = worldBounds.xMin + halfWidth, maxX = worldBounds.xMax - halfWidth;
        float minY = worldBounds.yMin + halfHeight, maxY = worldBounds.yMax - halfHeight;
        position.x = minX > maxX ? worldBounds.center.x : Mathf.Clamp(position.x, minX, maxX);
        position.y = minY > maxY ? worldBounds.center.y : Mathf.Clamp(position.y, minY, maxY);
        transform.position = position;
    }

    public void SetWorldBounds(Rect value) 
    { 
        worldBounds = value;

        if (useBounds)
        {
            ClampToBounds();
        } 
    }
}
