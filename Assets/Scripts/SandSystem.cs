using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;

public struct Sand : IComponentData {
    public float SpawnTime;
    public float TargetHeight;
    public float StartingHeight;
}

public class SandSystem : JobComponentSystem { 
    [BurstCompile]
    struct SandProcess : IJobProcessComponentData<Sand, Position> {
        public float Time;
        public float Gravity;
        public void Execute([ReadOnly] ref Sand sand, ref Position position) {
            float dt = Time - sand.SpawnTime;
            position.Value.y = math.select(
                math.max(sand.TargetHeight, sand.StartingHeight + dt * dt * Gravity),
                1000, 
                dt < 0); // Hide if sand hasn't spawned yet (y = 1000)
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        var job = new SandProcess() {
            Time = Time.time, Gravity = Physics.gravity.y
        };
        return job.Schedule(this, 128, inputDeps);
    }
}
