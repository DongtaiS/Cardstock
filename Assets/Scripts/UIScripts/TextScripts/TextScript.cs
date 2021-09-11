using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class TextScript : MonoBehaviour
{
    [SerializeField] private protected TextMeshProUGUI tmp;
    public delegate void PointerEnterDelegate(RectTransform rect);
    public delegate void PointerExitDelegate(RectTransform rect);
    [System.NonSerialized] public PointerEnterDelegate PointerEnter;
    [System.NonSerialized] public PointerExitDelegate PointerExit;
    [System.NonSerialized] public TooltipScript tooltip;
    public TextScript CreateTextScript(Transform parent, Vector3 localPosition)
    {
        TextScript text = Instantiate(this, parent);
        text.transform.localPosition = localPosition;
        return text;
    }
    public virtual void SetAlpha(float alpha)
    {
        tmp.alpha = alpha;
    }
    public void SetColor(Color color)
    {
        tmp.color = color;
    }
    public Color GetColor()
    {
        return tmp.color;
    }
    public void SetText(string inText)
    {
        tmp.text = inText;
    }
    public IEnumerator AnimateColor(Color newColor, float duration)
    {
        float progress = 0;
        Color original = tmp.color;
        StartCoroutine(Globals.InterpFloat(0, 1, duration, t => progress = t));
        while (progress < 1)
        {
            SetColor(Color.Lerp(original, newColor, progress));
            yield return Globals.FixedUpdate;
        }
        tmp.color = Color.Lerp(original, newColor, 1);
    }
    public IEnumerator FlashColor(Color color, float duration)
    {
        Color original = tmp.color;
        yield return AnimateColor(color, duration/2f);
        yield return AnimateColor(original, duration/2f);
    }
    public void OnPointerEnter()
    {
        if (tooltip != null)
        {
            StartCoroutine(tooltip.CheckFade(tooltip.FadeCurrent(1, 0.1f)));
        }
    }
    public void OnPointerExit()
    {
        if (tooltip != null)
        {
            StartCoroutine(tooltip.CheckFade(tooltip.FadeCurrent(0, 0.1f)));
        }
    }
}
