using System.Collections;
using UnityEngine;
[System.Serializable] 
public class SerializableRect
{
    [SerializeField] private float width;
    [SerializeField] private float height;
    public Rect AsRect()
    {
        return new Rect(-width, 0, width*2, height);
    }

}
public class AnimationScript : BasicSpriteObject
{
    [System.NonSerialized] public Animator animator;
    [SerializeField] private SerializableRect sRect;
    [System.NonSerialized] public Rect renderRect;
    private void OnDrawGizmos()
    {
        renderRect = sRect.AsRect();
        Gizmos.DrawLine(transform.TransformPoint(renderRect.min), transform.TransformPoint(renderRect.min + new Vector2(0, renderRect.height)));
        Gizmos.DrawLine(transform.TransformPoint(renderRect.min), transform.TransformPoint(renderRect.min + new Vector2(renderRect.width, 0)));
        Gizmos.DrawLine(transform.TransformPoint(renderRect.min), transform.TransformPoint(renderRect.min + new Vector2(0, renderRect.height)));
        Gizmos.DrawLine(transform.TransformPoint(renderRect.max), transform.TransformPoint(renderRect.max - new Vector2(renderRect.width, 0)));
        Gizmos.DrawLine(transform.TransformPoint(renderRect.max), transform.TransformPoint(renderRect.max - new Vector2(0, renderRect.height)));
    }
    public override void Setup()
    {
        base.Setup();
        renderRect = sRect.AsRect();
        animator = GetComponentInChildren<Animator>();
        animator.speed = 1f/Globals.AnimationMultiplier;
    }
    public IEnumerator PlayAndWaitForAnim(string anim, int layer = 0)
    {
        animator.Play(anim);
        yield return WaitForAnim(anim, layer);
    }
    public IEnumerator WaitForAnim(string anim, int layer = 0)
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(layer).IsName(anim));
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(layer).length);
    }
    public IEnumerator WaitAnimStart(string anim)
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(anim));
    }
    public IEnumerator CopySprite(SpriteRenderer spr)
    {
        while (isActiveAndEnabled)
        {
            sprites[0].sprite = spr.sprite;
            yield return Globals.FixedUpdate;
        }
    }
    public T Cast<T>()
    {
        if (this is T)
        {
            return (T)(object)this;
        }
        else
        {
            return default;
        }
    }
}