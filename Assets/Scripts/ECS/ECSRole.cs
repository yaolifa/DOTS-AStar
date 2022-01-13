using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using System.Collections.Generic;

[RequireComponent(typeof(ConvertToEntity))]
[RequiresEntityConversion]
public class ECSRole : MonoBehaviour, IConvertGameObjectToEntity
{
    private Entity _entity;
    private EntityManager _dstManager;
    private int _x;
    private int _y;
    private int _index;
    private List<int[]> _path;
    public Entity entity{
        get{
            return _entity;
        }
    }
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        this._entity = entity;
        this._dstManager = dstManager;

        _dstManager.AddComponentData(_entity, new CompMoveData{
            index = _index,
            pos = new int2(_x, _y),
            speed = 10,
        });

        DynamicBuffer<CompPathListData> dynamicBuffer = _dstManager.AddBuffer<CompPathListData>(_entity);
        if(_path != null){
            foreach(int[] array in _path){
                dynamicBuffer.Add(new CompPathListData{pos = new int2(array[0], array[1])});
            }
        }
    }

    public void Init(int x, int y, int index = 1, List<int[]> path = null){
        _x = x;
        _y = y;
        _index = index;
        _path = path;
    }
}
