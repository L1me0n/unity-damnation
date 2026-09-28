using UnityEngine;

public class NodeSafety : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BuildingNodePreview buildingNodePreview;
    [SerializeField] private bool isSafe = true;

    public bool IsSafe => isSafe;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Zone"))
        {
            isSafe = false;
            buildingNodePreview = GetComponent<BuildingNodePreview>();

            buildingNodePreview.SetDangerColor();
        }
    }
}
