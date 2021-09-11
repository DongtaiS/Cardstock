using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
public enum AnimationObjectType
{
    Sword, Spear, QuakeRock1, QuakeRock2, Broadsword
}
public class WeaponAnimations
{
    public enum Sword { SwordSwing, SwordSwingSlow, SwordSwingComplete, SwordSpin, SwordSpinSlow, SwordSpinComplete, Idle, Empty}
    public enum Spear { Thrust, ThrustSlow, ThrustComplete }
    public enum Broadsword { GreatCleave, GreatCleaveSlow, OverheadSwing, OverheadSwingSlow}
    public static IEnumerator ExtendSpear(AnimationScript spear, int cellDist, float durationPerCell = 0.1f)
    {
        SpriteRenderer spr = spear.GetComponentInChildren<SpriteRenderer>();
        Vector2 originalSize = spr.size;
        Vector2 newSize = spr.size;
        newSize.x += Globals.PlayerCombat.CurrentRoom.GetFloor().cellSize.x * cellDist;
        yield return Globals.InterpVector3(spr.size, newSize, durationPerCell * cellDist, result => { spr.size = result; spr.transform.localPosition= new Vector3(result.x, 0); });
        yield return Globals.InterpVector3(spr.size, originalSize, durationPerCell * cellDist * 3, result => { spr.size = result; spr.transform.localPosition = new Vector3(result.x, 0); });
    }
}
public class PrefabManager : MonoBehaviour
{
    [SerializeField] private AnimationScript BasicPreviewObj;
    [SerializeField] private GenericListToDictionary<AnimationObjectType, AssetReference> AnimationObjectList = new GenericListToDictionary<AnimationObjectType, AssetReference>();
    [SerializeField] private GenericListToDictionary<ProjectileTypes, AssetReference> ProjectileList = new GenericListToDictionary<ProjectileTypes, AssetReference>();
    [SerializeField] private GenericListToDictionary<BuffType, Sprite> BuffIconList = new GenericListToDictionary<BuffType, Sprite>();
    
    public GameObject BuffIconPrefab;
    public Dictionary<BuffType, Sprite> BuffIcons = new Dictionary<BuffType, Sprite>();

    private Dictionary<AnimationObjectType, AssetReference> animationObjects = new Dictionary<AnimationObjectType, AssetReference>();
    private Dictionary<ProjectileTypes, AssetReference> projectiles = new Dictionary<ProjectileTypes, AssetReference>();
    [System.Serializable]
    public class Pool
    {
        public ObjectPool ObjectType;
        public GameObject Prefab;
        public int Size;
    }

    public enum ObjectPool { Highlight, CardHighlight }
    public List<Pool> Pools = new List<Pool>();
    private Dictionary<ObjectPool, Queue<GameObject>> objectDictionary = new Dictionary<ObjectPool, Queue<GameObject>>();
    private Dictionary<GameObject, GameObject> parentDictionary = new Dictionary<GameObject, GameObject>();
    private GameObject container;

    void Awake()
    {
        animationObjects = AnimationObjectList.ToDictionary();
        BuffIcons = BuffIconList.ToDictionary();
        projectiles = ProjectileList.ToDictionary();
        container = new GameObject("ObjectPool");
        foreach (Pool pool in Pools)
        {
            GameObject childContainer = new GameObject(pool.Prefab.ToString());
            childContainer.transform.SetParent(container.transform);
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.Size; i++)
            {
                GameObject pooledObj = Instantiate(pool.Prefab, childContainer.transform);
                pooledObj.SetActive(false);
                objectPool.Enqueue(pooledObj);
            }
            objectDictionary.Add(pool.ObjectType, objectPool);
            parentDictionary.Add(pool.Prefab, childContainer);
        }
    }
    public GameObject SpawnObjectFromPool(ObjectPool objectType, Vector3 position)
    {
        GameObject SpawnedObject = objectDictionary[objectType].Dequeue();
        SpawnedObject.transform.position = position;
        SpawnedObject.SetActive(true);
        objectDictionary[objectType].Enqueue(SpawnedObject);
        return SpawnedObject;
    }
    public T SpawnObjectFromPool<T>(ObjectPool objectType, Transform parent, Vector3 position)
    {
        GameObject SpawnedObject = objectDictionary[objectType].Dequeue();
        if (SpawnedObject.TryGetComponent<T>(out T obj))
        {
            SpawnedObject.transform.SetParent(parent, false);
            SpawnedObject.transform.position = position;
            SpawnedObject.SetActive(true);
            objectDictionary[objectType].Enqueue(SpawnedObject);
            return obj;
        }
        else
        {
            return default;
        }
    }
    public AnimationScript CreatePreviewObj(CombatScript combatable)
    {
        AnimationScript preview = Instantiate(BasicPreviewObj, combatable.CurrentRoom.transform);
        preview.Setup();
        preview.StartCoroutine(preview.CopySprite(combatable.MainSprite));
        return preview;
    }
    public AnimationScript SpawnAnimationObject(AnimationObjectType type, Vector3 position, int dir, Transform parent)
    {
        if (animationObjects.TryGetValue(type, out AssetReference obj))
        {
            AnimationScript temp = obj.InstantiateAsync(parent).WaitForCompletion().GetComponent<AnimationScript>();
            temp.transform.position = position;
            temp.Setup();
            temp.SetDirection(dir);
            return temp;
        }
        return default(AnimationScript);
    }
    public ProjectileScript SpawnProjectile(ProjectileTypes type, Transform parent)
    {
        if (projectiles.TryGetValue(type, out AssetReference obj))
        {
            ProjectileScript temp = obj.InstantiateAsync(parent).WaitForCompletion().GetComponent<ProjectileScript>();
            temp.Setup();
            return temp;
        }
        return default(ProjectileScript);
    }
}
