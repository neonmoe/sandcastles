using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

public class ECSBootstrap : MonoBehaviour {
    public MeshInstanceRendererComponent SandMesh;

    private EntityArchetype SandArchetype;
    private MeshInstanceRenderer SandRenderer;
    private List<Entity> Entities = new List<Entity>();

    private void Start() {
        EntityManager EntityManager = World.Active.GetOrCreateManager<EntityManager>();
        SandArchetype = EntityManager.CreateArchetype(typeof(Sand), typeof(Position),
            typeof(TransformMatrix), typeof(MeshInstanceRendererComponent));
        SandRenderer = SandMesh.Value;
        Destroy(SandMesh.gameObject);
    }

    private void OnGUI() {
        if (GUILayout.Button("Create")) {
            CreateWorld();
        }
    }

    private void CreateWorld() {
        EntityManager EntityManager = World.Active.GetOrCreateManager<EntityManager>();

        foreach (Entity e in Entities) {
            EntityManager.DestroyEntity(e);
        }
        Entities.Clear();

        float craterStrength = Random.value;
        float craterSize = Random.value;
        float craterNoiseStrength = Random.value;

        int s = 50;
        for (int x = 0; x < s; x++) {
            for (int z = 0; z < s; z++) {
                float Height = Mathf.PerlinNoise(x * 0.1f, z * 0.1f) * 4 + 1;
                float x_ = x - 25;
                float z_ = z - 25;
                float Dist = Mathf.Sqrt(x_ * x_ + z_ * z_);
                float r = craterSize * 20 + 5;
                Height += (r - Mathf.Min(r, Mathf.Abs(Dist - r))) * craterStrength * (1 + Mathf.PerlinNoise(x_ * 0.2f, z_ * 0.2f) * craterNoiseStrength);
                for (int y = 0; y < Height; y++) {
                    CreateBlock(EntityManager, SandArchetype, SandRenderer, s, x, y, z, Dist);
                }
            }
        }
    }

    private void CreateBlock(EntityManager EntityManager, EntityArchetype Archetype, 
            MeshInstanceRenderer Renderer, int size, int x, int y, int z, float d) {
        float Step = 9.0f / size;
        d /= size;
        var Entity = EntityManager.CreateEntity(Archetype);
        float x_ = (x - (size - 1) / 2.0f) * Step;
        float y_ = y * Step;
        float z_ = (z - (size - 1) / 2.0f) * Step;
        EntityManager.SetComponentData(Entity, new Sand() {
            SpawnTime = Time.time + y_ * 0.25f + Random.value * 0.5f - d,
            TargetHeight = y_,
            StartingHeight = 15
        });
        EntityManager.SetComponentData(Entity, new Position(new float3(x_, 15, z_)));
        EntityManager.AddSharedComponentData(Entity, Renderer);
        Entities.Add(Entity);
    }
}
