using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public class HighlightScript : MonoBehaviour
{
    public Color CurrentColor;
    public Vector3Int cellCoord { get; private set; }
    public bool IsFading;
    [SerializeField] private protected SpriteRenderer backL;
    [SerializeField] private protected SpriteRenderer backR;
    [SerializeField] private protected SpriteRenderer frontL;
    [SerializeField] private protected SpriteRenderer frontR;
    public void SetColor(Color newColor)
    {
        CurrentColor = newColor;
        ChangeColor(newColor);
    }
    public void ChangeColor(Color newColor)
    {
        backL.color = newColor;
        backR.color = newColor;
        frontL.color = newColor;
        frontR.color = newColor;
    }
    public void SetAlpha(float a)
    {
        ChangeColor(Globals.ChangeColorAlpha(CurrentColor, a));
    }
    public void SetCellCoord(Vector3Int newCoord)
    {
        cellCoord = newCoord;
    }
    public void Enable()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(FadeIn(0.1f));
    }
    private protected IEnumerator Fade(float a, float b, float duration)
    {
        IsFading = true;
        yield return Globals.InterpFloat(a, b, duration, alpha => SetAlpha(alpha));
        IsFading = false;
    }
    public IEnumerator FadeIn(float duration)
    {
        yield return Fade(0, CurrentColor.a, duration);
    }
    public IEnumerator FadeOut(float duration)
    {
        yield return Fade(CurrentColor.a, 0, duration);
        gameObject.SetActive(false);
    }
    private protected void OnDisable()
    {
        StopAllCoroutines();
    }
}
