using UnityEngine;
using System.Collections.Generic;

public class MatchManager : MonoBehaviour
{
    [Header("Units")]
    [SerializeField] private List<UnitManager> allUnits = new List<UnitManager>();

    private int id = 0;

    public IReadOnlyList<UnitManager> AllUnits => allUnits;

    public void RegisterUnit(UnitManager unit, int teamID)
    {
        if (!allUnits.Contains(unit))
        {
            allUnits.Add(unit);
            unit.SetTeamID(teamID);
            unit.SetUnitID(id);
            id++;
        }
    }
}
