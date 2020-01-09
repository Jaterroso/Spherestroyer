﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

[AlwaysSynchronizeSystem]
public class ParticlesSpawnSystem : JobComponentSystem
{
    private EntityQuery entityQuery;

    protected override void OnCreate()
    {
        entityQuery = GetEntityQuery(
            ComponentType.ReadOnly(typeof(DestroyedIcosphereTag)),
            ComponentType.ReadOnly(typeof(Translation)),
            ComponentType.ReadOnly(typeof(MaterialId)));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        var translations = entityQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        var materialIds = entityQuery.ToComponentDataArray<MaterialId>(Allocator.TempJob);

        Entities.ForEach((in ParticlesSpawner particlesSpawner) =>
        {
            for (int i = 0; i < translations.Length; i++)
            {
                for (int u = 0; u < particlesSpawner.ParticlesToSpawn; u++)
                {
                    Entity particleEntity = ecb.Instantiate(particlesSpawner.Prefab);
                    ecb.SetComponent(particleEntity, translations[i]);
                    ecb.SetComponent(particleEntity, materialIds[i]);
                    ecb.AddComponent<UpdateMaterialTag>(particleEntity);
                }
            }
        }).Run();
        ecb.Playback(EntityManager);

        materialIds.Dispose();
        translations.Dispose();

        ecb.Dispose();

        return default;
    }
}