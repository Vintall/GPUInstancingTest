using Services.PlaneGeneration.Impls;
using UnityEditor;
using UnityEngine;

namespace DefaultNamespace
{
    public class TerrainChunk : MonoBehaviour
    {
        public bool EnableDebugGizmos { get; set; }
        public bool EnableVertexPreview { get; set; }
        public bool EnableVertexOrderPreview { get; set; }
        public float VertexPreviewSphereRadius { get; set; }
        public float VertexOrderPreviewYOffset { get; set; }
        
        [Header("Components")]
        [SerializeField] private MeshFilter meshFilter;

        public MeshFilter MeshFilter => meshFilter;
        
        public MeshData MeshData { get; set; }
        
        private void OnDrawGizmos()
        {
            if (!EnableDebugGizmos)
                return;
            
            var mesh = meshFilter.mesh;
            var vertices = mesh.vertices;

            if (EnableVertexPreview)
            {
                Gizmos.color = Color.red;
                foreach (var vertex in vertices) 
                    Gizmos.DrawSphere(vertex + transform.position, VertexPreviewSphereRadius);
            }

            if (EnableVertexOrderPreview)
            {
                var flattenMatrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
                var translateVector = new Vector4(0, VertexOrderPreviewYOffset, 0, 0);

                for (var i = 0; i < vertices.Length - 1; ++i)
                {
                    Gizmos.color = Color.Lerp(Color.red, Color.blue, (float)i / vertices.Length);
                    var firstPoint = flattenMatrix * vertices[i] + translateVector;
                    var secondPoint = flattenMatrix * vertices[i + 1] + translateVector;
                    Gizmos.DrawRay(firstPoint, secondPoint - firstPoint);
                }
            }
        }

        public void SetMesh(Mesh mesh)
        {
            meshFilter.mesh = mesh;
        }
    }
    
    [CustomEditor(typeof(TerrainChunk))]
    public class TerrainChunkEditor : Editor
    {
        private bool enableDebugGizmos;
        private bool enableVertexPreview;
        private float vertexPreviewRadius;
        private bool enableVertexOrderPreview;
        private float vertexOrderPreviewYOffset;

        private TerrainChunk _target;

        private void Awake() => 
            _target = (TerrainChunk)target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DebugGizmos();
        }

        private void DebugGizmos()
        {
            var result = GUILayout.Toggle(enableDebugGizmos, "Enable Debug Gizmos");
            _target.EnableDebugGizmos = result;
            enableDebugGizmos = result;
            
            if (!result)
                return;
            
            VertexPreview();
            VertexOrderPreview();
        }

        private void VertexPreview()
        {
            var result = GUILayout.Toggle(enableVertexPreview, "Enable Vertex Preview");
            enableVertexPreview = result;
            _target.EnableVertexPreview = result;
            
            if(!result)
                return;
            
            var radius = GUILayout.HorizontalSlider(vertexPreviewRadius, 0, 2);
            GUILayout.Space(15);
            vertexPreviewRadius = radius;
            _target.VertexPreviewSphereRadius = vertexPreviewRadius;
        }

        private void VertexOrderPreview()
        {
            var result = GUILayout.Toggle(enableVertexOrderPreview, "Enable Vertex Order Preview");
            enableVertexOrderPreview = result;
            _target.EnableVertexOrderPreview = result;
            
            if(!result)
                return;
            
            var offset = GUILayout.HorizontalSlider(vertexOrderPreviewYOffset, 0, 10);
            GUILayout.Space(15);
            vertexOrderPreviewYOffset = offset;
            _target.VertexOrderPreviewYOffset = vertexOrderPreviewYOffset;
        }
    }
}