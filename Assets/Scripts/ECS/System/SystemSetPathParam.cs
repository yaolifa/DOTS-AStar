using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[UpdateAfter(typeof(SystemMovePath))]
public class SystemSetPathParam : SystemBase {
    Random _random;
    private EndSimulationEntityCommandBufferSystem _ecbSystem;
    protected override void OnCreate()
    {
        _random = new Random(56);
        _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        int mapWidth = Main.instance.map.size;
        int mapHeight = Main.instance.map.size;
        int count = mapWidth * mapHeight;
        EntityManager entityManager = World.EntityManager;
        float3 originPosition = float3.zero;
        Entity map = GetSingletonEntity<CompMapFlag>();
        DynamicBuffer<CompMap> compMap = EntityManager.GetBuffer<CompMap>(map);
        EntityCommandBuffer ecb = _ecbSystem.CreateCommandBuffer();
        Entities
        .WithoutBurst()
        .WithNone<CompFindPathParam>()
        .ForEach((Entity entity, int entityInQueryIndex, in CompMoveData compMoveData) => { 
            if (compMoveData.index >= compMoveData.pathLength) {

                int endIndex = 0;
                while(compMap[endIndex].data == 1){
                    endIndex = _random.NextInt(1, count);
                }
                int endX, endY;
                Tools.GetXYByIndex(endIndex, mapWidth, out endX, out endY);
                ecb.AddComponent<CompFindPathParam>(entity, new CompFindPathParam { 
                    startPos = compMoveData.pos, endPos = new int2(endX, endY) 
                });
            }
        }).Run();
    }
}