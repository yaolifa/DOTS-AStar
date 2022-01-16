using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct AstarJob : IJob
{
    public int2 gridSize;
    public NativeArray<PathNode> pathNodeArray;
    [ReadOnly]
    public NativeArray<int2> neighbourOffsetArray;
    public int2 startPosition;
    public int2 endPosition;
    // [DeallocateOnJobCompletion]
    public NativeList<int> openList;
    // [DeallocateOnJobCompletion]
    public NativeHashMap<int, bool> closedHasMap;
    public Entity entity;

    public void Execute()
    {
        int endNodeIndex = Tools.CalculateIndex(endPosition.x, endPosition.y, gridSize.x);

        PathNode startNode = pathNodeArray[Tools.CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];

        openList.Add(startNode.index);
        while (openList.Length > 0)
        {
            int currentNodeIndex = Tools.GetLowestCostFNodeIndex(openList, pathNodeArray);
            PathNode currentNode = pathNodeArray[currentNodeIndex];

            if (currentNodeIndex == endNodeIndex)
            {
                // UnityEngine.Debug.LogError("找到");
                break;
            }

            for (int i = 0; i < openList.Length; i++)
            {
                if (openList[i] == currentNodeIndex)
                {
                    openList.RemoveAtSwapBack(i);
                    break;
                }
            }

            closedHasMap.Add(currentNodeIndex, true);

            for (int i = 0; i < neighbourOffsetArray.Length; i++)
            {
                int2 neighbourOffset = neighbourOffsetArray[i];
                int2 neighbourPosition = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);

                if (neighbourPosition.x < 0 || neighbourPosition.x >= gridSize.x || neighbourPosition.y < 0 || neighbourPosition.y >= gridSize.y)
                {
                    continue;
                }

                int neighbourNodeIndex = Tools.CalculateIndex(neighbourPosition.x, neighbourPosition.y, gridSize.x);

                if (closedHasMap.ContainsKey(neighbourNodeIndex))
                {
                    continue;
                }

                PathNode neighbourNode = pathNodeArray[neighbourNodeIndex];
                if (!neighbourNode.canMove)
                {
                    continue;
                }

                if (!openList.Contains(neighbourNode.index))
                {
                    openList.Add(neighbourNode.index);
                    neighbourNode.parentIndex = currentNodeIndex;
                    neighbourNode.g = currentNode.g + 10;
                    neighbourNode.h = Tools.CalculateDistanceCost(neighbourPosition, endPosition);
                    neighbourNode.f = neighbourNode.g + neighbourNode.h;
                    pathNodeArray[neighbourNodeIndex] = neighbourNode;
                }
                else
                {
                    int tentativeGCost = currentNode.g + 10;
                    if (tentativeGCost < neighbourNode.g)
                    {
                        neighbourNode.parentIndex = currentNodeIndex;
                        neighbourNode.g = tentativeGCost;
                        neighbourNode.f = neighbourNode.g + neighbourNode.h;
                        pathNodeArray[neighbourNodeIndex] = neighbourNode;
                    }
                }
            }
        }
    }
}