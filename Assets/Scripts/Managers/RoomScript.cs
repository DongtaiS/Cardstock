using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum RoomTypes { Combat, Shop, Event, Rest, Boss }
public class RoomScript : MonoBehaviour
{
    public Pathfinding pathfinding;
    public Dictionary<Vector3Int, GameObject> map = new Dictionary<Vector3Int, GameObject>();
    public List<DoorScript> doors = new List<DoorScript>();
    [System.NonSerialized] public RoomTypes RoomType;
    [System.NonSerialized] public int Depth;
    [System.NonSerialized] public Vector2Int mapCoord;
    public int playerDirection { get; private set; }
    
    [SerializeField] private Tilemap floor;
    [SerializeField] private List<Tilemap> Walls;
    [SerializeField] private List<Tilemap> WallShadows;
    [SerializeField] private Transform[] transformConnections = new Transform[4];

    private RoomManagerScript roomManager;

    public RoomScript[] roomConnections = new RoomScript[4];
    private List<Tilemap> tilemaps = new List<Tilemap>();
    private List<CombatScript> combatables = new List<CombatScript>();
    private List<Light> lights = new List<Light>();
    private bool isHidden;

    public void Setup(RoomManagerScript inRoomManager, Vector2Int coord, int depth, RoomTypes type)
    {
        GetComponentsInChildren(tilemaps);
        GetComponentsInChildren(lights);
        roomManager = inRoomManager;
        mapCoord = coord;
        pathfinding = new Pathfinding(floor);
        doors.AddRange(Enumerable.Repeat<DoorScript>(null, 4).ToList());
        RoomType = type;
        Depth = depth;
        BoundsInt.PositionEnumerator posEnum = floor.cellBounds.allPositionsWithin;
        while (posEnum.MoveNext())
        {
            if (HasEmptyTile(posEnum.Current))
            {
                map.Add(posEnum.Current, null);
            }
        }
    }
    public void ActivateRoom(RoomTypes roomType)
    {
        if (roomType == RoomTypes.Combat)
        {
            roomManager.SpawnEnemySet(this, playerDirection, EnemySetEnum.Combat1);
            Globals.GameState.StartCoroutine(Globals.GameState.ChangeState(GameStates.COMBAT));
        }
        else if (roomType == RoomTypes.Boss)
        {
            roomManager.SpawnBoss(this, playerDirection);
            Globals.GameState.StartCoroutine(Globals.GameState.ChangeState(GameStates.COMBAT));
        }
        else if (roomType == RoomTypes.Event)
        {
            Globals.PlayerCombat.playerMove.enabled = true;
            roomManager.SpawnEvent(this, playerDirection);
        }
    }
    public void HideRoom()
    {
        isHidden = true;
        foreach (CombatScript combatable in combatables)
        {
            combatable.gameObject.SetActive(false);
        }
        foreach (DoorScript door in doors)
        {
            if (door != null)
            {
                door.SetColor(Globals.ChangeColorAlpha(door.sprite.color, 0));
            }
        }
        foreach (Tilemap tmap in tilemaps)
        {
            tmap.gameObject.SetActive(false);
        }
        foreach (Light light in lights)
        {
            light.enabled = false;
        }
    }
    public void ShowRoom()
    {
        isHidden = false;
        foreach (CombatScript combatable in combatables)
        {
            combatable.gameObject.SetActive(true);
        }
        foreach (DoorScript door in doors)
        {
            if (door != null)
            {
                door.SetColor(Globals.ChangeColorAlpha(door.sprite.color, 1));
            }
        }
        foreach (Tilemap tmap in tilemaps)
        {
            tmap.gameObject.SetActive(true);
        }
        foreach (Light light in lights)
        {
            light.enabled = true;
        }
    }
    public Vector2Int GetMapCoord()
    {
        return mapCoord;
    }
    public void SetPlayerDirection(int dir)
    {
        playerDirection = dir;
    }
    public void SetInMap(Vector3Int cellCoord, GameObject inGO)
    {
        SetAtCell(cellCoord, inGO);
        if (inGO.TryGetComponent(out CombatScript combatScript))
        {
            combatables.Add(combatScript);
        }
    }
    public bool TryGetEnemyAtCell(Vector3Int coord, out EnemyScript e)
    {
        if (map.TryGetValue(coord, out GameObject GO))
        {
            if (GO != null && GO.TryGetComponent(out EnemyScript enemy))
            {
                e = enemy;
                return true;
            }
        }
        e = null;
        return false;
    }
    public EnemyScript FindClosestEnemy(Vector3Int cell)
    {
        List<CombatScript> enemies = GetAllEnemies();
        int dist = int.MaxValue;
        EnemyScript e = null;
        foreach (EnemyScript enemy in enemies)
        {
            if (Globals.PerpDist(cell, enemy.CellCoord) < dist)
            {
                dist = Globals.PerpDist(cell, enemy.CellCoord);
                e = enemy;
            }
        }
        return e;
    }
    public List<EnemyScript> GetEnemiesInRadius(Vector3Int centerCell, float radius)
    {
        List<EnemyScript> enemies = new List<EnemyScript>();
        List<Vector3Int> cells = Globals.GetCellsInRadius(centerCell, radius);
        foreach (Vector3Int cell in cells)
        {
            if (TryGetEnemyAtCell(cell, out EnemyScript e))
            {
                enemies.Add(e);
            }
        }
        return enemies;
    }
    public void RemoveAtCell(Vector3 coord)
    {
        RemoveAtCell(floor.WorldToCell(coord));
    }
    public void RemoveAtCell(Vector3Int coord)
    {
        map.Remove(coord);
        pathfinding.nodeMap.TryGetValue(coord, out PathNode node);
        if (node != null)
        {
            node.IsWalkable = true;
        }
    }
    public void SetAtCell(Vector3 coord, GameObject obj)
    {
        SetAtCell(floor.WorldToCell(coord), obj);
    }
    public void SetAtCell(Vector3Int coord, GameObject obj)
    {
        map[coord] = obj;
        pathfinding.nodeMap[coord].IsWalkable = false;
    }
    public List<CombatScript> GetAllCombatables()
    {
        return combatables;
    }
    public List<CombatScript> GetAllEnemies()
    {
        List<CombatScript> result = new List<CombatScript>(combatables);
        result.Remove(Globals.PlayerCombat);
        return result;
    }
    public Tilemap GetFloor()
    {
        return floor;
    }
    public bool IsWalkable(Vector3Int coord)
    {
        if (floor.HasTile(coord))
        {
            return HasEmptyTile(coord);
        }
        return false;
    }
    public bool HasEmptyTile(Vector3Int coord)
    {
        return floor.HasTile(coord) && (!map.ContainsKey(coord) || map[coord] == null);
    }
    public void ChangeRoom(RoomScript newRoom, Vector3Int newCell)
    {
        combatables.Remove(Globals.PlayerCombat);
        roomManager.SetCurrentMapCoord(newRoom.GetMapCoord());
        Globals.PlayerCombat.playerMove.enabled = false;
        StartCoroutine(ChangeRoomAnim(newRoom, newCell));
    }
    private IEnumerator ChangeRoomAnim(RoomScript newRoom, Vector3Int newCell)
    {
        PlayerCombatScript pCombat = Globals.PlayerCombat;
        if (newRoom.RoomType == RoomTypes.Combat)
        {
            StartCoroutine(Globals.AudioManager.FadeMusic(AudioManager.MusicEnum.Combat_ThroughTheAlps, 2f));
        }
        StartCoroutine(pCombat.Walk(pCombat.CellCoord + (Vector3Int)Globals.IntDirectionToVector2(pCombat.FacingDirection)));
        yield return Globals.WaitForSeconds(0.5f);
        yield return Globals.InterpFloat(0, 1, 0.5f, Globals.AnimationCurves.IncEaseInOut, a => UIManagerScript.GetDarkBG().color = Globals.ChangeColorAlpha(UIManagerScript.GetDarkBG().color, a));
        yield return Globals.WaitForSeconds(0.5f);
        HideRoom();
        pCombat.CurrentRoom = newRoom;
        pCombat.transform.position = newRoom.GetFloor().GetCellCenterWorld(newCell);
        newRoom.SetInMap(newRoom.GetFloor().WorldToCell(Globals.PlayerCombat.transform.position), Globals.PlayerCombat.gameObject);
        pCombat.UpdateCellCoord();
        roomManager.GenerateRooms();
        newRoom.ShowRoom();
        newRoom.ActivateRoom(newRoom.RoomType);
        Globals.CameraManager.ActivateUnitCam(newRoom.transform);
        Coroutine fade = StartCoroutine(Globals.InterpFloat(1, 0, 2f, Globals.AnimationCurves.IncEaseInOut, a => UIManagerScript.GetDarkBG().color = Globals.ChangeColorAlpha(UIManagerScript.GetDarkBG().color, a)));
        yield return Globals.WaitForSeconds(0.25f);
        pCombat.transform.SetParent(newRoom.transform);
        newRoom.StartCoroutine(newRoom.FlipWalls());
        StartCoroutine(pCombat.Walk(pCombat.CellCoord + (Vector3Int)Globals.IntDirectionToVector2(pCombat.FacingDirection)));
        yield return fade;
        Globals.CameraManager.ActivatePlayerCam();
    }
    public IEnumerator FlipWalls()
    {
        Coroutine anim = null;
        foreach (Tilemap tilemap in Walls)
        {
            tilemap.orientationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, tilemap.orientationMatrix.rotation.eulerAngles.y, 0), Vector3.one);
        }
        foreach (Tilemap tilemap in WallShadows)
        {
            tilemap.orientationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(270, tilemap.orientationMatrix.rotation.eulerAngles.y, 0), Vector3.one);
        }
        foreach (Tilemap tilemap in Walls)
        {
            StartCoroutine(Globals.InterpAngle(90, 0, 2f, false, ang => tilemap.orientationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(ang, tilemap.orientationMatrix.rotation.eulerAngles.y, 0), Vector3.one)));
            yield return Globals.WaitForSeconds(0.5f);
        }
        foreach (Tilemap tilemap in WallShadows)
        {
            anim = StartCoroutine(Globals.InterpAngle(270, 0, 2f, false, ang => tilemap.orientationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(ang, tilemap.orientationMatrix.rotation.eulerAngles.y, 0), Vector3.one)));
        }
        yield return anim;
    }
    public bool TryMoveTo(Vector3Int coord)
    {
        if (map.TryGetValue(coord, out GameObject GO))
        {
            if (GO != null && GO.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact(this);
                return false;
            }
        }
        if (!IsWalkable(coord))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public Transform GetTransformConnection(int direction)
    {
        return transformConnections[direction];
    }
    public void AddRoomConnection(RoomScript room, int direction)
    {
        roomConnections[direction] = room;
        roomManager.CreateDoor(this, direction);
    }
    public void AddDoor(DoorScript inDoor, int index)
    {
        doors[index] = inDoor;
        BoundsInt.PositionEnumerator enumerator = GetDoorBoundsInt(inDoor, floor).allPositionsWithin;
        while (enumerator.MoveNext())
        {
            map[enumerator.Current] = inDoor.gameObject;
        }
        if (isHidden)
        {
            inDoor.SetColor(Globals.ChangeColorAlpha(inDoor.sprite.color, 0));
        }
    }
    public void RemoveDoor(DoorScript inDoor)
    {
/*        while (enumerator.MoveNext())
        {
            map.Remove(enumerator.Current);
            floor.SetTile(enumerator.Current, null);
        }*/
        doors[doors.IndexOf(inDoor)] = null;
    }
    public void OpenDoor(DoorScript doorOpened)
    {
        for (int i = 0; i < doors.Count; i++)
        {
            DoorScript door = doors[i];
            if (door != null)
            {
                if (door == doorOpened)
                {
                    RoomScript newRoom = roomManager.GetMap()[mapCoord + Globals.IntDirectionToVector2(doors.IndexOf(doorOpened))];
                    newRoom.SetPlayerDirection(Globals.OppositeDirection(doors.IndexOf(doorOpened)));
                    BoundsInt.PositionEnumerator enumerator = GetDoorBoundsInt(door, floor).allPositionsWithin;
                    while (enumerator.MoveNext())
                    {
                        map.Remove(enumerator.Current);
                    }
                    StartCoroutine(door.Open());
                    StartCoroutine(newRoom.doors[Globals.OppositeDirection(i)].Open());
                    ChangeRoom(newRoom, GetCellInNewRoom(door, i, newRoom));
                    doors[doors.IndexOf(door)] = null;
                }
                else
                {
                    RemoveDoor(door);
                    Destroy(door.gameObject);
                }
            }
        }
    }
/*    public void ChangeTileToAlt(Vector3Int coord, Tilemap tilemap)
    {
        if (roomManager.GetTileToAlt(tilemap.GetTile(coord)) != null && tilemap.HasTile(coord))
        {
            tilemap.SetTile(coord, roomManager.GetTileToAlt(tilemap.GetTile(coord)));
        }
    }
    public void ChangeAltToTile(Vector3Int coord, Tilemap tilemap)
    {
        if (roomManager.GetAltToTile(tilemap.GetTile(coord)) != null && tilemap.HasTile(coord))
        {
            tilemap.SetTile(coord, roomManager.GetAltToTile(tilemap.GetTile(coord)));
        }
    }*/
    private Vector3Int GetCellInNewRoom(DoorScript door, int dir, RoomScript newRoom)
    {
        BoundsInt doorBounds = GetDoorBoundsInt(door, floor);
        BoundsInt newDoorBounds = newRoom.GetDoorBoundsInt(newRoom.doors[Globals.OppositeDirection(dir)], newRoom.GetFloor());
        Vector3Int tempCoord = new Vector3Int(0, 0, 0);
        if (dir % 2 == 0)
        {
            tempCoord.x = newDoorBounds.x - (doorBounds.x - Globals.PlayerCombat.CellCoord.x);
            tempCoord.y = newDoorBounds.y;
        }
        else
        {
            tempCoord.x = newDoorBounds.x;
            tempCoord.y = newDoorBounds.y - (doorBounds.y - Globals.PlayerCombat.CellCoord.y);
        }
        return tempCoord;
    }
    private BoundsInt GetDoorBoundsInt(DoorScript inDoor, Tilemap tilemap)
    {
        int dir = doors.IndexOf(inDoor);
        Vector3Int size = new Vector3Int(1, 1, 1);
        if (dir % 2 == 0)
        {
            size.x = 2;
        }
        else
        {
            size.y = -2;
        }
        BoundsInt bounds = new BoundsInt();
        int add = 1;
        if (dir == 1 || dir == 0)
        {
            add = -1;
        }
        bounds.position = tilemap.WorldToCell(inDoor.transform.TransformPoint(inDoor.LeftCorner.localPosition + new Vector3(0, 0, add)));
        bounds.size = size;
        return bounds;
    }
}