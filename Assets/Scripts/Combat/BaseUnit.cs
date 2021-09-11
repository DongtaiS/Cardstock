using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Linq;
public abstract class BasicSpriteObject : MonoBehaviour
{
    [SerializeField] private List<SpriteRenderer> flipSprites = new List<SpriteRenderer>();
    private SortingGroup sortingGroup;
    public float currentAlpha { get; private set; } = 1;
    private protected List<SpriteRenderer> sprites = new List<SpriteRenderer>();
    private protected Transform spriteContainer;
    public virtual void Setup()
    {
        sortingGroup = GetComponentInChildren<SortingGroup>();
        spriteContainer = transform.GetChild(0);
        GetComponentsInChildren(sprites);
    }
    public Bounds GetTotalBounds()
    {
        Bounds bounds = sprites[0].bounds;
        foreach(SpriteRenderer spr in sprites)
        {
            bounds.Encapsulate(spr.bounds.max);
            bounds.Encapsulate(spr.bounds.min);
        }
        return bounds;
    }
    public virtual void SetMaterial(Material inMat)
    {
        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.material = inMat;
        }
    }
    public virtual void SetDirection(int direction)
    {
        Vector3 temp = transform.localEulerAngles; //Changed from spriteContainer to transform
        temp.y = (direction - 1) * 90;
        transform.localEulerAngles = temp;
    }
    public void FlipSprites()
    {
        foreach (SpriteRenderer spr in flipSprites)
        {
            spr.sortingOrder *= -1;
        }
    }
    public virtual void SetColor(Color color)
    {
        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.color = color;
        }
        currentAlpha = color.a;
    }
    public virtual void SetAlpha(float alpha)
    {
        foreach (SpriteRenderer sprite in sprites)
        {
            sprite.color = Globals.ChangeColorAlpha(sprite.color, alpha);
        }
        currentAlpha = alpha;
    }
}
public abstract class BaseUnit : MonoBehaviour
{
    [SerializeField] private protected AssetReferenceGameObject AnimObjectAsset;
    [System.NonSerialized] public AnimationScript AnimObject;
    [System.NonSerialized] public RoomScript CurrentRoom;
    [System.NonSerialized] public SpriteRenderer MainSprite;
    [System.NonSerialized] public int FacingDirection;
    private protected RenderData renderData;
    private AssetReference asset;
    public Vector3Int CellCoord { get; private protected set; }

    private protected Collider2D SpriteCollider;
    public virtual void Setup(RoomScript currentRoom, AssetReference assetRef)
    {
        CurrentRoom = currentRoom;
        AnimObject = AnimObjectAsset.LoadAssetAsync<GameObject>().WaitForCompletion().GetComponent<AnimationScript>();
        renderData = Globals.RTCreator.AddRender(AnimObject, MainSprite, this);
        asset = assetRef;
    }
    public virtual void SetDirection(int direction)
    {
        FacingDirection = direction;
        Vector3 temp = MainSprite.transform.localEulerAngles;
        temp.y = (direction - 1) * 90;
        MainSprite.transform.localEulerAngles = temp;
    }
    public void SetAlpha(float alpha)
    {
        MainSprite.color = Globals.ChangeColorAlpha(MainSprite.color, alpha);
    }
    public virtual void UpdateCellCoord()
    {
        CellCoord = CurrentRoom.GetFloor().WorldToCell(transform.position);
    }
    public void SetCellCoord(Vector3Int newCoord)
    {
        CellCoord = newCoord;
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
        AnimObjectAsset.ReleaseAsset();
        asset.ReleaseInstance(gameObject);
    }
}
public abstract class EventUnit : BaseUnit, IInteractable
{
    public abstract void OnPlayerMove();
    public abstract void OnMouseDown();
    public override void Setup(RoomScript currentRoom, AssetReference assetRef)
    {
        MainSprite = GetComponentInChildren<SpriteRenderer>();
        base.Setup(currentRoom, assetRef);
    }
    public virtual Vector3Int GenSpawnCoord(RoomScript room, int playerDir)
    {
        List<Vector3Int> positions = room.map.Keys.ToList();
        while (positions.Count > 0)
        {
            Vector3Int cell = positions[Globals.random.Next(0, positions.Count)];
            if (room.HasEmptyTile(cell))
            {
                return cell;
            }
        }
        return new Vector3Int(-1, -1, -1);
    }
    public abstract void Interact(RoomScript room);
}
public interface IInteractable
{
    public void Interact(RoomScript room);
}
[System.Serializable]
public class BaseUnitInfo
{
    public List<SpriteRenderer> Sprites;
}

