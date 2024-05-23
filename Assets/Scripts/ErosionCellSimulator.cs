using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class ErosionCellSimulator : MonoBehaviour
    {
       [SerializeField] float waterDefaultVolume = 1f;
       [SerializeField] float waterEvaporation = 0.01f;
       [SerializeField] float gravity = 20f;
       [SerializeField] float minSlope = -4f;
       [SerializeField] float erosionSpeed = 0.9f;
       [SerializeField] float depositionSpeed = 0.02f;
       [SerializeField] int iterationsCount = 50;
       [SerializeField] float sedimentCapacity = 0.5f;
        
        private Vector3[][] _heightMap;
        private float[][] _waterLevel;

        private int _resolution;

        public Vector3[][] HeightMap => _heightMap;
        
        public ErosionCellSimulator(Vector3[][] heightMap)
        {
            _heightMap = heightMap;

            _resolution = heightMap.Length;
            
            _waterLevel = new float[heightMap.GetLength(0)][];

            for (var y = 0; y < _resolution; ++y)
                _waterLevel[y] = new float[_resolution];
        }

        public void SetupParameters()
        {
            SetupWaterLevel(10);
        }

        public void SetupSimulator(Vector3[][] heightMap)
        {
            _heightMap = heightMap;
            _resolution = heightMap.Length;
            _waterLevel = new float[heightMap.GetLength(0)][];

            for (var y = 0; y < _resolution; ++y)
                _waterLevel[y] = new float[_resolution];
        }

        public bool IsPointInBounds(Vector2Int point, float pointRadius, int resolution)
        {
            var pointLeftX = point.x - pointRadius;
            var pointRightX = point.x + pointRadius;
            var pointTopY = point.y - pointRadius;
            var pointBottomY = point.y + pointRadius;

            return pointLeftX >= 0 &&
                   pointRightX < resolution &&
                   pointTopY >= 0 &&
                   pointBottomY < resolution;
        }

        public Vector2Int[] GetPositionsInRadius(Vector2Int position, int pointRadius, int resolution)
        {
            var squareVolume = pointRadius * pointRadius * 4;
            var result = new List<Vector2Int>(squareVolume);

            for (var x = position.x - pointRadius; x < position.x + pointRadius; ++x)
            {
                for (var y = position.y - pointRadius; y < position.y + pointRadius; ++y)
                {
                    var positionInBound = new Vector2Int(x, y);

                    if (Vector2Int.Distance(position, positionInBound) <= pointRadius &&
                        IsPointInBounds(positionInBound, 1, resolution))
                        result.Add(positionInBound);
                }
            }
            
            return result.ToArray();
        }

        public struct Droplet
        {
            public Vector3 Position;
            public Vector3 Speed;
            public float WaterVolume;
            public float SedimentConcentration;
        }

        Vector3 GetSurfaceNormal(Vector2Int flooredPosition, Vector2 particlePosition) // x = x, y = z
        {
            var squareGrid = new Vector3[][]
            {
                new Vector3[]
                {
                    _heightMap[flooredPosition.y][flooredPosition.x],
                    _heightMap[flooredPosition.y + 1][flooredPosition.x]
                },
                new Vector3[]
                {
                    _heightMap[flooredPosition.y][flooredPosition.x + 1],
                    _heightMap[flooredPosition.y + 1][flooredPosition.x + 1]
                }
            };

            var distanceToFloor = Vector2.Distance(flooredPosition, particlePosition);
            var distanceToCeil = Vector2.Distance(squareGrid[1][1], particlePosition);


            if (distanceToFloor < distanceToCeil)
                return Vector3.Cross(squareGrid[0][1] - squareGrid[0][0], squareGrid[1][0] - squareGrid[0][0])
                    .normalized;
            
            if (distanceToFloor > distanceToCeil)
                return Vector3.Cross(squareGrid[1][0] - squareGrid[1][1], squareGrid[0][1] - squareGrid[1][1])
                    .normalized;

            var normal = ((squareGrid[1][0] + squareGrid[0][1] - squareGrid[0][0] - squareGrid[1][1]) / 2).normalized;

            if (normal.y < 0)
                normal *= -1;

            return normal;
        }

        public void SimulateDroplet(Vector2 currentPosition)
        {
            var iterations = iterationsCount;
            var droplet = new Droplet()
            {
                Position = new Vector3(currentPosition.x, 0, currentPosition.y),
                Speed = Vector3.zero,
                WaterVolume = waterDefaultVolume,
                SedimentConcentration = 0
            };

            var friction = 0.01f;

            while (iterations > 0 && droplet.WaterVolume > 0)
            {
                var flooredPosition = Vector2Int.FloorToInt(new Vector2(droplet.Position.x, droplet.Position.z));
                /*var gridPositions = new Vector2Int[]
                {
                    flooredPosition,
                    flooredPosition + Vector2Int.up,
                    flooredPosition + Vector2Int.right,
                    flooredPosition + Vector2Int.up + Vector2Int.right
                };
                var nearestPosition = gridPositions[0];
                var nearestDistance = Vector2.Distance(droplet.Position, nearestPosition);

                for (var i = 1; i < gridPositions.Length; ++i)
                {
                    var newDistance = Vector2.Distance(gridPositions[i], droplet.Position);

                    if (newDistance >= nearestDistance)
                        continue;

                    nearestDistance = newDistance;
                    nearestPosition = gridPositions[i];
                }

                */
                
                if (flooredPosition.x < 0 ||
                    flooredPosition.x >= _resolution - 1 ||
                    flooredPosition.y < 0 ||
                    flooredPosition.y >= _resolution - 1)
                    return;

                //var normal = GetSurfaceNormal(nearestPosition, droplet.Position);
                var normal = GetSurfaceNormal(flooredPosition, new Vector2(droplet.Position.x, droplet.Position.z));
                
                //Accelerate particle using newtonian mechanics using the surface normal.
                var acceleration = normal;
                droplet.Speed += acceleration; //F = ma, so a = F/m
                droplet.Position += droplet.Speed;
                droplet.Speed *= 1.0f - friction; //Friction Factor

                if ((int)droplet.Position.x <= 0 ||
                    (int)droplet.Position.x > _resolution - 1 ||
                    (int)droplet.Position.z <= 0 ||
                    (int)droplet.Position.z > _resolution - 1)
                    return;

                //Compute sediment capacity difference
                var maxsediment = droplet.WaterVolume * droplet.Speed.magnitude *
                                    (_heightMap[flooredPosition.y][flooredPosition.x].y -
                                     _heightMap[(int)droplet.Position.z][(int)droplet.Position.x].y);
                
                if (maxsediment < 0.0)
                    maxsediment = 0;
                        
                //var sdiff = maxsediment - droplet.SedimentConcentration;
                var sdiff = 0.1f - droplet.SedimentConcentration;

                //Act on the Heightmap and Droplet!

                droplet.SedimentConcentration += depositionSpeed * sdiff;
                _heightMap[flooredPosition.y][flooredPosition.x] -= Vector3.up * (droplet.WaterVolume * depositionSpeed * sdiff);

                //Evaporate the Droplet (Note: Proportional to Volume! Better: Use shape factor to make proportional to the area instead.)
                droplet.WaterVolume *= (1.0f - waterEvaporation);

                 --iterations;
            }
        }





        public void SimulateStep()
        {
            var distance = 0.5f;
            var newWaterLevel = new float[_resolution][];
            var newHeightMap = new float[_resolution][];
            
            for (var y = 0; y < _resolution; ++y)
            {
                newWaterLevel[y] = new float[_resolution];
                newHeightMap[y] = new float[_resolution];
            }

            for (var y = 1; y < _resolution - 1; ++y)
            {
                for (var x = 1; x < _resolution - 1; ++x)
                {
                    var currentHeight = _heightMap[y][x];
                    var leftHeight = _heightMap[y][x - 1];
                    var rightHeight = _heightMap[y][x + 1];
                    var topHeight = _heightMap[y - 1][x];
                    var bottomHeight = _heightMap[y + 1][x];
                    
                    var currentWaterLevel = _waterLevel[y][x];
                    var leftWaterLevel = _waterLevel[y][x - 1];
                    var rightWaterLevel = _waterLevel[y][x + 1];
                    var topWaterLevel = _waterLevel[y - 1][x];
                    var bottomWaterLevel = _waterLevel[y + 1][x];

                    //var leftSlope = CalculateSlope(currentHeight, leftHeight, distance);
                    //var rightSlope = CalculateSlope(currentHeight, rightHeight, distance);
                    //var topSlope = CalculateSlope(currentHeight, topHeight, distance);
                    //var bottomSlope = CalculateSlope(currentHeight, bottomHeight, distance);

                    // var leftSlope = currentWaterLevel - leftWaterLevel;
                    // var rightSlope = currentWaterLevel - rightWaterLevel;
                    // var topSlope = currentWaterLevel - topWaterLevel;
                    // var bottomSlope = currentWaterLevel - bottomWaterLevel;

                    var outflow = 0f;
                    var inflow = 0f;

                    //if (leftSlope < 0)
                    //    outflow += leftWaterLevel;
                    //
                    //if (rightSlope < 0)
                    //    outflow += rightWaterLevel;
                    //
                    //if (topSlope < 0)
                    //    outflow += topWaterLevel;
                    //
                    //if (bottomSlope < 0)
                    //    outflow += bottomWaterLevel;
                    //
                    //if (leftSlope > 0)
                    //    inflow += leftWaterLevel;
                    //
                    //if (rightSlope > 0)
                    //    inflow += rightWaterLevel;
                    //
                    //if (topSlope > 0)
                    //    inflow += topWaterLevel;
                    //
                    //if (bottomSlope > 0)
                    //    inflow += bottomWaterLevel;

                    newWaterLevel[y][x] = currentWaterLevel - outflow + inflow;
                    //newHeightMap[y][x] = _heightMap[y][x] - (Mathf.Abs(outflow) + Mathf.Abs(inflow));

                    if (newWaterLevel[y][x] <= 0)
                        newWaterLevel[y][x] = 0;
                }
            }

            _waterLevel = newWaterLevel;
            //_heightMap = newHeightMap;
        }

        private float CalculateSlope(float currentCell, float neighbourCell, float distance)
        {
            return (neighbourCell - currentCell) / distance;
        }

        public void SetupWaterLevel(float constantLevel)
        {
            for (var y = 0; y < _resolution; ++y)
            {
                for (var x = 0; x < _resolution; ++x)
                {
                    _waterLevel[y][x] = constantLevel;
                }
            }
        }
    }
}