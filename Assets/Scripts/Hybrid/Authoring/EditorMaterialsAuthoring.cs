﻿using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class EditorMaterialsAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public Material[] Materials;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddSharedComponentData(entity, new MaterialReferences()
        {
            Materials = Materials
        });
    }
}