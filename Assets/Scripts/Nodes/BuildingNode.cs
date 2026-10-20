using System.Collections.Generic;

public class BuildingNode
{
    private int nodeID;
    private int row;
    private int column;
    private bool isDeploymentBlocked;
    private int ownerTeam = -1;
    private List<UnitManager> occupyingUnits = new List<UnitManager>();

    public int NodeID => nodeID;
    public int Row => row;
    public int Column => column;
    public int OwnerTeam => ownerTeam;
    public IReadOnlyList<UnitManager> OccupyingUnits => occupyingUnits;

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

    public void SetOwnerTeam(int teamId)
    {
        ownerTeam = teamId;
    }

    public void AddOccupant(UnitManager unit)
    {
        if (occupyingUnits.Contains(unit))
        {
            return;
        }

        occupyingUnits.Add(unit);
    }

    public void RemoveOccupant(UnitManager unit)
    {
        if (!occupyingUnits.Contains(unit))
        {
            return;
        }

        occupyingUnits.Remove(unit);
    }
}