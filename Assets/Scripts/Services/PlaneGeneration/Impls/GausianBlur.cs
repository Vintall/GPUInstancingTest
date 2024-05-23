using UnityEngine;

namespace Services.PlaneGeneration.Impls
{
    public class GausianBlur : MonoBehaviour
    {
        public void ApplyGaussianBlur(ref Vector3[][] heightMap, int resolution)
        {
            var newHeightMap = new Vector3[resolution][];

            for (var z = 0; z < resolution; ++z)
            {
                newHeightMap[z] = new Vector3[resolution];
                
                for (var x = 0; x < resolution; ++x) 
                    newHeightMap[z][x] = heightMap[z][x];
            }
            
            for (var z = 1; z < resolution - 1; ++z)
            {
                for (var x = 1; x < resolution - 1; ++x)
                {
                    var bluredValue =
                        heightMap[z][x].y * 0.6f +
                        heightMap[z][x + 1].y * 0.075f +
                        heightMap[z + 1][x].y * 0.075f +
                        heightMap[z][x - 1].y * 0.075f +
                        heightMap[z - 1][x].y * 0.075f +
                        heightMap[z + 1][x + 1].y * 0.025f +
                        heightMap[z + 1][x - 1].y * 0.025f +
                        heightMap[z - 1][x + 1].y * 0.025f +
                        heightMap[z - 1][x - 1].y * 0.025f;

                    newHeightMap[z][x] = new Vector3(heightMap[z][x].x, bluredValue, heightMap[z][x].z);
                }
            }

            heightMap = newHeightMap;
        }
    }
}