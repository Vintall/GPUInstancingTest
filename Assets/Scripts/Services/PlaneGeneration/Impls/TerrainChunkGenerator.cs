using System.Collections.Generic;
using DefaultNamespace;
using NoiseTest;
using Services.NoiseGeneration.Impls;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace Services.PlaneGeneration.Impls
{
    public class TerrainChunkGenerator : MonoBehaviour
    {
        [SerializeField] private float planeSize;
        [SerializeField] private int pointsPerAxis;
        [SerializeField] private int iterationsCount;
        [SerializeField] private TerrainChunk planeAsset;
        [SerializeField] private PerlinNoiseGenerator perlinNoiseGenerator;
        [SerializeField] private ErosionCellSimulator erosionCellSimulator;
        [SerializeField] private bool useErrosion;
        [SerializeField] private Transform terrainChunksHolder;
            
        private void Start()
        {
            
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
            var openSimplexNoise = new OpenSimplexNoise(3248);
            
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
            
            var planeObject = Instantiate(planeAsset, terrainChunksHolder);
            var newMesh = new Mesh();
            var triangles = new List<int>();
            var texture2D = new Texture2D(pointsPerAxis, pointsPerAxis);
            
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

            var minHeight = 0f;
            var maxHeight = 0f;

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

                erosionCellSimulator.SetupSimulator(heightMapForErosion);

                for (var i = 0; i < iterationsCount; ++i)
                {
                    var position = new Vector2Int(Random.Range(0, pointsPerAxis), Random.Range(0, pointsPerAxis));
                    erosionCellSimulator.SimulateDroplet(position);
                }

                minHeight = heightMap[0][0].y;
                maxHeight = heightMap[0][0].y;

                for (var y = 0; y < pointsPerAxis; ++y)
                {
                    for (var x = 0; x < pointsPerAxis; ++x)
                    {
                        heightMap[y][x] = new Vector3(heightMap[y][x].x, erosionCellSimulator.HeightMap[y][x],
                            heightMap[y][x].z);

                        if (heightMap[y][x].y < minHeight)
                            minHeight = heightMap[y][x].y;
                        
                        if (heightMap[y][x].y > maxHeight)
                            maxHeight = heightMap[y][x].y;
                    }
                }
            }

            var difference = maxHeight - minHeight;
            
            for (var y = 0; y < pointsPerAxis; ++y)
            {
                for (var x = 0; x < pointsPerAxis; ++x)
                {
                    var colorValue = (heightMap[y][x].y - minHeight) / difference;
                    texture2D.SetPixel(y, x, new Color(colorValue, colorValue, colorValue));
                }
            }
            
            //var path = $"{Application.dataPath}\\Maps\\{Random.Range(0, 100000).ToString()}.png";
            var path = $"C:\\Users\\Vintall\\Desktop\\Maps\\{Random.Range(0, 100000).ToString()}.png";
            
            File.WriteAllBytes(path, texture2D.EncodeToPNG());
            
            
            
            var heightMapLinear = new Vector3[pointsPerAxis * pointsPerAxis];

            for (var y = 0; y < pointsPerAxis; ++y) 
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