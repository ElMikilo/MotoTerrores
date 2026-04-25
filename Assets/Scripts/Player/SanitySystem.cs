using UnityEngine;
using UnityEngine.UI;

public class SanitySystem : MonoBehaviour
{
    [SerializeField] private Slider sanitySlider;
    [SerializeField] private float maxSanity = 100f;

    [Header("UI Smoothing")]
    [SerializeField] private bool smoothUI = true;
    [SerializeField, Range(0.05f, 0.35f)] private float uiSmoothTime = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioSource deathSfxSource;
    [SerializeField, Range(0f, 1f)] private float deathSfxVolume = 1f;

    float displaySanity;
    float displayVel;
    private float currentSanity;
    private bool hasDepleted;

    private void Awake()
    {
        currentSanity = maxSanity;
        displaySanity = currentSanity;

        if (sanitySlider)
        {
            sanitySlider.wholeNumbers = false;
            sanitySlider.maxValue = maxSanity;
            sanitySlider.value = displaySanity;
        }

        if (deathSfxSource)
        {
            deathSfxSource.playOnAwake = false;
            deathSfxSource.loop = false;
        }
    }

    private void Update()
    {
        if (!sanitySlider) return;

        if (smoothUI)
        {
            displaySanity = Mathf.SmoothDamp(
                displaySanity, currentSanity, ref displayVel,
                Mathf.Max(0.01f, uiSmoothTime)
            );
            sanitySlider.value = displaySanity;
        }
        else
        {
            sanitySlider.value = currentSanity;
        }
    }

    public void TakeDamage(float amount)
    {
        if (hasDepleted) return;
        currentSanity = Mathf.Clamp(currentSanity - amount, 0f, maxSanity);

        if (!smoothUI && sanitySlider) sanitySlider.value = currentSanity;

        if (currentSanity <= 0f)
        {
            hasDepleted = true;
            if (deathClip)
            {
                if (deathSfxSource) deathSfxSource.PlayOneShot(deathClip, deathSfxVolume);
                else AudioSource.PlayClipAtPoint(deathClip, transform.position, deathSfxVolume);
            }
            LivesSystem.I?.LoseLife();
        }
    }

    public void RestoreFull()
    {
        currentSanity = maxSanity;
        if (!smoothUI && sanitySlider) sanitySlider.value = currentSanity;
        hasDepleted = false;
        displaySanity = currentSanity;
        displayVel = 0f;
    }

    public float GetSanity()
    {
        return currentSanity;
    }

    public float GetSanityPercent()
    {
        return currentSanity / maxSanity;
    }
}
