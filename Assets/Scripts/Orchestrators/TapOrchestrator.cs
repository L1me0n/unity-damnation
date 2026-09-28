using UnityEngine;
using System;

public class TapOrchestrator : MonoBehaviour
{
    [SerializeField] private MobileInputReader mobileInputReader;

    public event Action CheckUITap;
    public event Action<Vector2> CheckNodeTap;

    private void OnEnable()
    {
        mobileInputReader.TapPerformed += Orchestrate;
    }

    private void OnDisable()
    {
        mobileInputReader.TapPerformed -= Orchestrate;
    }

    private void Awake()
    {
        if (mobileInputReader == null)
        {
            mobileInputReader = FindObjectOfType<MobileInputReader>();
        }
    }

    private void Orchestrate(Vector2 touchPosition)
    {
        bool uiOwnedThisTap = MessageMenu.Instance.IsMenuOpen;

        if (uiOwnedThisTap)
        {
            CheckUITap?.Invoke();
            return;
        }
        
        CheckNodeTap?.Invoke(touchPosition);
    }
}
