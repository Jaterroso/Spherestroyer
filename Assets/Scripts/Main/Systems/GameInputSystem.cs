﻿using Unity.Entities;
using Unity.Jobs;
using Unity.Tiny.Audio;

[AlwaysUpdateSystem]
[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public class GameInputSystem : JobComponentSystem
{
    private InputWrapperSystem inputSystem;

    private EntityQuery inputEntityQuery;

    private BeginSimulationEntityCommandBufferSystem beginInitECBS;
    private EndSimulationEntityCommandBufferSystem endInitECBS;

    protected override void OnCreate()
    {
        inputSystem = World.GetOrCreateSystem<InputWrapperSystem>();

        inputEntityQuery = GetEntityQuery(typeof(InputTag));

        beginInitECBS = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        endInitECBS = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        RequireSingletonForUpdate<SoundManager>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (!inputSystem.IsTouchOrButtonDown()) return default;

        EntityCommandBuffer beginBuffer = beginInitECBS.CreateCommandBuffer();
        EntityCommandBuffer endBuffer = endInitECBS.CreateCommandBuffer();
        var soundManager = GetSingleton<SoundManager>();

        endBuffer.AddComponent<AudioSourceStart>(soundManager.InputAS);

        beginBuffer.AddComponent(inputEntityQuery, typeof(OnInputTag));
        endBuffer.RemoveComponent(inputEntityQuery, typeof(OnInputTag));

        return default;
    }
}