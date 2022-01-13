using Unity.Entities;
using Unity.Mathematics;
public struct CompMoveData:IComponentData
{
    public int index;
    public int2 pos;
    public float speed;
}