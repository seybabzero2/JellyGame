using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
    public static ScreenFlash Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] private Image flashPanel;

    [Header("Flash Settings")]
    [SerializeField] private float fadeDuration = 0.15f; // Швидкість одного "вспливу"
    [SerializeField] private float maxAlpha = 0.5f;      // Наскільки сильно червоніє екран (від 0 до 1)

    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        // Про всяк випадок ховаємо панель на старті
        if (flashPanel != null)
        {
            Color c = flashPanel.color;
            c.a = 0f;
            flashPanel.color = c;
        }
    }

    // Головний публічний метод, який ми будемо викликати
    public void TriggerRedFlash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        Color color = flashPanel.color;

        // Мигаємо суворо 3 рази
        for (int i = 0; i < 3; i++)
        {
            float elapsed = 0f;

            // 1. Плавне поява червоного (Fade In)
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(0f, maxAlpha, elapsed / fadeDuration);
                flashPanel.color = color;
                yield return null;
            }

            elapsed = 0f;

            // 2. Плавне зникнення червоного (Fade Out)
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(maxAlpha, 0f, elapsed / fadeDuration);
                flashPanel.color = color;
                yield return null;
            }
            
            // Легка пауза між миганнями для кращого візуалу
            yield return new WaitForSeconds(0.05f);
        }

        // Перестраховка: в кінці зануляємо альфу
        color.a = 0f;
        flashPanel.color = color;
    }
}