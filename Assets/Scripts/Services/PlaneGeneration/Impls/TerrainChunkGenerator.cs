using System;
using System.Collections.Generic;
using DefaultNamespace;
using Services.NoiseGeneration;
using Services.NoiseGeneration.Impls;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Services.PlaneGeneration.Impls
{
    public class TerrainChunkGenerator : MonoBehaviour
    {
        [SerializeField] private float planeWidth;
        [SerializeField] private float planeHeight;
        [SerializeField] private int pointsPerX;
        [SerializeField] private int pointsPerY;
        [SerializeField] private TerrainChunk planeAsset;
        [SerializeField] private PerlinNoiseGenerator perlinNoiseGenerator;

        private void Start()
        {
            GeneratePlane();
        }

        public TerrainChunk GeneratePlane()
        {
            var heightMap = new Vector3[pointsPerY * pointsPerX];
            
            for (var y = 0; y < pointsPerY; ++y)
            {
                for (var x = 0; x < pointsPerX; ++x)
                {
                    var point = new Vector3(x * planeWidth / (pointsPerX - 1), 0, y * planeHeight / (pointsPerY - 1));
                    var height = perlinNoiseGenerator.GeneratePoint(0, 10, point + transform.position);
                    heightMap[y * pointsPerX + x] = new Vector3(point.x, height, point.z);
                }
            }

            var planeObject = Instantiate(planeAsset);
            var newMesh = new Mesh();
            var triangles = new List<int>();

            for (var y = 0; y < pointsPerY - 1; ++y)
            {
                for (var x = 0; x < pointsPerX - 1; ++x)
                {
                    triangles.Add(y * pointsPerX + x + 1);
                    triangles.Add(y * pointsPerX + x);
                    triangles.Add((y + 1) * pointsPerX + x);
                    
                    triangles.Add((y + 1) * pointsPerX + x + 1);
                    triangles.Add(y * pointsPerX + x + 1);
                    triangles.Add((y + 1) * pointsPerX + x);
                }
            }

            newMesh.vertices = heightMap;
            newMesh.triangles = triangles.ToArray();
            
            newMesh.vertices = heightMap;
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