using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;

public class ECSBootstrap : MonoBehaviour {
    public MeshInstanceRendererComponent SandMesh;

    private EntityArchetype SandArchetype;
    private MeshInstanceRenderer SandRenderer;
    private List<Entity> Entities = new List<Entity>();

    private List<float> Values = new List<float>();

    private List<float> LastValues = new List<float>();

    private void Start() {
        EntityManager EntityManager = World.Active.GetOrCreateManager<EntityManager>();
        SandArchetype = EntityManager.CreateArchetype(typeof(Sand), typeof(Position),
            typeof(TransformMatrix), typeof(MeshInstanceRendererComponent));
        SandRenderer = SandMesh.Value;
        Destroy(SandMesh.gameObject);
    }

    private void Update() {
        if (Input.GetButtonDown("Upvote") && Values.Count > 0) {
            LastValues = new List<float>(Values);
        }
        if (Input.GetButtonDown("Reset Upvote")) {
            LastValues.Clear();
        }
        if (Input.GetButtonDown("Generate World")) {
            CreateWorld();
        }
    }

    private float ApplyGenes(float initialVal, float midPoint) {
        if (LastValues.Count > 0) {
            float GeneValue = LastValues[Values.Count] - midPoint;
            float NewValue = initialVal - midPoint;
            return NewValue + GeneValue + midPoint;
        } else {
            return initialVal;
        }
    }

    private void CreateWorld() {
        EntityManager EntityManager = World.Active.GetOrCreateManager<EntityManager>();

        foreach (Entity e in Entities) {
            EntityManager.DestroyEntity(e);
        }
        Entities.Clear();

        Values.Clear();
        Values.Add(ApplyGenes(Random.value, 0.5f)); // 0: Crater Strength
        Values.Add(ApplyGenes(Random.value, 0.5f)); // 1: Crater Size
        Values.Add(ApplyGenes(Random.value, 0.5f)); // 2: Crater Noisyness
        Values.Add(ApplyGenes(Random.value * 5 + 3, 5.5f)); // 3: Wall Width
        Values.Add(ApplyGenes(Mathf.Max(0, Random.value * 5 - 2), 0.5f)); // 4: Wall Height
        Values.Add(ApplyGenes(Random.value * 7 + 8, 11.5f)); // 5: Wall Radius
        Values.Add(ApplyGenes(Random.value * 4 + 1, 3f)); // 6: Wall Tower Height

        int s = 50;
        for (int x = 0; x < s; x++) {
            for (int z = 0; z < s; z++) {
                float Height = Mathf.PerlinNoise(x * 0.1f, z * 0.1f) * 4 + 1;
                float x_ = x - 25;
                float z_ = z - 25;
                float Dist = Mathf.Sqrt(x_ * x_ + z_ * z_);
                float r = Values[1] * 17 + 5;
                // Craters
                Height += (r - Mathf.Min(r, Mathf.Abs(Dist - r))) * Values[0] * (1 + Mathf.PerlinNoise(x_ * 0.2f, z_ * 0.2f) * Values[2]);
                // Walls
                bool insideWallX = Mathf.Abs(x_) < Values[5] + Values[3] / 2;
                bool insideWallZ = Mathf.Abs(z_) < Values[5] + Values[3] / 2;
                bool xWall = insideWallX && Mathf.Abs(x_) > Values[5] - Values[3] / 2;
                bool zWall = insideWallZ && Mathf.Abs(z_) > Values[5] - Values[3] / 2;
                if ((xWall || zWall) && insideWallX && insideWallZ) {
                    Height += Values[4];
                }
                if (xWall && zWall && Values[4] > 1.5) {
                    Height += Values[6];
                }

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
