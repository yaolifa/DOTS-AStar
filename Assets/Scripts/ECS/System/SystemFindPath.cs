using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using System.Collections.Generic;
using System;

[UpdateAfter(typeof(SystemSetPathParam))]
public class SystemFindPath : SystemBase
{   
    NativeArray<int2> direction;
    private EndSimulationEntityCommandBufferSystem _ecbSystem;
    List<IDisposable> disposables;
    List<JobAstar> jobs;
    protected override void OnCreate()
    {
        jobs = new List<JobAstar>();
        disposables = new List<IDisposable>();
        direction = new NativeArray<int2>(4, Allocator.Persistent);
        direction[0] = new int2(-1, 0);
        direction[1] = new int2(1, 0);
        direction[2] = new int2(0, 1);
        direction[3] = new int2(0, -1);
        _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        direction.Dispose();
    }

    protected override void OnUpdate() {
        if(Main.instance.map == null) return;

        int gridWidth = Main.instance.map.size;
        int gridHeight = Main.instance.map.size;
        int2 gridSize = new int2(gridWidth, gridHeight);

        NativeArray<PathNode> pathNodeArray = Tools.GetPathNodeArray();
        NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(Allocator.TempJob);
        EntityCommandBuffer ecb = _ecbSystem.CreateCommandBuffer();
        Entities
        .WithoutBurst()
        .ForEach((Entity entity, ref CompFindPathParam pathfindingParams) => {
            NativeArray<PathNode> tmpPathNodeArray = new NativeArray<PathNode>(pathNodeArray, Allocator.TempJob);
            NativeList<int> openList = new NativeList<int>(Allocator.TempJob);
            disposables.Add(openList);
            NativeHashMap<int, bool> closedHasMap = new NativeHashMap<int, bool>(gridSize.x * gridSize.y, Allocator.TempJob);
            disposables.Add(closedHasMap);
            JobAstar jobAstar = new JobAstar {
                gridSize = gridSize,
                pathNodeArray = tmpPathNodeArray,
                openList = openList,
                closedHasMap = closedHasMap,
                direction = direction,
                startPosition = pathfindingParams.startPos,
                endPosition = pathfindingParams.endPos,
                entity = entity,
            };
            jobs.Add(jobAstar);
            jobHandles.Add(jobAstar.Schedule());
            ecb.RemoveComponent<CompFindPathParam>(entity);
        }).Run();

        JobHandle.CompleteAll(jobHandles);
        foreach(var v in disposables){
            v.Dispose();
        }
        disposables.Clear();
        jobHandles.Dispose();

        foreach (JobAstar jobAstar in jobs) {
            new JobSetBufferPath {
                entity = jobAstar.entity,
                gridSize = jobAstar.gridSize,
                pathNodeArray = jobAstar.pathNodeArray,
                pathfindingParamsComponentDataFromEntity = GetComponentDataFromEntity<CompFindPathParam>(),
                pathFollowComponentDataFromEntity = GetComponentDataFromEntity<CompMoveData>(),
                pathPositionBufferFromEntity = GetBufferFromEntity<CompPathListData>(),
            }.Run();
        }

        jobs.Clear();
    }
}