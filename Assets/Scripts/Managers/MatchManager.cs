using UnityEngine;
using System.Collections.Generic;

public class MatchManager : MonoBehaviour
{
    [Header("Units")]
    [SerializeField] private List<UnitManager> allUnits = new List<UnitManager>();

    private int id = 0;

    public IReadOnlyList<UnitManager> AllUnits => allUnits;

    private void OnDisable()
    {
        foreach (UnitManager unit in allUnits)
        {
            unit.OnDeath -= HandleDeath;
        }

        allUnits.Clear();
    }

    private void Start()
    {
        BeginMatch(); // Temporarily begin match on game start
    }

    public void BeginMatch()
    {
        MessageMenu.Instance.DisplayMessage("Battle starts! Please choose drop location");
    }

    public void RegisterUnit(UnitManager unit, int teamID)
    {
        if (!allUnits.Contains(unit))
        {
            allUnits.Add(unit);
            unit.OnDeath += HandleDeath;
            unit.SetTeamID(teamID);
            unit.SetUnitID(id);
            id++;
        }
    }

    private void HandleDeath(UnitManager unit)
    {
        allUnits.Remove(unit);
    }
}
