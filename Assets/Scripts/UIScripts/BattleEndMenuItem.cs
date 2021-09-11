using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public abstract class BattleEndMenuItem : MonoBehaviour
{
    [SerializeField] private protected SerializedMenuInfo Info;
    private protected Image mainImage;
    private protected Image highlightImage;
    private protected Color HighlightBase;
    private protected Color HighlightHover;
    private protected Color FadeColor1;
    private protected Color FadeColor2;
    private protected Material mainImageMaterial;
    private protected BattleEndMenu battleEndMenu;
    private protected IEnumerator highlightAnim;
    public abstract void OnClick();
    public virtual void OnEnter()
    {
        StartCoroutine(ChangeHighlightColor(HighlightHover, 0.25f));
    }
    public virtual void OnExit()
    {
        StartCoroutine(ChangeHighlightColor(HighlightBase, 0.25f));
    }
    public virtual void Setup()
    {
        battleEndMenu = UIManagerScript.battleEndMenu;
        mainImage = Info.mainImage;
        highlightImage = Info.highlightImage;
        HighlightBase = Info.HighlightBase;
        HighlightHover = Info.HighlightHover;
        highlightImage.color = HighlightBase;
        FadeColor1 = Info.FadeColor1;
        FadeColor2 = Info.FadeColor2;
        mainImageMaterial = Info.mainImageMaterial;
        mainImage.material = Instantiate(mainImageMaterial);
        mainImage.materialForRendering.SetColor("FadeColor1", FadeColor1);
        mainImage.materialForRendering.SetColor("FadeColor2", FadeColor2);
    }
    public IEnumerator ChangeHighlightColor(Color newColor, float duration)
    {
        Vector4 startColor = highlightImage.color;
        Vector4 color = newColor;
        float startTime = Time.unscaledTime;
        float t = 0;
        StartCoroutine(Globals.UnscaledInterpFloat(0, 1, duration, false, val => t = val));
        while (t <= 1)
        {
            highlightImage.color = Vector4.Lerp(startColor, color, t);
            if (t >= 1)
            {
                break;
            }
            yield return Globals.EndOfFrame;
        }
    }
    private protected IEnumerator FadeOut(float duration)
    {
        GetComponent<EventTrigger>().enabled = false;
        battleEndMenu.RemoveMenuItem(this);
        float val = 0;
        StartCoroutine(Globals.UnscaledInterpFloat(0, 1, duration, false, result => val = result));
        while (val < 1)
        {
            mainImage.materialForRendering.SetFloat("Progress", val);
            highlightImage.color = Globals.ChangeColorAlpha(highlightImage.color, 1 - val);
            yield return Globals.EndOfFrame;
        }
        battleEndMenu.Fan();
        StopAllCoroutines();
        Destroy(gameObject);
    }
    private protected void CheckHighlightAnim(IEnumerator inAnim)
    {
        if (highlightAnim != null)
        {
            StopCoroutine(highlightAnim);
        }
        StartCoroutine(inAnim);
    }
}
