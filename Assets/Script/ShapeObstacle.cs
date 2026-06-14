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
        float dist = Vector3.Distance(transform.position, JellyController.Instance.transform.position);

        if (dist < 0.5f)
        {
            hasBeenChecked = true;
            
            float currentJellyHeight = JellyController.Instance.CurrentHeight;
            float currentJellyWidth = JellyController.Instance.CurrentWidth;

            // --- СЕНЬЙОРНИЙ БАЛАНС ТОЛЕРАНТНОСТІ ---
            // Збільшуємо запас до 0.350f. Це компенсує пружину желе (коли воно 2.05 замість 2.0)
            // і прощає гравцю, якщо він став, наприклад, 1.9 чи 0.6 через недотискання.
            float tolerance = 0.35f;

            // Перевіряємо, чи втиснувся гравець у габарити дірки з урахуванням похибки пружини
            bool heightFits = currentJellyHeight <= (holeHeight + tolerance);
            bool widthFits = currentJellyWidth <= (holeWidth + tolerance);

            if (heightFits && widthFits)
            {
                Debug.Log("<color=green>SUCCESS!</color>");
                Camera.main.GetComponent<CameraFollow>()?.AddShakeImpulse(0.6f, 0.2f);
            }
            else
            {
                Debug.Log("<color=red>FAIL!</color>");
                Camera.main.GetComponent<CameraFollow>()?.AddShakeImpulse(2.0f, 1.0f);

                // ВИКЛИК МИГАННЯ UI
                ScreenFlash.Instance?.TriggerRedFlash();
            }
        }
    }
}