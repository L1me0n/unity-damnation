using UnityEngine;
using System.Collections.Generic;

public class ZoneBuilder : MonoBehaviour
{
    [Header("Borders")]
    [SerializeField] private GameObject leftBorder;
    [SerializeField] private GameObject rightBorder;
    [SerializeField] private GameObject topBorder;
    [SerializeField] private GameObject bottomBorder;

    [Header("Shrinking Zone")]
    [SerializeField] private GameObject leftShrinking;
    [SerializeField] private GameObject rightShrinking;
    [SerializeField] private GameObject topShrinking;
    [SerializeField] private GameObject bottomShrinking;

    [Header("Border Settings")]
    [SerializeField] private Vector2 zoneFocus = new Vector2(0, 0);
    [SerializeField] private int zoneHalfExtent = 32;
    [SerializeField] private int zoneStage = 0;
    [SerializeField] List<int> stageHalfExtents = new List<int> {32, 24, 20, 16, 12, 8};

    [Header("Shrinking Settings")]
    [SerializeField] private bool isShrinking = false;
    [SerializeField] private float leftDistance;
    [SerializeField] private float rightDistance;
    [SerializeField] private float topDistance;
    [SerializeField] private float bottomDistance;
    [SerializeField] private float leftCenter;
    [SerializeField] private float rightCenter;
    [SerializeField] private float topCenter;
    [SerializeField] private float bottomCenter;
    [SerializeField] private float leftSize = 0f;
    [SerializeField] private float rightSize = 0f;
    [SerializeField] private float topSize = 0f;
    [SerializeField] private float bottomSize = 0f;
    [SerializeField] private int shrinkingStep = 0;
    [SerializeField] private int maxSteps = 4;

    private float leftStart;
    private float rightStart;
    private float topStart;
    private float bottomStart;

    private float leftChunk;
    private float rightChunk;
    private float topChunk;
    private float bottomChunk;

    public bool IsShrinking => isShrinking;

    private void Awake()
    {
        leftCenter = leftShrinking.transform.position.x;
        rightCenter = rightShrinking.transform.position.x;
        topCenter = topShrinking.transform.position.y;
        bottomCenter = bottomShrinking.transform.position.y;

        leftStart = leftShrinking.transform.position.x;
        rightStart = rightShrinking.transform.position.x;
        topStart = topShrinking.transform.position.y;
        bottomStart = bottomShrinking.transform.position.y;
    }

    public void TransitionStage()
    {
        isShrinking = false;

        shrinkingStep = 0;

        zoneHalfExtent = stageHalfExtents[zoneStage];

        zoneStage++;

        BuildBorder();
    }

    public void TransitionShrinking()
    {
        isShrinking = true;

        ShrinkZone();
    }

    private void BuildBorder()
    {
        // First change zone focus to random value
        // Zone focus cannot be on the edges, specifically it has to be withing the inner 20%
        int focusSize = Mathf.CeilToInt(zoneHalfExtent * 2 * 0.2f);

        float x = GenerateValue(focusSize);
        float y = GenerateValue(focusSize);
        
        zoneFocus = new Vector2(
            x, 
            y);

        // New zone size
        zoneHalfExtent = stageHalfExtents[zoneStage];

        ConfigureBorders(zoneFocus, zoneHalfExtent);
    }

    private void ShrinkZone()
    {
        if (shrinkingStep == 0)
        {
            leftDistance = leftBorder.transform.position.x - (leftCenter + leftSize / 2);
            rightDistance = (rightCenter - rightSize / 2) - rightBorder.transform.position.x;
            topDistance = (topCenter - topSize / 2) - topBorder.transform.position.y;
            bottomDistance = bottomBorder.transform.position.y - (bottomCenter + bottomSize / 2);

            leftChunk = leftDistance / maxSteps;
            rightChunk = rightDistance / maxSteps;
            topChunk = topDistance / maxSteps;
            bottomChunk = bottomDistance / maxSteps;
        }

        shrinkingStep++;

        leftCenter += leftChunk / 2;
        rightCenter -= rightChunk / 2;
        topCenter -= topChunk / 2;
        bottomCenter += bottomChunk / 2;

        leftSize = (leftCenter - leftStart) * 2;
        rightSize = (rightStart - rightCenter) * 2;
        topSize = (topStart - topCenter) * 2;
        bottomSize = (bottomCenter - bottomStart) * 2;

        leftShrinking.transform.position = new Vector3(
            leftCenter,
            leftShrinking.transform.position.y,
            leftShrinking.transform.position.z);

        rightShrinking.transform.position = new Vector3(
            rightCenter,
            rightShrinking.transform.position.y,
            rightShrinking.transform.position.z);

        topShrinking.transform.position = new Vector3(
            topShrinking.transform.position.x,
            topCenter,
            topShrinking.transform.position.z);

        bottomShrinking.transform.position = new Vector3(
            bottomShrinking.transform.position.x,
            bottomCenter,
            bottomShrinking.transform.position.z);



        leftShrinking.transform.localScale = new Vector3(
            leftSize,
            leftShrinking.transform.localScale.y,
            leftShrinking.transform.localScale.z);

        rightShrinking.transform.localScale = new Vector3(
            rightSize,
            rightShrinking.transform.localScale.y,
            rightShrinking.transform.localScale.z);

        topShrinking.transform.localScale = new Vector3(
            topShrinking.transform.localScale.x,
            topSize,
            topShrinking.transform.localScale.z);

        bottomShrinking.transform.localScale = new Vector3(
            bottomShrinking.transform.localScale.x,
            bottomSize,
            bottomShrinking.transform.localScale.z);

        if (shrinkingStep == maxSteps)
        {
            isShrinking = false;
        }
    }

    private void ConfigureBorders(Vector2 center, int halfExtent)
    {
        float leftX = Mathf.Max(center.x - halfExtent, leftBorder.transform.position.x);
        float rightX = Mathf.Min(center.x + halfExtent, rightBorder.transform.position.x);
        float topY = Mathf.Min(center.y + halfExtent, topBorder.transform.position.y);
        float bottomY =  Mathf.Max(center.y - halfExtent, bottomBorder.transform.position.y);

        float width = rightX - leftX;
        float height = topY - bottomY;

        float centerX = (rightX + leftX) / 2;
        float centerY = (topY + bottomY) / 2;

        leftBorder.transform.position = new Vector3(
            (float)leftX,
            centerY,
            leftBorder.transform.position.z);

        rightBorder.transform.position = new Vector3(
            (float)rightX,
            centerY,
            rightBorder.transform.position.z);

        topBorder.transform.position = new Vector3(
            centerX,
            (float)topY,
            topBorder.transform.position.z);

        bottomBorder.transform.position = new Vector3(
            centerX,
            (float)bottomY,
            bottomBorder.transform.position.z);



        leftBorder.transform.localScale = new Vector3(
            leftBorder.transform.localScale.x,
            height,
            leftBorder.transform.localScale.z);

        rightBorder.transform.localScale = new Vector3(
            rightBorder.transform.localScale.x,
            height,
            rightBorder.transform.localScale.z);

        topBorder.transform.localScale = new Vector3(
            width,
            topBorder.transform.localScale.y,
            topBorder.transform.localScale.z);

        bottomBorder.transform.localScale = new Vector3(
            width,
            bottomBorder.transform.localScale.y,
            bottomBorder.transform.localScale.z);
    }

    private float GenerateValue(int focusSize)
    {
        return UnityEngine.Random.Range(-focusSize, focusSize + 1);
    }
} 
