using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.AddressableAssets;
using System.Linq;
using UnityEngine.ResourceManagement.AsyncOperations;

public class RoomManagerScript : MonoBehaviour
{
    [SerializeField] private List<AssetReference> allRooms = new List<AssetReference>();
    [SerializeField] private List<AssetReference> allDoors = new List<AssetReference>();
    [SerializeField] private List<AssetReference> allEventUnits = new List<AssetReference>();
    [SerializeField] private EnemySets enemies;

    private Dictionary<EnemySetEnum, List<List<AssetReference>>> enemySets = new Dictionary<EnemySetEnum, List<List<AssetReference>>>(); //Maybe change int to enum
    private Dictionary<EnemySets.BossSets, List<List<AssetReference>>> bossSets = new Dictionary<EnemySets.BossSets, List<List<AssetReference>>>();
    private Dictionary<int, EnemySets.BossSets> bossesByLevel = new Dictionary<int, EnemySets.BossSets>(); //Choose from list to get a boss for current level
    private List<List<AssetReference>> eventSets = new List<List<AssetReference>>();

    [SerializeField] private List<TileBase> allTiles = new List<TileBase>();
    [SerializeField] private List<TileBase> altTiles = new List<TileBase>();
    private Dictionary<TileBase, TileBase> tileToAlt = new Dictionary<TileBase, TileBase>();
    private Dictionary<TileBase, TileBase> altToTile = new Dictionary<TileBase, TileBase>();

    private Dictionary<Vector2Int, RoomScript> map = new Dictionary<Vector2Int, RoomScript>();
    private int currentDepth = 0;
    public Vector2Int CurrentMapCoord;
    public EnemySets.BossSets currentBoss;

