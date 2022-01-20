using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct JobSetBufferPath : IJob {
    
    public int2 gridSize;

    [DeallocateOnJobCompletion]
    public NativeArray<PathNode> pathNodeArray;
    public Entity entity;
    public ComponentDataFromEntity<CompFindPathParam> pathfindingParamsComponentDataFromEntity;
    public ComponentDataFromEntity<CompMoveData> pathFollowComponentDataFromEntity;
    public BufferFromEntity<CompPathListData> pathPositionBufferFromEntity;

    public void Execute() {
        DynamicBuffer<CompPathListData> pathPositionBuffer = pathPositionBufferFromEntity[entity];
        pathPositionBuffer.Clear();

        CompFindPathParam pathfindingParams = pathfindingParamsComponentDataFromEntity[entity];
        int endNodeIndex = Tools.CalculateIndex(pathfindingParams.endPos.x, pathfindingParams.endPos.y, gridSize.x);
        PathNode endNode = pathNodeArray[endNodeIndex];

        Tools.CalculatePath(pathNodeArray, endNode, pathPositionBuffer);
        
        CompMoveData moveData = pathFollowComponentDataFromEntity[entity];
        moveData.index = 1;
        moveData.pathLength = pathPositionBuffer.Length;
        pathFollowComponentDataFromEntity[entity] = moveData;
    }
}
