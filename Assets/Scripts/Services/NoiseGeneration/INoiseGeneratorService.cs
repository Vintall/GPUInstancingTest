using UnityEngine;

namespace Services.NoiseGeneration
{
    public interface INoiseGeneratorService
    {
        float GetHeight(Vector3 point);
    }
}