using UnityEngine;

public class ShapeObstacle : BaseObstacle
{
    [HideInInspector] public float holeHeight;
    [HideInInspector] public float holeWidth;

    private bool hasBeenChecked = false;

    private void OnEnable()
    {
        hasBeenChecked = false;
    }

    protected override void Update()
    {
        base.Update();
        if (!hasBeenChecked) CheckPassage();
    }

    private void CheckPassage()
    {
        float playerZ = JellyController.Instance.transform.position.z;
        if (Mathf.Abs(transform.position.z - playerZ) < 0.5f)
        {
            hasBeenChecked = true;
            float currentJellyHeight = JellyController.Instance.CurrentHeight;
            float currentJellyWidth = JellyController.Instance.CurrentWidth;
            float tolerance = 0.2f;

            if (currentJellyHeight <= holeHeight + tolerance && currentJellyWidth <= holeWidth + tolerance)
            {
                Debug.Log("<color=green>SUCCESS!</color>");
            }
            else
            {
                Debug.Log("<color=red>FAIL!</color>");
            }
        }
    }
}