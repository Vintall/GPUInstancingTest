using UnityEngine;
using UnityEngine.Windows;

namespace Services.PlaneGeneration.Impls
{
    public class HeightTextureDrawer : MonoBehaviour
    {
        [SerializeField] private Color lowerColor;
        [SerializeField] Color higherColor;
            
        public void GenerateTexture(Vector3[][] vertices, int resolution)
        {
            var heightMap = new float[resolution][];

            for (var z = 0; z < resolution; ++z)
            {
                heightMap[z] = new float[resolution];
                
                for (var x = 0; x < resolution; ++x)
                    heightMap[z][x] = vertices[z][x].y;
            }

            GenerateTexture(heightMap, resolution);
        }
        
        public void GenerateTexture(float[][] heightMap, int resolution)
        {
            var texture2D = new Texture2D(resolution, resolution);

            NormalizeHeightMap(ref heightMap, resolution);
            
            for (var z = 0; z < resolution; ++z)
            for (var x = 0; x < resolution; ++x)
            {
                var finalColor = Color.Lerp(lowerColor, higherColor, heightMap[z][x]);
                texture2D.SetPixel(x, z, finalColor);
            }
            
            var path = $"C:\\Users\\Vintall\\Desktop\\Maps\\{Random.Range(0, 1000000).ToString()}.png";
            File.WriteAllBytes(path, texture2D.EncodeToPNG());
        }

        private void NormalizeHeightMap(ref float[][] heightMap, int resolution)
        {
            var minHeight = heightMap[0][0];
            var maxHeight = heightMap[0][0];
            
            for (var z = 0; z < resolution; ++z)
            for (var x = 1; x < resolution; ++x)
            {
                if (heightMap[z][x] < minHeight)
                    minHeight = heightMap[z][x];

                if (heightMap[z][x] > maxHeight)
                    maxHeight = heightMap[z][x];
            }
            
            var difference = maxHeight - minHeight;
            
            for (var z = 0; z < resolution; ++z)
            for (var x = 0; x < resolution; ++x)
                heightMap[z][x] = (heightMap[z][x] - minHeight) / difference;
        }
    }
}