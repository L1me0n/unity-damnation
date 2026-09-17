using UnityEngine;
using System;

public class TurnManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int currentTurn = 0;

    public event Action OnTurnEnded;

    public void EndTurn()
    {
        currentTurn++;
        
        OnTurnEnded?.Invoke();
    }
}
