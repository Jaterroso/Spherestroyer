﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

public class MoveSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
        RequireSingletonForUpdate<GameState>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var gameDataEntity = GetSingletonEntity<GameState>();
        var gameData = EntityManager.GetComponentData<GameState>(gameDataEntity);

        if (!gameData.IsGameActive) return inputDeps;

        var jobHandle = new MoveSystemJob()
        {
            deltaTime = Time.DeltaTime
        }.Schedule(this, inputDeps);
        return jobHandle;
    }

    [BurstCompile]
    private struct MoveSystemJob : IJobForEach<Translation, Move>
    {
        public float deltaTime;

        public void Execute(ref Translation translation, [ReadOnly] ref Move move)
        {
            translation.Value.x += move.speedX * deltaTime;
            translation.Value.y += move.speedY * deltaTime;
            translation.Value.z += move.speedZ * deltaTime;
        }
    }
}