using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuffIconScript : MonoBehaviour
{
    public Vector3 TargetPos;
    public int row = 0;
    [SerializeField] public SpriteRenderer sprite;
    [SerializeField] private protected TextMeshPro text;
    private IEnumerator currentFade;
    private IEnumerator currentTranslate;
    public void SetText(string inText)
    {
        text.text = inText;
    }
    public void SetSprite(Sprite s)
    {
        sprite.sprite = s;
    }
    public void SetTextColor(Color color)
    {
        text.color = color;
    }
    public void Enable()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        StartCoroutine(CheckFade(Fade(0f, 1f, 0.5f)));
    }
    public void Disable()
    {
        Disable(0.25f);
    }
    public void Disable(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut(duration));
    }
    public void ResetTargetPos()
    {
        TargetPos = transform.localPosition;
    }
    private IEnumerator CheckFade(IEnumerator inAnim)
    {
        if (currentFade != null)
        {
            StopCoroutine(currentFade);
        }
        currentFade = inAnim;
        yield return StartCoroutine(currentFade);
    }
    public IEnumerator CheckTranslate(Vector3 translation, float duration)
    {
        if (currentTranslate != null)
        {
            StopCoroutine(currentTranslate);
        }
        currentTranslate = Translate(translation, duration);
        yield return StartCoroutine(currentTranslate);
    }
    private IEnumerator Translate(Vector3 translation, float duration)
    {
        TargetPos += translation;
        duration *= Vector3.Distance(transform.localPosition, TargetPos) / 4;
        yield return Globals.InterpVector3(transform.localPosition, TargetPos, duration, result => transform.localPosition = result);
        ResetTargetPos();
    }
    public IEnumerator Fade(float a, float b, float duration)
    {
        yield return Globals.InterpFloat(a, b, duration, result => SetAlphaAll(result));
    }
    private IEnumerator FadeOut(float duration)
    {
        yield return Fade(1f, 0f, duration);
        gameObject.SetActive(false);
    }
    private void SetAlphaAll(float alpha)
    {
        sprite.color = Globals.ChangeColorAlpha(sprite.color, alpha);
        text.color = Globals.ChangeColorAlpha(text.color, alpha);
    }
}
