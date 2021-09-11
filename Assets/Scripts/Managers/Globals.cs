using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cinemachine;
[System.Serializable]
public class GenericListToDictionary<T1, T2>
{
    [System.Serializable]
    public class GenericListItem
    {
        public T1 key;
        public T2 value;
    }
    public List<GenericListItem> list = new List<GenericListItem>();
    public Dictionary<T1, T2> ToDictionary()
    {
        Dictionary<T1, T2> dict = new Dictionary<T1, T2>();
        foreach (GenericListItem item in list)
        {
            dict.Add(item.key, item.value);
        }
        return dict;
    }

}
public class Globals : MonoBehaviour
{
    [SerializeField] private float animationMultiplier;
    public static float AnimationMultiplier;
    public static bool IsGamePaused { get; private set; } = false;
    private static int pausePriority;
    [SerializeField] private AssetReference player;
    public static GameObject GameManager;
    public static PlayerCombatScript PlayerCombat { get; private set; }
    public static GameStateScript GameState { get; private set; }
    public static CameraManagerScript CameraManager { get; private set; }
    public static TurnManagerScript TurnManager { get; private set; }
    public static DeckScript Deck { get; private set; }
    public static PrefabManager PrefabManager { get; private set; }
    public static RoomManagerScript RoomManager { get; private set; }
    public static TraitManagerScript TraitManager { get; private set; }
    public static RaycastManager Raycaster { get; private set; }
    public static RenderTextureCreator RTCreator { get; private set; }
    public static AudioManager AudioManager { get; private set; }
    public static ProjectileScript Projectiles { get; private set; }
    public static GameObject World;
    public static LayerMask CombatableMask { get; private set; }
    public static LayerMask ObjectMask { get; private set; }
    public static LayerMask TilemapGroundMask { get; private set; }
    public static LayerMask TilemapRoomMask { get; private set; }
    public static LayerMask HighlightMask { get; private set; }
    public static LayerMask UnitCollisionMask { get; private set; }
    public static readonly float MOUSETARGETMULTIPLIER = 1f / Mathf.Tan(30f * (Mathf.PI / 180f));
    public static readonly WaitForFixedUpdate FixedUpdate = new WaitForFixedUpdate();
    public static readonly WaitForEndOfFrame EndOfFrame = new WaitForEndOfFrame();
    public static readonly System.Random random = new System.Random();
    public AnimationCurvesSO AnimCurves;
    public static AnimationCurvesSO AnimationCurves;
    private void Awake()
    {
        AnimationMultiplier = animationMultiplier;
        GameManager = gameObject;
        CombatableMask = LayerMask.GetMask(new string[] { "Combatables" });
        ObjectMask = LayerMask.GetMask(new string[] { "Combatables", "Obstacles" });
        TilemapGroundMask = LayerMask.GetMask(new string[] { "Ground" });
        TilemapRoomMask = LayerMask.GetMask(new string[] { "Ground", "Walls" });
        HighlightMask = LayerMask.GetMask(new string[] { "Highlights" });
        UnitCollisionMask = LayerMask.GetMask(new string[] { "UnitCollision" });
        GameState = GetComponent<GameStateScript>();
        CameraManager = GetComponent<CameraManagerScript>();
        TurnManager = GetComponent<TurnManagerScript>();
        Deck = GetComponent<DeckScript>();
        PrefabManager = GetComponent<PrefabManager>();
        RoomManager = GetComponent<RoomManagerScript>();
        TraitManager = GetComponent<TraitManagerScript>();
        Raycaster = GetComponent<RaycastManager>();
        /*        CurrentRoom = Vector3OverlapPoint(Player.transform.position, TilemapGroundMask).GetComponentInParent<RoomScript>();*/
        World = GameObject.FindGameObjectWithTag("World");
        RTCreator = GameObject.FindGameObjectWithTag("RenderTextureCreator").GetComponent<RenderTextureCreator>();
        AudioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        AnimationCurves = AnimCurves;
        PlayerCombat = player.InstantiateAsync(World.transform).WaitForCompletion().GetComponent<PlayerCombatScript>();
    }
    private void Start()
    {
        PlayerCombat.transform.SetParent(RoomManager.GetCurrentRoom().transform);
        PlayerCombat.transform.localPosition = new Vector3(10, 0, 10);
        PlayerCombat.Setup(RoomManager.GetCurrentRoom(), player);
    }
    public static WaitForSeconds WaitForSeconds(float duration)
    {
        return new WaitForSeconds(duration * AnimationMultiplier);
    }
    public static void Pause(int prio) //Pauses game if prio passed in is higher than current priority
    {
        IsGamePaused = true;
        if (prio > pausePriority)
        {
            pausePriority = prio;
            Time.timeScale = 0;
        }
    }
    public static void Unpause(int prio) //Unpauses game if prio passed in is higher than current priority
    {
        if (prio >= pausePriority)
        {
            IsGamePaused = false;
            pausePriority = 0;
            Time.timeScale = 1;
        }
    }
    public static Collider2D Vector3OverlapPoint(Vector3 coord, LayerMask layerMask) //Returns a collider2D at the given world coordinate (
    {
        return Physics2D.OverlapPoint(new Vector2(coord.x, coord.y), layerMask);
    }
    public static Vector3 WorldTransformPoint(Vector3 inCoord) // Converts the global coordinate passed in into a local position of the World object
    {
        return World.transform.InverseTransformPoint(inCoord);
    }
    public static Vector3 Vector3ChangeX(Vector3 original, float newX) // Returns a vector3 with a new x value
    {
        return new Vector3(newX, original.y, original.z);
    }
    public static Vector3 Vector3ChangeY(Vector3 original, float newY) // Returns a vector3 with a new y value
    {
        return new Vector3(original.x, newY, original.z);
    }
    public static Vector3 Vector3ChangeZ(Vector3 original, float newZ) // Returns a vector3 with a new z value
    {
        return new Vector3(original.x, original.y, newZ);
    }
    public static int Vector3ToDir(Vector3Int posA, Vector3Int posB)    // Returns an int representing the direction from posA to posB
    {                                                                   // 0: B is above A, 1: B is to the right of A, 2: B is below A, 3: B is to the left of A
        if (posA != posB)
        {
            if (Mathf.Abs(posA.x - posB.x) > Mathf.Abs(posA.y - posB.y))
            {
                if (posA.x < posB.x)
                {
                    return 1;
                }
                else
                {
                    return 3;
                }
            }
            else
            {
                if (posA.y < posB.y)
                {
                    return 0;
                }
                else
                {
                    return 2;
                }
            }
        }
        Debug.Log("error vector3todir");
        return -1;
    }
    public static bool CheckPerpendicular(Vector3Int posA, Vector3Int posB) //Returns true if the two cell coordinates are not equal but perpendicular
    {
        if (posA != posB)
        {
            if (posA.x == posB.x || posA.y == posB.y)
            {
                return true;
            }
        }
        return false;
    }
    public static int PerpDist(Vector3Int posA, Vector3Int posB) // Returns the sum of the distance in x values and distance in y values of the two cell coordinates
    {
        return Mathf.Abs(posA.x - posB.x) + Mathf.Abs(posA.y - posB.y);
    }
    public static bool InRange(float min, float max, float val) // Returns true if val is between min and max (both inclusive)
    {
        return val >= min && val <= max;
    }
    public static bool CheckPerpRange(Vector3Int posA, Vector3Int posB, int rangeMin, int rangeMax)  //returns true if the player and parameter cell are perpendicular, if they are 
    {                                                                                               //within range, and if there are no directional issues
        if (CheckPerpendicular(posA, posB))
        {
            int dist = (int)Vector3Int.Distance(posA, posB);
            if (InRange(rangeMin, rangeMax, dist))
            {
                return true;
            }
        }
        return false;
    }
    public static int OppositeDirection(int direction) // Returns the opposite int directino of the direction that is passed in
    {
        return direction - 2 * (int)Mathf.Sign(direction.CompareTo(2));
    }
    public static Vector2Int IntDirectionToVector2(int direction)   //Returns a Vector2Int with coordinates that represent the direction relative to (0,0)
    {                                                               //0 = (0,1), 1 = (1,0), 2 = (0,-1), 3 = (-1, 0)
        switch (direction)
        {
            case 0:
                return Vector2Int.up;
            case 1:
                return Vector2Int.right;
            case 2:
                return Vector2Int.down;
            case 3:
                return Vector2Int.left;
        }
        return Vector2Int.zero;
    }
    public static Vector3Int DirectionToWorldVector3(int direction) // Returns a Vector3Int with coordinates that represent the direction relative to (0,0)
    {                                                               // World is isometric, so 0 = (0,0,1), 1 = (1,0,0), 2 = (0,0,-1), 3 = (-1,0,0)
        switch (direction)
        {
            case 0:
                return Vector3Int.forward;
            case 1:
                return Vector3Int.right;
            case 2:
                return Vector3Int.back;
            case 3:
                return Vector3Int.left;
        }
        Debug.Log("Error direction to world vector3");
        return Vector3Int.up;
    }
    public static float DirectionToRotation(int direction)  // returns an angle representing the direction (where down is default)
    {                                                       // 0 = 180, 1 = 90, 2 = 0, 3 = -90
        return (direction - 2) * -90;
    }
    public static Vector3 ScreenToWorld()       // Converts the position of the mouse to a world position
    {
        return CameraPosToWorld(Input.mousePosition);
    }
    public static Vector3 CameraPosToWorld(Vector3 screenPos)
    {
        Plane plane = new Plane(World.transform.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        plane.Raycast(ray, out float enter);
        Debug.DrawLine(Vector3.zero, ray.GetPoint(enter));
        return ray.GetPoint(enter);
    }
    public static void Shuffle<T>(List<T> list) // Randomizes a list
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static Color ChangeColorAlpha(Color inColor, float alpha) // Returns a new color with a new alpha value (same r, g, b values)
    {
        inColor.a = alpha;
        return inColor;
    }
    public static IEnumerator UnscaledInterpVector3(Vector3 posA, Vector3 posB, float duration, Action<Vector3> result) // Coroutine that interpolates a vector3 value, unaffected by timescale (pausing)
    {
        yield return UnscaledInterpVector3(posA, posB, duration, AnimationCurves.IncLinear, result);
    }
    public static IEnumerator UnscaledInterpVector3(Vector3 posA, Vector3 posB, float duration, AnimationCurve curve, Action<Vector3> result) // Coroutine that interpolates a vector3 value, unaffected by timescale (pausing)
    {
        duration *= AnimationMultiplier;
        if (duration == 0)
        {
            Debug.Log("duration of 0");
            yield break;
        }
        float startTime = Time.unscaledTime;
        float t = 0;
        while (t < 1)
        {
            t = curve.Evaluate((Time.unscaledTime - startTime) / duration);
            result(Vector3.Lerp(posA, posB, t));
            if (t >= 1)
            {
                break;
            }
            yield return EndOfFrame;
        }
    }
/*    public static IEnumerator UnscaledInterpVector3(Vector3 posA, Vector3 posB, float duration, AnimationCurve curve, Action<Vector3> result)
    {
        if (duration == 0)
        {
            Debug.Log("duration of 0");
            yield break;
        }
        float startTime = Time.unscaledTime;
        float t = 0;
        while (t < 1)
        {
            t = curve.Evaluate((Time.unscaledTime - startTime) / duration);
            result(Vector3.Lerp(posA, posB, t));
            if (t >= 1)
            {
                break;
            }
            yield return EndOfFrame;
        }
    }*/
    public static IEnumerator InterpVector3(Vector3 posA, Vector3 posB, float duration, Action<Vector3> result) // Coroutine that interpolates a vector3 value
    {
        yield return InterpVector3(posA, posB, duration, AnimationCurves.IncLinear, result);
    }
    public static IEnumerator InterpVector3(Vector3 posA, Vector3 posB, float duration, AnimationCurve curve, Action<Vector3> result) // Coroutine that interpolates a vector3 value using an animation curve
    {
        duration *= AnimationMultiplier;
        if (duration == 0)
        {
            Debug.Log("duration of 0");
            yield break;
        }
        float startTime = Time.time;
        float t = 0;
        while (t < 1)
        {
            t = curve.Evaluate((Time.time - startTime) / duration);
            result(Vector3.Lerp(posA, posB, t));
            if (t >= 1)
            {
                break;
            }
            yield return FixedUpdate;
        }
    }
    public static IEnumerator UnscaledInterpFloat(float a, float b, float duration, bool smoothStep, Action<float> result) // Coroutine that interpolates a float value, unaffected by timescale
    {
        duration *= AnimationMultiplier;
        if (duration == 0)
        {
            yield break;
        }
        float startTime = Time.unscaledTime;
        float t = 0;
        while (t < 1)
        {
            t = (Time.unscaledTime - startTime) / duration;
            if (smoothStep)
            {
                t = Mathf.SmoothStep(0, 1, t);
            }
            result(Mathf.Lerp(a, b, t));
            if (t >= 1)
            {
                break;
            }
            yield return EndOfFrame;
        }
    }

    public static IEnumerator InterpFloat(float a, float b, float duration, Action<float> result) // Coroutine that interpolates a vector3 value
    {
        yield return InterpFloat(a, b, duration, AnimationCurves.IncLinear, result);
    }
    public static IEnumerator InterpFloat(float a, float b, float duration, AnimationCurve curve, Action<float> result) 
    {
        duration *= AnimationMultiplier;
        if (duration == 0)
        {
            yield break;
        }
        float startTime = Time.time;
        float t = 0;
        while (t < 1)
        {
            t = curve.Evaluate((Time.time - startTime) / duration);
            result(Mathf.Lerp(a, b, t));
            if (t >= 1)
            {
                break;
            }
            yield return FixedUpdate;
        }
    }
    public static IEnumerator InterpAngle(float a, float b, float duration, bool smoothStep, Action<float> result) // Coroutine that interpolates an angle (deals with 360 -> 0 correctly)
    {
        duration *= AnimationMultiplier;
        if (duration == 0)
        {
            yield break;
        }
        float startTime = Time.time;
        float t = 0;
        while (t < 1)
        {
            t = (Time.time - startTime) / duration;
            if (smoothStep)
            {
                t = Mathf.SmoothStep(0, 1, t);
            }
            result(Mathf.LerpAngle(a, b, t));
            if (t >= 1)
            {
                break;
            }
            yield return FixedUpdate;
        }
    }
    public static bool InCellRadius(Vector3Int baseCoord, Vector3Int compareCoord, float radius) // Returns true if the compare cell is within the radius of the base cell
    {
        int xDist = Mathf.Abs(baseCoord.x - compareCoord.x);
        int yDist = Mathf.Abs(baseCoord.y - compareCoord.y);
        if (RoundToNearestInt(radius) != Mathf.FloorToInt(radius))
        {
            return xDist + yDist <= Mathf.CeilToInt(radius);
        }
        else
        {
            return xDist <= radius && yDist <= radius && xDist + yDist <= (int)radius + 1;
        }
    }
    public static List<Vector3Int> GetCellsInRadius(Vector3Int baseCell, float radius, bool includeBase = false) //Overload, returns a list of cells in a radius around the base cell
    {
        List<Vector3Int> result = new List<Vector3Int>();
        return GetCellsInRadius(result, baseCell, radius, includeBase);
    }
    private static List<Vector3Int> GetCellsInRadius(List<Vector3Int> result, Vector3Int baseCell, float radius, bool includeBase) // Recursively adds cells to the list, from the outside of the radius inwards
    {
        float nextR = radius;
        if (radius < 0.5f)
        {
            if (includeBase)
            {
                result.Add(baseCell);
            }
            return result;
        }
        int add = 0;
        if (RoundToNearestInt(radius) != Mathf.Floor(radius))
        {
            radius = Mathf.CeilToInt(radius);
        }
        else
        {
            radius = (int)radius;
            add = 1;
        }
        for (int x = 0; x <= radius; x++)
        {
            for (int y = 0; y <= radius + add - x; y++)
            {
                if (y <= radius && (y + x == radius + add) || (y + x == radius))
                {
                    if (x > 0)
                    {
                        result.Add(new Vector3Int(baseCell.x + x, baseCell.y + y, 0));
                        result.Add(new Vector3Int(baseCell.x - x, baseCell.y - y, 0));
                    }
                    if (y > 0)
                    {
                        result.Add(new Vector3Int(baseCell.x + x, baseCell.y - y, 0));
                        result.Add(new Vector3Int(baseCell.x - x, baseCell.y + y, 0));
                    }
                }
            }
        }
        return GetCellsInRadius(result, baseCell, nextR - 1f, includeBase);
    }
    public static int GetCellCountOfRadius(float radius, bool includeCenter)
    {
        int cellCount = 0;
        if (includeCenter)
        {
            cellCount++;
        }
        if (radius % 1 == 0.5f)
        {
            cellCount += 4;
        }
        cellCount += (int)radius * 4;
        return cellCount;
    }
    public static int RadialKnockbackDir(Vector3Int posA, Vector3Int posB, int bFaceDir)    // Returns an int direction representing the direction a unit (posB) is knocked back relative to posA
    {                                                                                       // If posA and posB are both not perpendicular and equal distance away in both axes, it uses the
        int dir;                                                                            // direction that is perpendicular to posB's direction and away from posA
        Vector3Int diff = posB - posA;
        if (CheckPerpendicular(posA, posB) || Math.Abs(diff.x) != Mathf.Abs(diff.y))
        {
            dir = Vector3ToDir(posA, posB);
        }
        else
        {
            if (diff.x > 0)
            {
                if (diff.y > 0)
                {
                    dir = 1 - bFaceDir % 2;
                }
                else
                {
                    dir = 1 + bFaceDir % 2;
                }
            }
            else
            {
                if (diff.y > 0)
                {
                    dir = 3 * (1 - bFaceDir % 2);
                }
                else
                {
                    dir = 3 - bFaceDir % 2;
                }
            }
        }
        return dir;
    }
    public static List<Vector3Int> GetCellsInLine(Vector3Int start, Vector3Int end, bool includeStart, bool includeEnd) // Returns a list of cells in a line from start to end
    {
        List<Vector3Int> result = new List<Vector3Int>();
        if (CheckPerpendicular(start, end))
        {
            int i = includeStart ? 0 : 1;
            int dist = (int)Vector3Int.Distance(start, end);
            Vector3Int increment = (end - start) / dist;
            int limit = includeEnd ? dist : dist - 1;
            for (; i <= limit; i++)
            {
                result.Add(start + increment * i);
            }
        }
        return result;
    }
    private protected List<Vector3Int> GetCellsInLineCentered(Vector3Int center, int offset, float r, int dir)  // Returns a symmetrical list of cells in a line centered at center,
    {                                                                                                           // dir represent horizontal or vertical (even = vertical, odd = horizontal)
        List<Vector3Int> result = new List<Vector3Int>();
        for (int dist = offset; dist <= r; dist++)
        {
            if (dist == 0)
            {
                result.Add(center);
            }
            else
            {
                result.Add(center + (Vector3Int)(IntDirectionToVector2(dir) * dist));
                result.Add(center - (Vector3Int)(IntDirectionToVector2(dir) * dist));
            }
        }
        return result;
    }
    public static int RoundToNearestInt(float value) // Rounds float to nearest int where decimals >= 0.5 round up and < 0.5 round down
    {
        if (value - Mathf.Floor(value) >= 0.5)
        {
            return Mathf.CeilToInt(value);
        }
        else
        {
            return Mathf.FloorToInt(value);
        }
    }
    public static bool CompareLists(List<CardAttackData> a, List<CardAttackData> b) // Returns true if the two lists are equal
    {
        if (a != null && b != null && a.Count == b.Count)
        {
            Dictionary<CardAttackData, int> counts = new Dictionary<CardAttackData, int>();
            foreach (CardAttackData t in a)
            {
                if (!counts.ContainsKey(t))
                {
                    counts.Add(t, 1);
                }
                else
                {
                    counts[t]++;
                }
            }
            foreach (CardAttackData t in b)
            {
                if (!counts.ContainsKey(t))
                {
                    Debug.Log("can't find");
                    return false;
                }
                else
                {
                    counts[t]--;
                }
            }
            foreach (int count in counts.Values)
            {
                if (count != 0)
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }
    public static IEnumerator YieldThenDestroy(IEnumerator yieldTo, GameObject GO) //Coroutine that executes the coroutine passed in, then destroys the gameobject
    {
        yield return yieldTo;
        Destroy(GO);
    }
    public static IEnumerator CheckAnim(IEnumerator currentAnim, IEnumerator inAnim, MonoBehaviour GO)  //Coroutine that stops the current coroutine if applicable, then starts the new coroutine. 
    {                                                                                                   //This method call is assigned to the variable passed in as currrentAnim:
        if (currentAnim != null)                                                                        //Ex. currentFadeAnimation = CheckAnim(currentFadeAnimation, Fade(), this);
        {
            GO.StopCoroutine(currentAnim);
        }
        currentAnim = inAnim;
        yield return GO.StartCoroutine(currentAnim);
    }
}