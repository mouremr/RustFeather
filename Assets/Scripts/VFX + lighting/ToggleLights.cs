using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ToggleLights : MonoBehaviour
{
    [SerializeField] private Light2D globalLight;
    [SerializeField] private GameObject interiorLights;
    [SerializeField] private GameObject exteriorLights;

    [Header("Global light Presets")]
    [SerializeField] private Color exteriorColor = Color.white;
    [SerializeField] private float exteriorIntensity = 1f;
    [SerializeField] private Color interiorColor = new Color(0.6f, 0.6f, 0.75f);
    [SerializeField] private float interiorIntensity = 0.35f;

    [SerializeField] private float fadeDuration = 0.5f;

    private bool isInside;
    private Coroutine fadeRoutine;

    void Awake()
    {
        globalLight.intensity = exteriorIntensity;
        interiorLights.SetActive(false);
    }

    public void toggleLights()
    {
        if(gameObject.activeSelf)
            SetInterior(!isInside);
    }

    public void SetInterior(bool inside)
    {
        isInside = inside;

        // Local (non-global) lights don't have the one-global-light restriction,
        // so they can just switch immediately.
        if (interiorLights != null) interiorLights.SetActive(inside);
        if (exteriorLights != null) exteriorLights.SetActive(!inside);

        Color targetColor = inside ? interiorColor : exteriorColor;
        float targetIntensity = inside ? interiorIntensity : exteriorIntensity;

        // Stop any fade in progress; the new one starts from wherever the light currently is
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Fade(targetColor, targetIntensity));
    }

    private IEnumerator Fade(Color targetColor, float targetIntensity)
    {
        Color startColor = globalLight.color;
        float startIntensity = globalLight.intensity;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(fadeDuration, 0.0001f);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            globalLight.color = Color.Lerp(startColor, targetColor, eased);
            globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, eased);
            yield return null;
        }

        globalLight.color = targetColor;
        globalLight.intensity = targetIntensity;
        fadeRoutine = null;
    }
}