    private void Awake()
    {
        CurrentMapCoord = new Vector2Int(0, 0);
        for (int i = 0; i < altTiles.Count; i++)
        {
            tileToAlt.Add(allTiles[i], altTiles[i]);
            altToTile.Add(altTiles[i], allTiles[i]);
        }
        CreateRoom(CurrentMapCoord, 0, new Vector3(-10, 0, -10), 0);
        currentBoss = EnemySets.BossSets.Twins;
        GenerateRooms();
        GenerateRooms();
        map[CurrentMapCoord].ShowRoom();
        foreach (EnemySets.EnemySet set in enemies.EnemySetList)
        {
            foreach (EnemySetEnum setType in set.enemySetTypes)
            {
                if (!enemySets.ContainsKey(setType))
                {
                    enemySets.Add(setType, new List<List<AssetReference>>());
                }
                enemySets[setType].Add(set.enemies);
            }
        }

        eventSets.Add(new List<AssetReference> { allEventUnits[0] });
        eventSets.Add(new List<AssetReference> { allEventUnits[1] });
        /*        SpawnEnemies(map[new Vector2Int(0, 1)], 2, 1);
                SpawnEnemies(map[new Vector2Int(1, 0)], 3, 1);
                SpawnEnemies(map[new Vector2Int(0, -1)], 0, 1);
                SpawnEnemies(map[new Vector2Int(-1, 0)], 1, 1);*/

    }
    public void SetCurrentMapCoord(Vector2Int newCoord)
    {
        CurrentMapCoord = newCoord;
    }
    public TileBase GetTileToAlt(TileBase inTile)
    {
        return tileToAlt[inTile];
    }
    public TileBase GetAltToTile(TileBase inTile)
    {
        return altToTile[inTile];
    }
    public Dictionary<Vector2Int, RoomScript> GetMap()
    {
        return map;
    }
    public RoomScript GetCurrentRoom()
    {
        return map[CurrentMapCoord];
    }
    public void GenerateRooms()
    {
        List<RoomScript> rooms = map.Values.ToList();
        for (int i = 0; i < rooms.Count; i++)
        {
            RoomScript room = rooms[i];
            if (room.Depth == currentDepth)
            {
                List<int> possibleDirs = new List<int>();
                for (int d = 0; d < 4; d++)
                {
                    Vector2Int tempCoord = room.mapCoord + Globals.IntDirectionToVector2(d);
                    if (room.GetTransformConnection(d) != null && (!map.ContainsKey(tempCoord) || map[tempCoord].Depth == currentDepth + 1))
                    {
                        possibleDirs.Add(d);
                    }
                }
                do
                {
                    int dir = possibleDirs[Globals.random.Next(0, possibleDirs.Count)];
                    Vector2Int tempCoord = room.mapCoord + Globals.IntDirectionToVector2(dir);
                    if (map.ContainsKey(tempCoord))
                    {
                        if (map[tempCoord].Depth == currentDepth + 1 && map[tempCoord].GetTransformConnection(dir) != null)
                        {
                            map[tempCoord].AddRoomConnection(room, dir);
                        }
                    }
                    else 
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            int index = Globals.random.Next(0, allRooms.Count);
                            GameObject go = (GameObject)allRooms[index].editorAsset;
                            if (go.GetComponent<RoomScript>().GetTransformConnection(Globals.OppositeDirection(dir)) != null)
                            {
                                CreateRoom(room.mapCoord, index, dir, currentDepth + 1);
                                break;
                            }
                        }
                    }
                    possibleDirs.Remove(dir);
                } while (currentDepth == 0 && possibleDirs.Count > 2 || Globals.random.Next(0, 2) > 0 && possibleDirs.Count > 0);
            }
        }
        currentDepth++;
    }
    public void CreateRoom(Vector2Int baseCoord, int roomIndex, int direction, int depth)
    {
        RoomScript baseRoom = map[baseCoord];
        Vector2Int newCoord = baseCoord + Globals.IntDirectionToVector2(direction);
        if (baseRoom == null)
        {
            return;
        }
        RoomScript newRoom = allRooms[roomIndex].InstantiateAsync(Globals.World.transform).WaitForCompletion().GetComponent<RoomScript>();
        newRoom.transform.localPosition = Globals.WorldTransformPoint(baseRoom.GetTransformConnection(direction).position) - newRoom.GetTransformConnection(Globals.OppositeDirection(direction)).localPosition;
        newRoom.Setup(this, newCoord, depth, GenerateRoomType(depth));
        map[newCoord] = newRoom;
        newRoom.AddRoomConnection(baseRoom, Globals.OppositeDirection(direction));
        baseRoom.AddRoomConnection(newRoom, direction);
        newRoom.HideRoom();
    }
    public void CreateRoom(Vector2Int baseCoord, int roomIndex, Vector3 position, int depth)
    {
        RoomScript newRoom = allRooms[roomIndex].InstantiateAsync(Globals.World.transform).WaitForCompletion().GetComponent<RoomScript>();
        newRoom.transform.localPosition = position;
        newRoom.Setup(this, baseCoord, depth, RoomTypes.Event);
        map[baseCoord] = newRoom;
        newRoom.HideRoom();
    }
    public void CreateDoor(RoomScript room, int dir)
    {
        DoorScript door = allDoors[0].InstantiateAsync(room.transform).WaitForCompletion().GetComponent<DoorScript>();
        door.Setup(dir);
        door.transform.localPosition = room.GetTransformConnection(dir).localPosition;
        door.transform.localEulerAngles = new Vector3(0, dir % 2 * 90);
        room.AddDoor(door, dir);
    }
    public void SpawnEnemySet(RoomScript room, int playerDir, EnemySetEnum setType)
    {
        foreach (AssetReference enemy in enemySets[setType][Globals.random.Next(0, enemySets[setType].Count)]) //Globals.random.Next(0, enemySets[setType].Count)
        {
            SpawnEnemy(room, enemy, playerDir);
        }
    }
    public void SpawnBoss(RoomScript room, int playerDir)
    {
        switch (currentBoss)
        {
            case EnemySets.BossSets.Twins:
                TwinBlue twinBlue = (TwinBlue)SpawnEnemy(room, enemies.TwinBlue, playerDir);
                TwinRed twinRed = (TwinRed)SpawnEnemy(room, enemies.TwinRed, playerDir);
                twinBlue.twinRed = twinRed;
                twinRed.twinBlue = twinBlue;
                break;
        }
    }
    private T AssetReferenceInstantiate<T> (AssetReference asset)
    {
        return asset.LoadAssetAsync<GameObject>().WaitForCompletion().GetComponent<T>();
    }
    private EnemyScript SpawnEnemy(RoomScript room, AssetReference enemy, int playerDir)
    {
        EnemyScript e = enemy.InstantiateAsync(room.transform).WaitForCompletion().GetComponent<EnemyScript>();
        e.Setup(room, enemy);
        e.transform.position = room.GetFloor().GetCellCenterWorld(e.GenSpawnCoord(room, playerDir));
        e.UpdateCellCoord();
        e.SetDirection(room.playerDirection);
        room.SetInMap(e.CellCoord, e.gameObject);
        StartCoroutine(Globals.InterpFloat(0, 1, 1f, a => e.SetAlpha(a)));
        return e;
    }
    private RoomTypes GenerateRoomType(int depth)
    {
        if (depth == 2)
        {
            return RoomTypes.Boss;
        }
        else if (Globals.random.Next(0, 3) == 0)//one in 3
        {
            return RoomTypes.Event;
        }
        else
        {
            return RoomTypes.Combat;
        }
    }
    public void SpawnEvent(RoomScript room, int playerDir)
    {
        List<AssetReference> eventUnits = eventSets[Globals.random.Next(0, eventSets.Count)]; //Globals.random.Next(0, eventSets.Count)
        foreach (AssetReference assetRef in eventUnits)
        {
            EventUnit unit = assetRef.InstantiateAsync(room.transform).WaitForCompletion().GetComponent<EventUnit>();
            unit.Setup(room, assetRef);
            unit.transform.position = room.GetFloor().GetCellCenterWorld(unit.GenSpawnCoord(room, playerDir));
            unit.UpdateCellCoord();
            room.SetInMap(unit.CellCoord, unit.gameObject);
        }
    }
    public static List<Vector3Int> SpawnCoordRemoveCenter(RoomScript room)
    {
        BoundsInt roomBounds = room.GetFloor().cellBounds;
        List<Vector3Int> positions = room.map.Keys.ToList();
        roomBounds.size /= 2;
        RemoveCellsInBounds(ref positions, roomBounds);
        return positions;
    }
    public static List<Vector3Int> SpawnCoordRemovePlayerSide(RoomScript room)
    {
        BoundsInt roomBounds = room.GetFloor().cellBounds;
        List<Vector3Int> positions = room.map.Keys.ToList();
        switch (room.playerDirection)
        {
            case 0:
                roomBounds.yMin = roomBounds.yMax - Globals.RoundToNearestInt(roomBounds.size.y / 4f);
                break;
            case 1:
                roomBounds.xMin = roomBounds.xMax - Globals.RoundToNearestInt(roomBounds.size.y / 4f);
                break;
            case 2:
                roomBounds.yMax = roomBounds.yMin + Globals.RoundToNearestInt(roomBounds.size.y / 4f);
                break;
            case 3:
                roomBounds.xMax = roomBounds.xMin + Globals.RoundToNearestInt(roomBounds.size.y / 4f);
                break;
        }
        RemoveCellsInBounds(ref positions, roomBounds);
        return positions;
    }
    public static void RemoveCellsInBounds(ref List<Vector3Int> coords, BoundsInt bounds)
    {
        BoundsInt.PositionEnumerator boundsEnum = bounds.allPositionsWithin;
        while (boundsEnum.MoveNext())
        {
            coords.Remove(boundsEnum.Current);
        }
    }
    public static Vector3Int GenSpawnCoord(EnemyScript enemy, List<Vector3Int> positions)
    {
        while (positions.Count > 0)
        {
            Vector3Int cell = positions[Globals.random.Next(0, positions.Count)];
            if (enemy.CheckCellToSpawn(cell))
            {
                return cell;
            }
            positions.Remove(cell);
        }
        Debug.Log("Could not find a spawn coord");
        return new Vector3Int(-1, -1, -1);
    }
}