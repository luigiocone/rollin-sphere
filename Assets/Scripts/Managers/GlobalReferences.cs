using UnityEngine;

public class GlobalReferences : MonoBehaviour
{
    public GameObject Player;
    public GameObject Camera;

    [Header("Managers")]
    public InputManager InputManager;
    public GameFlowManager GameFlowManager;
    public EnemyManager EnemyManager;
    public ToastManager ToastManager;
    public CursorManager CursorManager;
    public MenuManager MenuManager;
    public AudioManager AudioManager;
}

