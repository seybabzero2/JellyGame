using UnityEngine;

public class TurnObstacle : BaseObstacle
{
    public bool isLeftTurn; 
    private bool hasTurned = false;

    private void OnEnable()
    {
        hasTurned = false;
    }

    protected override void Update()
    {
        base.Update();
        if (!hasTurned) CheckTurnInput();
    }

    private void CheckTurnInput()
    {
        float dist = Vector3.Distance(transform.position, JellyController.Instance.transform.position);

        if (dist < 8f && dist > 1f)
        {
            if (UnityEngine.InputSystem.Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                TryExecuteTurn(true);
            }
            else if (UnityEngine.InputSystem.Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                TryExecuteTurn(false);
            }
        }
        
        Vector3 directionToPlayer = JellyController.Instance.transform.position - transform.position;
        Vector3 movementDirection = -LevelManager.Instance.currentDirection;

        bool isBehind = Vector3.Dot(directionToPlayer, movementDirection) < 0;

        if (isBehind && dist > 2f && !hasTurned)
        {
            Debug.Log("<color=red>Missed Turn! Splat!</color>");
            hasTurned = true; 
        }
    }

private void TryExecuteTurn(bool inputLeft)
{
    if (inputLeft == isLeftTurn)
    {
        Debug.Log("<color=yellow>PERFECT TURN!</color>");
        hasTurned = true;
        
        LevelManager.Instance.ExecuteWorldTurn(isLeftTurn, this);
    }
    else
    {
        Debug.Log("<color=red>WRONG WAY!</color>");
        hasTurned = true; 
        Camera.main.GetComponent<CameraFollow>()?.AddShakeImpulse(2f, 0.5f);

        // ВИКЛИК МИГАННЯ UI
        ScreenFlash.Instance?.TriggerRedFlash();
    }
}
}