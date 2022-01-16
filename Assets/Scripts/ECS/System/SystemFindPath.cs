using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
[UpdateAfter(typeof(SystemSetPathParam))]
public class SystemFindPath : ComponentSystem
{   
    NativeArray<int2> neighbourOffsetArray;
    List<IDisposable> disposables;
    protected override void OnCreate()
    {
        disposables = new List<IDisposable>();
        neighbourOffsetArray = new NativeArray<int2>(4, Allocator.Persistent);
        neighbourOffsetArray[0] = new int2(-1, 0);
        neighbourOffsetArray[1] = new int2(1, 0);
        neighbourOffsetArray[2] = new int2(0, 1);
        neighbourOffsetArray[3] = new int2(0, -1);
    }

    protected override void OnDestroy()
    {
        neighbourOffsetArray.Dispose();
    }

    protected override void OnUpdate() {
        if(Main.instance.map == null) return;

        int gridWidth = Main.instance.map.size;
        int gridHeight = Main.instance.map.size;
        int2 gridSize = new int2(gridWidth, gridHeight);

        List<AstarJob> astarJobList = new List<AstarJob>();
        NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
        NativeArray<PathNode> pathNodeArray = Tools.GetPathNodeArray();

        Entities.ForEach((Entity entity, ref CompFindPathParam pathfindingParams) => {

            NativeArray<PathNode> tmpPathNodeArray = new NativeArray<PathNode>(pathNodeArray, Allocator.TempJob);
            NativeList<int> openList = new NativeList<int>(Allocator.TempJob);
            disposables.Add(openList);
            NativeHashMap<int, bool> closedHasMap = new NativeHashMap<int, bool>(gridSize.x * gridSize.y, Allocator.TempJob);
            disposables.Add(closedHasMap);
            AstarJob astarJob = new AstarJob {
                gridSize = gridSize,
                pathNodeArray = tmpPathNodeArray,
                openList = openList,
                closedHasMap = closedHasMap,
                neighbourOffsetArray = neighbourOffsetArray,
                startPosition = pathfindingParams.startPos,
                endPosition = pathfindingParams.endPos,
                entity = entity,
            };
            astarJobList.Add(astarJob);
            jobHandleList.Add(astarJob.Schedule());
            PostUpdateCommands.RemoveComponent<CompFindPathParam>(entity);
        });

        JobHandle.CompleteAll(jobHandleList);

        foreach (var item in disposables)
        {
            item.Dispose();
        }
        disposables.Clear();

        foreach (AstarJob findPathJob in astarJobList) {
            new SetBufferPathJob {
                entity = findPathJob.entity,
                gridSize = findPathJob.gridSize,
                pathNodeArray = findPathJob.pathNodeArray,
                pathfindingParamsComponentDataFromEntity = GetComponentDataFromEntity<CompFindPathParam>(),
                pathFollowComponentDataFromEntity = GetComponentDataFromEntity<CompMoveData>(),
                pathPositionBufferFromEntity = GetBufferFromEntity<CompPathListData>(),
            }.Run();
        }
    }
}