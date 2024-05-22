using UnityEngine;

namespace Services.PlaneGeneration.Impls
{
    public class MeshGenerator : MonoBehaviour
    {
        public void GenerateMesh(int points, )
        {
            
        }
        
        private T[][] GenerateGrid<T>()
        {
            var resultGrid = new T[Resolution][];

            for (var y = 0; y < Resolution; ++y)
            {
                resultGrid[y] = new T[Resolution];
            }

            return resultGrid;
        }

        private int[] GenerateTriangles(Vector2[][] grid)
        {
            for (var y = 0; y < Resolution - 1; ++y)
            {
                for (var x = 0; x < Resolution - 1; ++x)
                {
                    triangles.Add(y * Resolution + x + 1);
                    triangles.Add(y * Resolution + x);
                    triangles.Add((y + 1) * Resolution + x);
                    
                    triangles.Add((y + 1) * Resolution + x + 1);
                    triangles.Add(y * Resolution + x + 1);
                    triangles.Add((y + 1) * Resolution + x);
                }
            }
        }
    }
    
    public struct MeshData
    {
        public int Resolution;
        public float Size;
        public Vector3[][] Vertices;
        public int[] Triangles;

        public MeshData(int resolution, float size, Vector3[][] vertices, int[] triangles)
        {
            Resolution = resolution;
            Size = size;
            Vertices = vertices;
            Triangles = triangles;
        }
            
        
    }
}