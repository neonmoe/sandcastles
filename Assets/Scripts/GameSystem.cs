using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;

public class GameSystem : MonoBehaviour {
    public MeshInstanceRendererComponent SandMesh;
    public UIController UIController;

    private EntityArchetype SandArchetype;
    private MeshInstanceRenderer SandRenderer;
    private List<Entity> Entities = new List<Entity>();

    private List<float> Values = new List<float>();
    private List<float> LastValues = new List<float>();

    private int GeneratedWorlds = 0;

    private void Start() {
        EntityManager EntityManager = World.Active.GetOrCreateManager<EntityManager>();
        SandArchetype = EntityManager.CreateArchetype(typeof(Sand), typeof(Position),
            typeof(TransformMatrix), typeof(MeshInstanceRendererComponent));
        SandRenderer = SandMesh.Value;
        Destroy(SandMesh.gameObject);
    }

    private void Update() {
        if (Input.GetButtonDown("Upvote") && Values.Count > 0) {
            UIController.HideTutorial1();
            LastValues = new List<float>(Values);
            UIController.SetSavedBars(LastValues);
        }
        if (Input.GetButtonDown("Reset Upvote")) {
            LastValues.Clear();
            UIController.DiscardSavedValues();
        }
        if (Input.GetButtonDown("Generate World")) {
            CreateWorld();
            UIController.HideTutorial0();
            GeneratedWorlds++;
            if (GeneratedWorlds > 2) {
                UIController.ShowTutorial1();
            }
        }
    }

    private float ApplyGenes(float initialVal) {
        if (LastValues.Count > 0) {
            float GeneValue = LastValues[Values.Count] - 0.5f;
            float NewValue = initialVal - 0.5f;
            return (NewValue + GeneValue) / 2f + 0.5f;
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
        /* What the indices will change:
         * 0: Crater Strength
         * 1: Crater Size
         * 2: Crater Noisyness
         * 3: Wall Width
         * 4: Wall Height
         * 5: Wall Radius
         * 6: Wall Tower Height
         */
        for (int i = 0; i < 7; i++) {
            Values.Add(ApplyGenes(Random.value));
        }
        UIController.SetCurrentBars(Values);

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
                float WallHalfWidth = (Values[3] * 5 + 3) / 2f;
                float WallRadius = Values[5] * 7 + 8;
                bool insideWallX = Mathf.Abs(x_) < WallRadius + WallHalfWidth;
                bool insideWallZ = Mathf.Abs(z_) < WallRadius + WallHalfWidth;
                bool xWall = insideWallX && Mathf.Abs(x_) > WallRadius - WallHalfWidth;
                bool zWall = insideWallZ && Mathf.Abs(z_) > WallRadius - WallHalfWidth;
                if ((xWall || zWall) && insideWallX && insideWallZ) {
                    Height += Mathf.Max(0, Values[4] * 5 - 2);
                }
                if (xWall && zWall && Values[4] > 1.5) {
                    Height += Values[6] * 4 + 1;
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
