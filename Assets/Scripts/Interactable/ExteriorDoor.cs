using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ExteriorDoor : Interactible
{
    [SerializeField] private Transform exteriors;
    [SerializeField] float insideAlpha = 0.25f;
    [SerializeField] float fadeDuration = 1f;
    private Tilemap[] exteriorTilemaps;
    private SpriteRenderer[] exteriorSprites;
    private bool inside = false;

    private Coroutine fadeRoutine;

    void Awake()
    {
        exteriorTilemaps = exteriors.GetComponentsInChildren<Tilemap>();
        exteriorSprites = exteriors.GetComponentsInChildren<SpriteRenderer>();
        fadeRoutine = StartCoroutine(FadeExteriors(1f));
    }

    public override void Interact()
    {
        //Debug.Log("Turning down Alpha");
        inside = !inside;
        float targetAlpha = inside ? insideAlpha : 1f;
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeExteriors(targetAlpha));

    }
    private IEnumerator FadeExteriors(float targetAlpha)
    {
        // grab current alpha as the starting point (assumes all tilemaps share alpha)
        float startAlpha = exteriorTilemaps.Length > 0 ? exteriorTilemaps[0].color.a : 1f;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);

            foreach (Tilemap tm in exteriorTilemaps)
            {
                Color c = tm.color;
                c.a = a;
                tm.color = c;
            }

            foreach (SpriteRenderer sr in exteriorSprites)
            {
                Color c = sr.color;
                c.a = a;
                sr.color = c;
            }

            yield return null;
        }

        // snap to exact target in case of float imprecision
        foreach (Tilemap tm in exteriorTilemaps)
        {
            Color c = tm.color;
            c.a = targetAlpha;
            tm.color = c;
        }

        foreach (SpriteRenderer sr in exteriorSprites)
        {
            Color c = sr.color;
            c.a = targetAlpha;
            sr.color = c;
        }
    }
    

}