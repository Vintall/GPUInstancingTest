using UnityEngine;

namespace Services.PlaneGeneration.Impls
{
    public class MeshDataGenerator : MonoBehaviour
    {
        public MeshData GenerateMesh(int resolution, float size)
        {
            var meshData = new MeshData();
            var vertices = GenerateGridArray<Vector3>(resolution);
            var triangles = GenerateTriangles(resolution);
            
            GeneratePointsGrid(ref vertices, resolution, size);
            meshData.Vertices = vertices;
            meshData.Triangles = triangles;
            meshData.Resolution = resolution;
            meshData.Size = size;

            return meshData;
        }

        private T[][] GenerateGridArray<T>(int resolution)
        {
            var resultGrid = new T[resolution][];

            for (var x = 0; x < resolution; ++x) 
                resultGrid[x] = new T[resolution];

            return resultGrid;
        }

        private void GeneratePointsGrid(ref Vector3[][] vertices, int resolution, float size)
        {
            for (var z = 0; z < resolution; ++z)
            for (var x = 0; x < resolution; ++x)
            {
                var point = new Vector3(x * size / (resolution - 1), 0, z * size / (resolution - 1));
                vertices[z][x] = point;
            }
        }

        private int[] GenerateTriangles(int resolution)
        {
            var verticesCount = (resolution - 1) * (resolution - 1) * 6;
            var triangles = new int[verticesCount];
            
            for (int z = 0, i = 0; z < resolution - 1; ++z)
            {
                for (var x = 0; x < resolution - 1; ++x)
                {
                    triangles[i++] = z * resolution + x;
                    triangles[i++] = (z + 1) * resolution + x;
                    triangles[i++] = z * resolution + x + 1;
                    
                    triangles[i++] = (z + 1) * resolution + x + 1;
                    triangles[i++] = z * resolution + x + 1;
                    triangles[i++] = (z + 1) * resolution + x;
                }
            }

            return triangles;
        }
    }
}