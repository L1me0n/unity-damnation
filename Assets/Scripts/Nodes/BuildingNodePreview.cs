using UnityEngine;

public class BuildingNodePreview : MonoBehaviour
{
    private BuildingNode myNode;

    public BuildingNode Node => myNode;

    public void Initialize(BuildingNode node)
    {
        myNode = node;
    }
}
