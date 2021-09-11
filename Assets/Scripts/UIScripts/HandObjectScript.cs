using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class HandObjectScript : MonoBehaviour
{
    public bool HandOpen { get; private set; }
    public GameObject CardContainer;

    [SerializeField] private float duration = 0.25f;

    [System.NonSerialized] public RectTransform RectTransform;
    IEnumerator currentAnim;
    public IEnumerator OpenHand()
    {
        HandOpen = true;
        yield return CheckAnim(AnimateHand(transform.position.y, 0f, duration));
    }
    public IEnumerator CloseHand()
    {
        HandOpen = false;
        yield return CheckAnim(AnimateHand(transform.position.y, -1080 / 2f, duration));
    }

    public IEnumerator AnimateHand(float pointA, float pointB, float d)
    {
        float startTime = Time.time;
        float t = 0;
        while (t < 1)
        {
            t = (Time.time - startTime) / d;
            transform.position = new Vector2(transform.position.x, Mathf.Lerp(pointA, pointB, t));
            if (t >= 1)
            {
                break;
            }
            yield return Globals.FixedUpdate;
        }
    }
    public void UpdateSize()
    {
        RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, UIManagerScript.Canvas.GetComponentInParent<RectTransform>().rect.width * 2 / 3);
        RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, UIManagerScript.Canvas.GetComponentInParent<RectTransform>().rect.height / 4);
    }
    private void Start()
    {
        RectTransform = GetComponent<RectTransform>();
    }
    private IEnumerator CheckAnim(IEnumerator inAnim)
    {
        if (currentAnim != null)
        {
            StopCoroutine(currentAnim);
        }
        currentAnim = inAnim;
        yield return StartCoroutine(currentAnim);
    }
}
