using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using NoiseTest;
using Services.NoiseGeneration.Impls;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Windows;

namespace Services.PlaneGeneration.Impls
{
    public class TerrainChunkGenerator : MonoBehaviour
    {
        [SerializeField] private TerrainChunk planeAsset;
        [SerializeField] private PerlinNoiseGenerator perlinNoiseGenerator;
        [SerializeField] private ErosionCellSimulator erosionCellSimulator;
        [SerializeField] private HeightTextureDrawer heightTextureDrawer;
        
        [SerializeField] private float planeSize;
        [SerializeField] private int resolution;
        [SerializeField] private int iterationsCount;
        [SerializeField] private MeshDataGenerator meshDataGenerator;
        [SerializeField] private bool useErrosion;
        [SerializeField] private Transform terrainChunksHolder;
        private TerrainChunk lastGenerated;
        
        public void ApplyPerlin(ref Vector3[][] grid)
        {
            var openSimplexNoise = new OpenSimplexNoise(3248);
            
            for (var z = 0; z < resolution; ++z)
            for (var x = 0; x < resolution; ++x)
            {
                var point = grid[z][x];
                point.y = (float)openSimplexNoise.Evaluate(point.x, point.z);
                grid[z][x] = point;
            }
        }

        public TerrainChunk GeneratePlane()
        {
            var meshData = meshDataGenerator.GenerateMesh(resolution, planeSize);
            
            ApplyPerlin(ref meshData.Vertices);
            
            var planeObject = Instantiate(planeAsset, terrainChunksHolder);
            var newMesh = new Mesh();


            if (useErrosion)
            {
                planeObject.name += " Erroded";

                erosionCellSimulator.SetupSimulator(meshData.Vertices);

                for (var i = 0; i < iterationsCount; ++i)
                {
                    var position = new Vector2(Random.Range(0f, resolution - 1), Random.Range(0f, resolution - 1));
                    erosionCellSimulator.SimulateDroplet(position);

                }
            }

            heightTextureDrawer.GenerateTexture(meshData.Vertices, resolution);
            
            var heightMapLinear = new Vector3[resolution * resolution];

            for (var z = 0; z < resolution; ++z)
            for (var x = 0; x < resolution; ++x)
                heightMapLinear[z * resolution + x] = meshData.Vertices[z][x];

            newMesh.vertices = heightMapLinear;
            newMesh.triangles = meshData.Triangles;
            newMesh.RecalculateNormals();
            newMesh.RecalculateTangents();
            newMesh.RecalculateBounds();
            
            planeObject.MeshFilter.mesh = newMesh;

            lastGenerated = planeObject;
            
            return planeObject;
        }
    }

    [CustomEditor(typeof(TerrainChunkGenerator))]
    public class PlaneGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("RegeneratePlane"))
            {
                (target as TerrainChunkGenerator).GeneratePlane();
            }
        }
    }
}