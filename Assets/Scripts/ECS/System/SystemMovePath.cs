using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public class SystemMovePath : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        Entities
        .WithBurst()
        .WithNone<CompFindPathParam>()
        .ForEach((ref DynamicBuffer<CompPathListData> dynamicBuffer , ref CompMoveData moveData, ref Translation translation) =>
        {
            if (moveData.index < moveData.pathLength)
            {
                CompPathListData pathData = dynamicBuffer[moveData.index];
                int dirX = pathData.pos.x - moveData.pos.x;
                int dirY = pathData.pos.y - moveData.pos.y;
                float x = translation.Value.x + deltaTime * dirX * moveData.speed;
                float y = translation.Value.z + deltaTime * dirY * moveData.speed;
                if (
                    (dirX < 0 && x <= pathData.pos.x) ||
                    (dirX > 0 && x >= pathData.pos.x) ||
                    (dirY > 0 && y >= pathData.pos.y) ||
                    (dirY < 0 && y <= pathData.pos.y))
                {
                    moveData.pos = pathData.pos;
                    translation.Value = new float3(pathData.pos.x, 0, pathData.pos.y);
                    moveData.index++;
                }
                else
                {
                    translation.Value = new float3(x, 0, y);
                }
            }
        }).ScheduleParallel();
    }
}