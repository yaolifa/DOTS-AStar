using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

static class Tools
{
    public static NativeArray<PathNode> pathNodes;
    public static int CalculateIndex(int x, int y, int width)
    {
        return x + y * width;
    }

    public static void GetXYByIndex(int index, int width, out int x, out int y)
    {
        y = index / width;
        x = index % width;
    }

    public static void CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode, DynamicBuffer<CompPathListData> pathPositionBuffer)
    {
        if (endNode.parentIndex == -1)
        {
            // UnityEngine.Debug.LogError("找不到路径！");
        }
        else
        {
            NativeList<int2> list = new NativeList<int2>(Allocator.Temp);
            list.Add(new int2(endNode.x, endNode.y));

            PathNode currentNode = endNode;
            while (currentNode.parentIndex != -1)
            {
                PathNode cameFromNode = pathNodeArray[currentNode.parentIndex];
                list.Add(new int2(cameFromNode.x, cameFromNode.y));
                currentNode = cameFromNode;
            }

            for (int i = list.Length - 1; i >= 0; i--)
            {
                pathPositionBuffer.Add(new CompPathListData { pos = new int2(list[i].x, list[i].y) });
            }
            list.Dispose();
        }
    }

    public static int CalculateDistanceCost(int2 current, int2 end){
        int disX = current.x - end.x;
        int disY = current.y - end.y;
        return (Math.Abs(disX) + Math.Abs(disY)) * 10;
    }

    public static int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray) {
        PathNode lowestCostPathNode = pathNodeArray[openList[0]];
        for (int i = 1; i < openList.Length; i++) {
            PathNode testPathNode = pathNodeArray[openList[i]];
            if (testPathNode.f < lowestCostPathNode.f) {
                lowestCostPathNode = testPathNode;
            }
        }
        return lowestCostPathNode.index;
    }
    
    public static NativeArray<PathNode> GetPathNodeArray() {
        if(pathNodes.Length == 0){
            Map map = Main.instance.map;
            pathNodes = new NativeArray<PathNode>(map.size * map.size, Allocator.Persistent);

            for (int x = 0; x < map.size; x++) {
                for (int y = 0; y < map.size; y++) {
                    PathNode pathNode = new PathNode();
                    pathNode.x = x;
                    pathNode.y = y;
                    pathNode.index = CalculateIndex(x, y, map.size);
                    pathNode.canMove = map.mapData[x, y] == 0;
                    pathNode.parentIndex = -1;
                    pathNodes[pathNode.index] = pathNode;
                }
            }
        }

        return pathNodes;
    }
}
