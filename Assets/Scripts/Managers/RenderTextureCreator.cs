using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;
public class RenderData
{
    public AnimationScript AnimObj;
    public SpriteRenderer SpriteRenderer;
    public Rect Rect;
    public Sprite Sprite;
    public RenderData(AnimationScript animObj, SpriteRenderer spr, Rect rect)
    {
        AnimObj = animObj;
        SpriteRenderer = spr;
        Rect = rect;
        Sprite = null;
    }
}
public class RenderTextureCreator : MonoBehaviour
{
    [SerializeField] private Transform renderParent;
    private RenderTexture renderTexture;
    private Camera cam;
    private Texture2D dest;
    private List<RenderData> renderedObjects = new List<RenderData>();
    public RenderData AddRender(AnimationScript animObj, SpriteRenderer spr, BaseUnit unit)
    {
        RenderData data = new RenderData(animObj, spr, new Rect());
        data.AnimObj = Instantiate(animObj, renderParent);
        data.AnimObj.Setup();
        unit.AnimObject = data.AnimObj;
        data.Rect = data.AnimObj.renderRect;
        if (data.Rect.size == Vector2.zero)
        {
            Debug.Log(data.Rect);
            Bounds bounds = data.AnimObj.GetTotalBounds();
            data.Rect = new Rect(0, 0, bounds.size.x * 1.75f, bounds.size.y * 1.25f);
        }
        else
        {
            data.Rect.x += data.Rect.width / 2f;
        }
        if (renderedObjects.Count > 0)
        {
            data.Rect.x += renderedObjects[renderedObjects.Count - 1].Rect.x;
            data.Rect.x += renderedObjects[renderedObjects.Count - 1].Rect.width;
            if (data.Rect.x + data.Rect.width > cam.orthographicSize*2)
            {
                //next row
            }
        }
        data.AnimObj.transform.localPosition = new Vector2(data.Rect.center.x, 0);
        Debug.DrawLine(renderParent.TransformPoint(data.AnimObj.transform.localPosition), renderParent.TransformPoint(data.AnimObj.transform.localPosition + new Vector3(0, data.Rect.height)), Color.red, 120f);
        Debug.DrawLine(renderParent.TransformPoint(data.Rect.min), renderParent.TransformPoint(data.Rect.min + new Vector2(data.Rect.width, 0)), Color.red, 120f);
        Debug.DrawLine(renderParent.TransformPoint(data.Rect.min), renderParent.TransformPoint(data.Rect.min + new Vector2(0, data.Rect.height)), Color.red, 120f);
        Debug.DrawLine(renderParent.TransformPoint(data.Rect.max), renderParent.TransformPoint(data.Rect.max - new Vector2(data.Rect.width, 0)), Color.red, 120f);
        Debug.DrawLine(renderParent.TransformPoint(data.Rect.max), renderParent.TransformPoint(data.Rect.max - new Vector2(0, data.Rect.height)), Color.red, 120f);
        renderedObjects.Add(data);
        return data;
    }
    public void RemoveRenderObject(RenderData data)
    {
        if (renderedObjects.Contains(data))
        {
            renderedObjects.Remove(data);
            Destroy(data.AnimObj.gameObject);
            UpdatePositions();
        }
    }
    private void UpdatePositions()
    {
        for (int i = 0; i < renderedObjects.Count; i++)
        {
            RenderData data = renderedObjects[i];
            data.Rect.x = 0;
            if (i > 0)
            {
                data.Rect.x += renderedObjects[i - 1].Rect.x;
                data.Rect.x += renderedObjects[i - 1].Rect.width;
                if (data.Rect.x + data.Rect.width > cam.orthographicSize * 2)
                {
                    Debug.Log("next row");
                }
            }
            data.AnimObj.transform.localPosition = new Vector2(data.Rect.center.x, 0);
/*            renderedObjects[i] = data;*/
        }
    }
    private void Render()
    {
        float divisor = (cam.orthographicSize * 2);
        cam.Render();
        dest.Apply(true);
        Graphics.ConvertTexture(renderTexture, dest);
        for (int i = 0; i < renderedObjects.Count; i++)
        {
            RenderData data = renderedObjects[i];
            Destroy(data.Sprite);
            Rect newRect = new Rect(data.Rect);
            newRect.size *= renderTexture.width / divisor;
            newRect.position *= renderTexture.width / divisor;
            data.Sprite = Sprite.Create(dest, newRect, new Vector2(0.5f, 0f), renderTexture.width / divisor, 0, SpriteMeshType.FullRect);
            data.SpriteRenderer.sprite = data.Sprite;
        }
    }
    private void Start()
    {
        cam = GetComponent<Camera>();
        renderTexture = cam.targetTexture;
        dest = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
    }
    private void FixedUpdate()
    {
        Render();
    }
}
