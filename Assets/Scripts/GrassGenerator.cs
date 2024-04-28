using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GrassGenerator : MonoBehaviour
{
    [SerializeField] private RenderTexture _sprite;
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    public RenderTexture Sprite => _sprite;

    public Mesh Mesh => _mesh;

    public Material Material => _material;

    void Start()
    {
        
    }
    
    void Update()
    {
        
    }
}
[CustomEditor(typeof(GrassGenerator))]
public class Test : Editor
{
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (Event.current.type.Equals(EventType.Repaint))
        {
            Graphics.DrawTexture(new Rect(10, 10, 300, 300), (target as GrassGenerator).Sprite);
        }
    }

    private void Func(int id)
    {
        
    }
}
