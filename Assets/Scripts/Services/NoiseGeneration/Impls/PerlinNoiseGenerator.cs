using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Services.NoiseGeneration.Impls
{
    public class PerlinNoiseGenerator : MonoBehaviour
    {
        public PerlinNoiseGenerator()
        {
            Debug.Log($"[{typeof(PerlinNoiseGenerator)}] was initialized");
        }

        public float GeneratePoint(int seed, int span, Vector3 point)
        {
            var baseOffset = new Vector3(point.x % span, point.y % span, point.z % span);
            var basePoint = point - baseOffset;
            var endPoint = basePoint + Vector3.one * span;

            var edgePoints = new Vector3[]
            {
                basePoint,
                new Vector3(endPoint.x, basePoint.y, basePoint.z),
                new Vector3(basePoint.x, endPoint.y, basePoint.z),
                new Vector3(basePoint.x, basePoint.y, endPoint.z),
                new Vector3(endPoint.x, endPoint.y, basePoint.z),
                new Vector3(endPoint.x, basePoint.y, endPoint.z),
                new Vector3(basePoint.x, endPoint.y, endPoint.z),
                endPoint
            };
            var noisePoints = new Vector3[edgePoints.Length];

            for (var i = 0; i < edgePoints.Length; ++i)
            {
                var xh = (int)edgePoints[i].x;
                var yh = (int)edgePoints[i].y;
                var zh = (int)edgePoints[i].z;

                HashingVector(xh, yh, zh, seed);

                noisePoints[i] = HashVector;
            }

            var dotProduct = new float[]
            {
                DotGradient(noisePoints[0], edgePoints[0], point, span),
                DotGradient(noisePoints[1], edgePoints[1], point, span),
                DotGradient(noisePoints[2], edgePoints[2], point, span),
                DotGradient(noisePoints[3], edgePoints[3], point, span),
                DotGradient(noisePoints[4], edgePoints[4], point, span),
                DotGradient(noisePoints[5], edgePoints[5], point, span),
                DotGradient(noisePoints[6], edgePoints[6], point, span),
                DotGradient(noisePoints[7], edgePoints[7], point, span)
            };

            var xRelation = (point.x - edgePoints[0].x) / span;
            var yRelation = (point.y - edgePoints[0].y) / span;
            var zRelation = (point.z - edgePoints[0].z) / span;

            xRelation *= xRelation;
            yRelation *= yRelation;
            zRelation *= zRelation;

            var firstPlateInterpolation = Interpolate(
                Interpolate(dotProduct[0], dotProduct[1], xRelation),
                Interpolate(dotProduct[2], dotProduct[4], xRelation),
                yRelation);

            var secondPlateInterpolation = Interpolate(
                Interpolate(dotProduct[3], dotProduct[5], xRelation),
                Interpolate(dotProduct[6], dotProduct[7], xRelation),
                yRelation);

            var fullInterpolation = Interpolate(firstPlateInterpolation, secondPlateInterpolation, zRelation);

            return fullInterpolation / 2 + 0.5f;
        }

        private float DotGradient(Vector3 noiseVector, Vector3 gridPoint, Vector3 point, float span)
        {
            var delta = point - gridPoint;
            return Vector3.Dot(noiseVector, delta / span);
        }
        
        private int HashingVector(int x, int y, int z, int seed) => InitHash(x + InitHash(y + InitHash(z + InitHash(seed))));

        private int InitHash(int seed)
        {
            Random.InitState(seed);
            return Random.Range(0, 32769);
        }
        
        private Vector3 HashVector => 
            new Vector3(Random.Range(-1f, 1f),Random.Range(-1f, 1f),Random.Range(-1f, 1f));
        
        private float Interpolate(float a, float b, float w) =>
            (b - a) * (Mathf.Sin(Mathf.PI * (w - 0.5f)) / 2 + 0.5f) + a;
    }
}