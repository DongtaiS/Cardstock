using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class TooltipScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI desc;
    [SerializeField] private Image background;
    private IEnumerator currentFade;
    private RectTransform rectTransform;
    public TooltipScript CreateTooltip(Vector3 localPosition, Transform parent)
    {
        TooltipScript tooltip = Instantiate(this, parent);
        tooltip.transform.localPosition = localPosition;
        tooltip.SetAlpha(0);
        tooltip.rectTransform = tooltip.GetComponent<RectTransform>();
        return tooltip;
    }
    public void SetTitleText(string text)
    {
        title.text = text;
    }
    public void SetDescText(string text)
    {
        desc.text = text;
        StartCoroutine(EditSize());
    }
    private IEnumerator EditSize()
    {
        yield return Globals.FixedUpdate;
        while (rectTransform.rect.width / rectTransform.rect.height < 1.75f)
        {
            Vector2 temp = rectTransform.sizeDelta;
            temp.x += 10;
            rectTransform.sizeDelta = temp;
        }
    }
    public IEnumerator CheckFade(IEnumerator newFade)
    {
        currentFade = Globals.CheckAnim(currentFade, newFade, this);
        yield return currentFade;
    }
    public IEnumerator Fade(float a, float b, float baseDuration)
    {
/*        float duration = baseDuration * Mathf.InverseLerp(1-b, b, title.alpha);*/
        yield return Globals.InterpFloat(a, b, baseDuration, val => SetAlpha(val));
    }
    public IEnumerator FadeCurrent(float b, float duration)
    {
        yield return Fade(title.alpha, b, duration);
    }
    private void SetAlpha(float alpha)
    {
        title.color = Globals.ChangeColorAlpha(title.color, alpha);
        desc.color = Globals.ChangeColorAlpha(desc.color, alpha);
        background.color = Globals.ChangeColorAlpha(background.color, alpha);
    }
}
