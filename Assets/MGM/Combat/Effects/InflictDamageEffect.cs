﻿using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using Wayn.Mgm.Event;
using Wayn.Mgm.Event.Registry;

namespace Wayn.Mgm.Combat.Effects
{
    /// <summary>
    /// This effect add the Amount the the Target entity's Health
    /// </summary>
    [Serializable]
    public struct InflictDamageEffect : IEffect
    {
        /// <summary>
        /// The amount of health changed.
        /// </summary>
        public float Amount;
    }


    public class InflictDamageEffectConsumer : EffectConsumerSystem<InflictDamageEffect>
    {
        private EffectDisptacherSystem m_EffectCommandSystem;


        protected override JobHandle ScheduleJob(
            in NativeMultiHashMap<MapKey, EffectCommand>.Enumerator EffectCommandEnumerator,
            in NativeHashMap<int, InflictDamageEffect> RegisteredEffects)
        {
            JobHandle jh = new ConsumerJob()
            {
                EffectCommandEnumerator = EffectCommandEnumerator,
                RegisteredEffects = RegisteredEffects,
                Healths = GetComponentDataFromEntity<Health>(false),
                OnDeathBuffer = GetBufferFromEntity<OnDeath>(true),
                EffectCommandQueue = m_EffectCommandSystem.CreateCommandsQueue()
            }.Schedule(Dependency);

            m_EffectCommandSystem.AddJobHandleFromProducer(jh);
            return jh;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            m_EffectCommandSystem = World.GetOrCreateSystem<EffectDisptacherSystem>();
        }



        [BurstCompile]
        public struct ConsumerJob : IJob
        {
            [ReadOnly]
            public NativeMultiHashMap<MapKey, EffectCommand>.Enumerator EffectCommandEnumerator;
            [ReadOnly]
            public NativeHashMap<int, InflictDamageEffect> RegisteredEffects;

            public ComponentDataFromEntity<Health> Healths;
            public NativeQueue<EffectCommand>.ParallelWriter EffectCommandQueue;
            public BufferFromEntity<OnDeath> OnDeathBuffer;

            public void Execute()
            {
                while (EffectCommandEnumerator.MoveNext())
                {
                    EffectCommand command = EffectCommandEnumerator.Current;
                    Entity target = command.Target;

                    if (!Healths.Exists(target)) continue;

                    Healths[target] = PoolMethods.SubtractValue(Healths[target], RegisteredEffects[command.RegistryReference.VersionId].Amount);

                    if (Healths[target].Value > 0) continue;

                    if (!OnDeathBuffer.Exists(target)) continue;

                    NativeArray<OnDeath>.Enumerator OnDeathEnum = OnDeathBuffer[target].GetEnumerator();

                    while (OnDeathEnum.MoveNext())
                    {
                        EffectCommandQueue.Enqueue(new EffectCommand()
                        {
                            RegistryReference = OnDeathEnum.Current.RegistryEventReference,
                            Emitter = command.Emitter,
                            Target = target
                        });
                    }
                }
            }
        }
    }

}
