public class BuildingNode
{
    private int nodeID;
    private int row;
    private int column;

    public BuildingNode(int nodeID, int row, int column)
    {
        this.nodeID = nodeID;
        this.row = row;
        this.column = column;
    }

    public int NodeID => nodeID;
    public int Row => row;
    public int Column => column;
}