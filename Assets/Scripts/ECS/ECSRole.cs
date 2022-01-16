using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using System.Collections.Generic;

[RequireComponent(typeof(ConvertToEntity))]
[RequiresEntityConversion]
public class ECSRole : MonoBehaviour, IConvertGameObjectToEntity
{
    public RoleEntitiesOwner roleEntitiesOwner;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        roleEntitiesOwner.Convert(entity, dstManager, conversionSystem);
    }
}
