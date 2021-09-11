using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class Pathfinding
{
    public Dictionary<Vector3Int, PathNode> nodeMap = new Dictionary<Vector3Int, PathNode>();
    private HashSet<PathNode> nodesToSearch = new HashSet<PathNode>();
    private HashSet<PathNode> searchedNodes = new HashSet<PathNode>();
    private Tilemap tilemap;
    private bool showDebugLines = true;
    public Pathfinding(Tilemap floor)
    {
        tilemap = floor;
        BoundsInt.PositionEnumerator enumerator = floor.cellBounds.allPositionsWithin;
        while (enumerator.MoveNext())
        {
            if (floor.HasTile(enumerator.Current))
            {
                nodeMap.Add(enumerator.Current, new PathNode(enumerator.Current));
            }
        }
    }
    public List<Vector3Int> GeneratePath(Vector3Int start, Vector3Int end, int movesPerTurn, bool inclusiveEnd)
    {
        return GeneratePath(start, end, new RectInt(0,0,1,1), movesPerTurn, inclusiveEnd);
    }
    public List<Vector3Int> GeneratePath(Vector3Int start, Vector3Int end, RectInt size, int movesPerTurn, bool inclusiveEnd)
    {
        nodesToSearch.Clear();
        searchedNodes.Clear();
        foreach (PathNode node in nodeMap.Values)
        {
            node.Reset();
        }
        if (nodeMap.TryGetValue(start, out PathNode startNode))
        {
            startNode.direction = -1;
            nodesToSearch.Add(startNode);
            while (nodesToSearch.Count > 0)
            {
                PathNode currentNode = GetLowestFCostNode();
                if (currentNode.coordinate == end)
                {
                    return CalculatePath(currentNode, inclusiveEnd);
                }
                nodesToSearch.Remove(currentNode);
                searchedNodes.Add(currentNode);

                bool inBounds = true;
                RectInt.PositionEnumerator enumerator = size.allPositionsWithin;
                while (enumerator.MoveNext())
                {
                    Vector3Int tempCoord = currentNode.coordinate + (Vector3Int)enumerator.Current;
                    if (tempCoord != start)
                    {
                        if (nodeMap.TryGetValue(tempCoord, out PathNode node))
                        {
                            if (!node.IsWalkable)
                            {
                                Debug.Log("not walkable");
                                inBounds = false;
                                break;
                            }
                        }
                        else
                        {
                            inBounds = false;
                            break;
                        }
                    }
                }

                if (inBounds)
                {
                    foreach (PathNode node in GetNeighbors(currentNode))
                    {
                        if (!searchedNodes.Contains(node))
                        {
                            int tempGCost = CalculateGCost(currentNode, node, CalculateTurnCost(currentNode, movesPerTurn));
                            if (tempGCost < node.gCost)
                            {
                                if (currentNode.direction == node.direction || currentNode.direction == -1)
                                {
                                    node.countInDir = currentNode.countInDir + 1;
                                }
                                else
                                {
                                    node.countInDir = 1;
                                }
                                node.lastNode = currentNode;
                                node.gCost = tempGCost;
                                node.hCost = CalculateHCost(node, end, movesPerTurn);
                                node.CalculateFCost();
                                if (node.coordinate == end)
                                {
                                    return CalculatePath(node, inclusiveEnd);
                                }
                                if (node.IsWalkable)
                                {
                                    nodesToSearch.Add(node);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("Bounds error");
                }
            }
            Debug.Log("no more nodes ");
        }
        Debug.Log("error in GeneratePath");
        return null;
    }
    private int CalculateTurnCost(PathNode currentNode, int movesPerTurn)
    {
        int mod = currentNode.countInDir % movesPerTurn;
        if (mod == 0)
        {
            return 0;
        }
        return movesPerTurn - mod;
    }
    private int CalculateHCost(PathNode node, Vector3Int end, int movesPerTurn)
    {
        int xMod = 0;
        int yMod = 0;
        if (node.direction % 2 == 0)
        {
            yMod = node.countInDir;
        }
        else
        {
            xMod = node.countInDir;
        }
        int xCost = Mathf.CeilToInt(Mathf.Abs(node.coordinate.x - end.x) + xMod / (float)movesPerTurn);
        int yCost = Mathf.CeilToInt(Mathf.Abs(node.coordinate.y - end.y) + yMod / (float)movesPerTurn);
        return xCost + yCost;
    }
    private int CalculateGCost(PathNode currentNode, PathNode nextNode, int turnCost)
    {
        if (currentNode.direction != -1 && currentNode.direction != nextNode.direction)
        {
            return 1 + currentNode.gCost + turnCost;
        }
        else
        {
            return 1 + currentNode.gCost;
        }
    }
    private List<PathNode> GetNeighbors(PathNode node)
    {
        List<PathNode> neighborNodes = new List<PathNode>();
        for (int i = 0; i < 4; i++)
        {
            if (nodeMap.TryGetValue(node.coordinate + (Vector3Int)Globals.IntDirectionToVector2(i), out PathNode neighbor))
            {
                neighbor.direction = i;
                neighborNodes.Add(neighbor);
            }
        }
        return neighborNodes;
    }
    private PathNode GetLowestFCostNode()
    {
        PathNode lowestFCostNode = null;
        foreach (PathNode node in nodesToSearch)
        {
            if (lowestFCostNode == null || node.fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = node;
            }
        }
        return lowestFCostNode;
    }
    private List<Vector3Int> CalculatePath(PathNode endNode, bool inclusiveEnd)
    {
        List<Vector3Int> result = new List<Vector3Int>();
        PathNode currentNode = endNode;
        if (inclusiveEnd)
        {
            result.Add(currentNode.coordinate);
        }
        while (currentNode.lastNode != null)
        {
            if (showDebugLines)
            {
                Debug.DrawLine(tilemap.GetCellCenterWorld(currentNode.coordinate), tilemap.GetCellCenterWorld(currentNode.lastNode.coordinate), Color.white, 2f);
            }
            result.Add(currentNode.lastNode.coordinate);
            currentNode = currentNode.lastNode;
        }
        result.Reverse();
        return result;
    }
}

public class PathNode
{
    public Vector3Int coordinate;
    public int gCost;
    public int hCost;
    public int fCost;
    public int? direction;
    public int countInDir;
    public PathNode lastNode;
    public bool IsWalkable = true;
    public PathNode(Vector3Int coord)
    {
        coordinate = coord;
    }
    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
    public void Reset()
    {
        gCost = int.MaxValue;
        hCost = 0;
        fCost = gCost + hCost;
        lastNode = null;
        direction = null;
        countInDir = 0;
    }
}
