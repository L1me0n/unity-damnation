using UnityEngine;

public class BuildingNodePreview : MonoBehaviour
{
    private BuildingNode myNode;

    private SpriteRenderer childSpriteRenderer;
    private Color originalColor;

    public BuildingNode Node => myNode;

    private void Awake()
    {
        Transform childTransform = transform.Find("Tile");

        if (childTransform == null)
        {
            return;
            
        }

        childSpriteRenderer = childTransform.GetComponent<SpriteRenderer>();

        originalColor = childSpriteRenderer.color;
    }

    public void Initialize(BuildingNode node)
    {
        myNode = node;
    }

    public void SetBlockedColor()
    {
        childSpriteRenderer.color = new Color(0.1383647f, 0.1009453f, 0.08223559f, 1f); 
    }

    public void HighlightColor()
    {
        childSpriteRenderer.color = new Color(0.6918238f, 0.491016f, 0f, 1f); 
    }

    public void ResetColor()
    {
        childSpriteRenderer.color = originalColor;
    }
}
