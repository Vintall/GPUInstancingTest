using System;
using System.Collections.Generic;
using DefaultNamespace;
using NoiseTest;
using Services.NoiseGeneration;
using Services.NoiseGeneration.Impls;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Services.PlaneGeneration.Impls
{
    public class TerrainChunkGenerator : MonoBehaviour
    {
        [SerializeField] private float planeSize;
        [SerializeField] private int pointsPerAxis;
        [SerializeField] private int iterationsCount;
        [SerializeField] private TerrainChunk planeAsset;
        [SerializeField] private PerlinNoiseGenerator perlinNoiseGenerator;
        [SerializeField] private bool useErrosion;
        private void Start()
        {
            GeneratePlane();
        }

        public T[][] GenerateGrid<T>()
        {
            var resultGrid = new T[pointsPerAxis][];

            for (var y = 0; y < pointsPerAxis; ++y)
            {
                resultGrid[y] = new T[pointsPerAxis];
            }

            return resultGrid;
        }

        public void ApplyPerlin(ref Vector3[][] grid)
        {
            OpenSimplexNoise openSimplexNoise = new OpenSimplexNoise(3248);
            //var halfSize = size / 2;
            //var stepBetweenPoints = size / pointsPerAxis;
            
            for (var y = 0; y < pointsPerAxis; ++y)
            {
                for (var x = 0; x < pointsPerAxis; ++x)
                {
                    var point = grid[y][x];
                    point.y = (float)openSimplexNoise.Evaluate(point.x, point.z);
                    grid[y][x] = point;
                }
            }
        }

        public TerrainChunk GeneratePlane()
        {
            var heightMap = GenerateGrid<Vector3>();
            
             for (var y = 0; y < pointsPerAxis; ++y)
             {
                 for (var x = 0; x < pointsPerAxis; ++x)
                 {
                     var point = new Vector3(x * planeSize / (pointsPerAxis - 1), 0, y * planeSize / (pointsPerAxis - 1));
                     heightMap[y][x] = point;
                 }
             }

            ApplyPerlin(ref heightMap);
            
            var planeObject = Instantiate(planeAsset);
            var newMesh = new Mesh();
            var triangles = new List<int>();
            
            for (var y = 0; y < pointsPerAxis - 1; ++y)
            {
                for (var x = 0; x < pointsPerAxis - 1; ++x)
                {
                    triangles.Add(y * pointsPerAxis + x + 1);
                    triangles.Add(y * pointsPerAxis + x);
                    triangles.Add((y + 1) * pointsPerAxis + x);
                    
                    triangles.Add((y + 1) * pointsPerAxis + x + 1);
                    triangles.Add(y * pointsPerAxis + x + 1);
                    triangles.Add((y + 1) * pointsPerAxis + x);
                }
            }

            if (useErrosion)
            {
                planeObject.name += " Erroded";
                var heightMapForErosion = GenerateGrid<float>();

                for (var y = 0; y < pointsPerAxis; ++y)
                {
                    for (var x = 0; x < pointsPerAxis; ++x)
                    {
                        heightMapForErosion[y][x] = heightMap[y][x].y;
                    }
                }

                ErosionCellSimulator erosionCellSimulator = new ErosionCellSimulator(heightMapForErosion);

                for (var i = 0; i < iterationsCount; ++i)
                {
                    erosionCellSimulator.SimulateDroplet();
                }

                for (var y = 0; y < pointsPerAxis; ++y)
                {
                    for (var x = 0; x < pointsPerAxis; ++x)
                    {
                        heightMap[y][x] = new Vector3(heightMap[y][x].x, erosionCellSimulator.HeightMap[y][x],
                            heightMap[y][x].z);
                    }
                }
            }

            var heightMapLinear = new Vector3[pointsPerAxis * pointsPerAxis];
            
            for(var y = 0;y<pointsPerAxis;++y)
            for (var x = 0; x < pointsPerAxis; ++x)
            {
                heightMapLinear[y * pointsPerAxis + x] = heightMap[y][x];
            }
            
            newMesh.vertices = heightMapLinear;
            newMesh.triangles = triangles.ToArray();
            newMesh.RecalculateNormals();
            newMesh.RecalculateTangents();
            newMesh.RecalculateBounds();
            
            planeObject.MeshFilter.mesh = newMesh;

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