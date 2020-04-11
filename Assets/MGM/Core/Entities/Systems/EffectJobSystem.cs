﻿using Wayn.Mgm.Events;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateBefore(typeof(EffectConsumerSystemGroup))]
public abstract class EffectJobSystem : JobComponentSystem
{

    protected NativeQueue<EffectCommand>.ParallelWriter m_EffectCommandQueue;
    protected NativeQueue<EffectCommand>.ParallelWriter m_EffectCommandQueue2;
    protected NativeQueue<EffectCommand>.ParallelWriter m_EffectCommandQueue3;
    protected EffectBufferSystem m_EffectBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        if (m_EffectBufferSystem == null)
        {
            m_EffectBufferSystem = World.GetOrCreateSystem<EffectBufferSystem>();
            m_EffectCommandQueue = m_EffectBufferSystem.CreateCommandsQueue();
            m_EffectCommandQueue2 = m_EffectBufferSystem.CreateCommandsQueue();
            m_EffectCommandQueue3 = m_EffectBufferSystem.CreateCommandsQueue();
        }
    }

    protected void AddJobHandleForConsumer(JobHandle job)
    {
        m_EffectBufferSystem.AddJobHandleForConsumer(job);
    }

}