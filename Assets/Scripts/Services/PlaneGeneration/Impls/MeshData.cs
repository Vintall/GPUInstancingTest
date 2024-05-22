using UnityEngine;

namespace Services.PlaneGeneration.Impls
{
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