public class BuildingNode
{
    private int nodeID;
    private int row;
    private int column;
    private bool isDeploymentBlocked;

    public int NodeID => nodeID;
    public int Row => row;
    public int Column => column;

    public bool IsDeploymentBlocked => isDeploymentBlocked;

    public BuildingNode(int nodeID, int row, int column)
    {
        this.nodeID = nodeID;
        this.row = row;
        this.column = column;
    }

    public void BlockDeployment()
    {
        isDeploymentBlocked = true;
    }

    public void UnblockDeployment()
    {
        isDeploymentBlocked = false;
    }
}