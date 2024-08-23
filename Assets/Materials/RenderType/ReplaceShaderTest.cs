using UnityEditor;
using UnityEngine;

namespace Materials.RenderType
{
    public class ReplaceShaderTest : MonoBehaviour
    {
        [SerializeField] private Shader[] shaders;
        [SerializeField] private Camera camera;
    
        public void ReplaceShader(int shaderId)
        {
            Debug.Log($"Shader {shaderId}");
            camera.SetReplacementShader(shaders[shaderId], "Opaque");
        }

        public void Reset()
        {
            
        }
    }

    [CustomEditor(typeof(ReplaceShaderTest))]
    public class ReplaceShaderTestEditor : Editor
    {
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var targetClass = target as ReplaceShaderTest;

            if (GUILayout.Button("Set Shader 1"))
                targetClass.ReplaceShader(0);
            
            if (GUILayout.Button("Set Shader 2"))
                targetClass.ReplaceShader(1);
            
            if (GUILayout.Button("Reset"))
                targetClass.Reset();
        }
    }
}