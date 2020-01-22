﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

public class SphereCollisionSystem : JobComponentSystem
{
    private DestructionBufferSystem ecbs;

    protected override void OnCreate()
    {
        ecbs = World.GetOrCreateSystem<DestructionBufferSystem>();
        RequireSingletonForUpdate<GameData>();
        RequireSingletonForUpdate<SpikeTag>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var gameData = GetSingleton<GameData>();
        if (!gameData.IsGameActive) return inputDeps;

        var spikeEntity = GetSingletonEntity<SpikeTag>();

        var jobHandle = new SphereCollisionSystemJob()
        {
            ecb = ecbs.CreateCommandBuffer().ToConcurrent(),
            MaterialIdData = GetComponentDataFromEntity<MaterialId>(true),
            TranslationData = GetComponentDataFromEntity<Translation>(true),
            SpikeEntity = spikeEntity
        }.Schedule(this, inputDeps);

        ecbs.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }

    [BurstCompile]
    [RequireComponentTag(typeof(SphereTag))]
    [ExcludeComponent(typeof(DestroyedTag))]
    private struct SphereCollisionSystemJob : IJobForEachWithEntity<Translation, MaterialId>
    {
        public EntityCommandBuffer.Concurrent ecb;
        [ReadOnly] public ComponentDataFromEntity<MaterialId> MaterialIdData;
        [ReadOnly] public ComponentDataFromEntity<Translation> TranslationData;
        [ReadOnly] public Entity SpikeEntity;

        public void Execute(
            Entity entity,
            int index,
            [ReadOnly] ref Translation translation,
            [ReadOnly] ref MaterialId materialId)
        {
            if (!MaterialIdData.Exists(SpikeEntity)) return;
            if (!TranslationData.Exists(SpikeEntity)) return;

            MaterialId spikeMaterial = MaterialIdData[SpikeEntity];
            Translation spikeTranslation = TranslationData[SpikeEntity];

            if (translation.Value.y <= spikeTranslation.Value.y + 0.5f)
            {
                if (materialId.currentMaterialId == spikeMaterial.currentMaterialId)
                {
                    ecb.AddComponent<DestroyedTag>(index, entity);
                }
                else
                {
                    ecb.AddComponent<DestroyedTag>(index, SpikeEntity);
                }
            }
        }
    }
}