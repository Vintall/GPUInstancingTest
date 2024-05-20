using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class ErosionCellSimulator : MonoBehaviour
    {
       [SerializeField] float distance = 0.5f;

       [SerializeField] float waterDefaultVolume = 1f;
       [SerializeField] float waterEvaporation = 0.01f;
       [SerializeField] float gravity = 20f;
       [SerializeField] float minSlope = -4f;
       [SerializeField] float erosionSpeed = 0.9f;
       [SerializeField] float depositionSpeed = 0.02f;
       [SerializeField] int iterationsCount = 50;
       [SerializeField] float sedimentCapacity = 0.5f;
        
        private float[][] _heightMap;
        private float[][] _waterLevel;

        private int _xLength;
        private int _yLength;

        public float[][] HeightMap => _heightMap;
        
        public ErosionCellSimulator(float[][] heightMap)
        {
            _heightMap = heightMap;

            _yLength = heightMap.Length;
            _xLength = heightMap[0].Length;
            
            _waterLevel = new float[heightMap.GetLength(0)][];

            for (var y = 0; y < _yLength; ++y)
                _waterLevel[y] = new float[_xLength];
        }

        public void SetupParameters()
        {
            SetupWaterLevel(10);
        }

        public void SetupSimulator(float[][] heightMap)
        {
            _heightMap = heightMap;

            _yLength = heightMap.Length;
            _xLength = heightMap[0].Length;
            
            _waterLevel = new float[heightMap.GetLength(0)][];

            for (var y = 0; y < _yLength; ++y)
                _waterLevel[y] = new float[_xLength];
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

        public Vector2Int[] GetPositionsInRadius(Vector2Int position, int pointRadius)
        {
            var squareVolume = pointRadius * pointRadius * 4;
            var result = new List<Vector2Int>(squareVolume);

            for (var x = position.x - pointRadius; x < position.x + pointRadius; ++x)
            {
                for (var y = position.y - pointRadius; y < position.y + pointRadius; ++y)
                {
                    var positionInBound = new Vector2Int(x, y);

                    if (Vector2Int.Distance(position, positionInBound) <= pointRadius)
                        result.Add(positionInBound);
                }
            }
            
            return result.ToArray();
        }
        
        public void SimulateDroplet(Vector2Int currentPosition)
        {
            var iterations = iterationsCount;
            var waterVolume = waterDefaultVolume;
            var carriedSediment = 0f;
            var speed = 0f;
            var dropletRadius = 5;

            while (iterations > 0 && waterVolume > 0)
            {
                var currentHeight = _heightMap[currentPosition.x][currentPosition.y];

                var leftPosition = currentPosition + Vector2Int.left;
                var rightPosition = currentPosition + Vector2Int.right;
                var topPosition = currentPosition - Vector2Int.up;
                var bottomPosition = currentPosition - Vector2Int.down;
                
                var lowestPosition = currentPosition;
                var currentLowestHeight = currentHeight;

                // Check for lowest point
                if (leftPosition.x >= 0)
                {
                    var leftHeight = _heightMap[leftPosition.x][leftPosition.y];
                    
                    
                    if (leftHeight < currentLowestHeight)
                    {
                        lowestPosition = leftPosition;
                        currentLowestHeight = leftHeight;
                    }
                }

                if (rightPosition.x < _xLength)
                {
                    var rightHeight = _heightMap[rightPosition.x][rightPosition.y];
                    
                    if (rightHeight < currentLowestHeight)
                    {
                        lowestPosition = rightPosition;
                        currentLowestHeight = rightHeight;
                    }
                }

                if (topPosition.y >= 0)
                {
                    var topHeight = _heightMap[topPosition.x][topPosition.y];
                    
                    if (topHeight < currentLowestHeight)
                    {
                        lowestPosition = topPosition;
                        currentLowestHeight = topHeight;
                    }
                }

                if (bottomPosition.y < _yLength)
                {
                    var bottomHeight = _heightMap[bottomPosition.x][bottomPosition.y];
                    
                    if (bottomHeight < currentLowestHeight)
                    {
                        lowestPosition = bottomPosition;
                    }
                }
                
                
                var lowestHeight = _heightMap[lowestPosition.x][lowestPosition.y];
                var lowestSlope = Mathf.Min(minSlope, CalculateSlope(currentHeight, lowestHeight, distance));
                var heightDifference = currentHeight - lowestHeight;
                var slopeStrength = lowestSlope / minSlope;

                var sedimentQuantity = carriedSediment / (sedimentCapacity * waterVolume);
                var sedimentAccumulationRate = 1 - sedimentQuantity;
                var sedimentDepositionRate = sedimentQuantity;

                var takenSediment =
                    Mathf.Abs(sedimentAccumulationRate * heightDifference/* * erosionSpeed * slopeStrength*/);
                var depositedSediment =
                    carriedSediment * sedimentDepositionRate * depositionSpeed * (1 - slopeStrength);
                
                if (takenSediment > heightDifference)
                    takenSediment = heightDifference;
                
                if (carriedSediment > (sedimentCapacity * waterVolume))
                    carriedSediment = (sedimentCapacity * waterVolume);


                var totalSedimentDelta = (depositedSediment - takenSediment) / 15;
                carriedSediment -= totalSedimentDelta;
                _heightMap[currentPosition.x][currentPosition.y] += totalSedimentDelta;
                
                currentPosition = lowestPosition;
                
                waterVolume -= waterEvaporation;
                --iterations;
            }
        }
        
        
        
        
        
        public void SimulateStep()
        {
            var distance = 0.5f;
            var newWaterLevel = new float[_yLength][];
            var newHeightMap = new float[_yLength][];
            
            for (var y = 0; y < _yLength; ++y)
            {
                newWaterLevel[y] = new float[_xLength];
                newHeightMap[y] = new float[_xLength];
            }

            for (var y = 1; y < _yLength - 1; ++y)
            {
                for (var x = 1; x < _xLength - 1; ++x)
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

                    var leftSlope = CalculateSlope(currentHeight, leftHeight, distance);
                    var rightSlope = CalculateSlope(currentHeight, rightHeight, distance);
                    var topSlope = CalculateSlope(currentHeight, topHeight, distance);
                    var bottomSlope = CalculateSlope(currentHeight, bottomHeight, distance);

                    // var leftSlope = currentWaterLevel - leftWaterLevel;
                    // var rightSlope = currentWaterLevel - rightWaterLevel;
                    // var topSlope = currentWaterLevel - topWaterLevel;
                    // var bottomSlope = currentWaterLevel - bottomWaterLevel;

                    var outflow = 0f;
                    var inflow = 0f;

                    if (leftSlope < 0)
                        outflow += leftWaterLevel;
                    
                    if (rightSlope < 0)
                        outflow += rightWaterLevel;
                    
                    if (topSlope < 0)
                        outflow += topWaterLevel;
                    
                    if (bottomSlope < 0)
                        outflow += bottomWaterLevel;
                    
                    if (leftSlope > 0)
                        inflow += leftWaterLevel;
                    
                    if (rightSlope > 0)
                        inflow += rightWaterLevel;
                    
                    if (topSlope > 0)
                        inflow += topWaterLevel;
                    
                    if (bottomSlope > 0)
                        inflow += bottomWaterLevel;

                    newWaterLevel[y][x] = currentWaterLevel - outflow + inflow;
                    newHeightMap[y][x] = _heightMap[y][x] - (Mathf.Abs(outflow) + Mathf.Abs(inflow));

                    if (newWaterLevel[y][x] <= 0)
                        newWaterLevel[y][x] = 0;
                }
            }

            _waterLevel = newWaterLevel;
            _heightMap = newHeightMap;
        }

        private float CalculateSlope(float currentCell, float neighbourCell, float distance)
        {
            return (neighbourCell - currentCell) / distance;
        }

        public void SetupWaterLevel(float constantLevel)
        {
            for (var y = 0; y < _yLength; ++y)
            {
                for (var x = 0; x < _xLength; ++x)
                {
                    _waterLevel[y][x] = constantLevel;
                }
            }
        }
    }
}