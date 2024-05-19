using UnityEngine;

namespace DefaultNamespace
{
    public class ErosionCellSimulator : IErosionCellSimulator
    {
        private float[][] _heightMap;
        private float[][] _waterLevel;

        private readonly int _xLength;
        private readonly int _yLength;

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

        public void SimulateDroplet()
        {
            var distance = 0.5f;

            var currentPosition = new Vector2Int(Random.Range(0, _xLength), Random.Range(0, _yLength));
            var waterVolume = 1f;
            var waterEvaporation = 0.001f;
            var gravity = 20f;
            var minSlope = -4f;
            var erosionSpeed = 0.9f;
            var depositionSpeed = 0.02f;
            var iterations = 50;
            var carriedSediment = 0f;

            while (iterations > 0)
            {
                var currentHeight = _heightMap[currentPosition.x][currentPosition.y];

                var leftPosition = currentPosition + Vector2Int.left;
                var rightPosition = currentPosition + Vector2Int.right;
                var topPosition = currentPosition - Vector2Int.up;
                var bottomPosition = currentPosition - Vector2Int.down;
                
                var outflow = 0f;
                var inflow = 0f;

                var lowestPosition = currentPosition;

                if (leftPosition.x >= 0)
                {
                    var leftHeight = _heightMap[leftPosition.x][leftPosition.y];
                    
                    if (leftHeight < currentHeight)
                        lowestPosition = leftPosition;
                }

                if (rightPosition.x < _xLength)
                {
                    var rightHeight = _heightMap[rightPosition.x][rightPosition.y];
                    
                    if (rightHeight < currentHeight)
                        lowestPosition = rightPosition;
                }

                if (topPosition.y >= 0)
                {
                    var topHeight = _heightMap[topPosition.x][topPosition.y];
                    
                    if (topHeight < currentHeight)
                        lowestPosition = topPosition;
                }

                if (bottomPosition.y < _yLength)
                {
                    var bottomHeight = _heightMap[bottomPosition.x][bottomPosition.y];
                    
                    if (bottomHeight < currentHeight)
                        lowestPosition = bottomPosition;
                }
                
                var lowestHeight = _heightMap[lowestPosition.x][lowestPosition.y];
                var lowestSlope = Mathf.Min(minSlope, CalculateSlope(currentHeight, lowestHeight, distance));
                var heightDifference = currentHeight - lowestHeight;

                var takenSediment = Mathf.Abs(lowestSlope * heightDifference * erosionSpeed);
                
                if (takenSediment > heightDifference / 3)
                    takenSediment = heightDifference / 3;

                carriedSediment += takenSediment;

                var depositedSediment = carriedSediment * depositionSpeed / Mathf.Max(0.1f, 1 - (lowestSlope / 4));

                _heightMap[currentPosition.x][currentPosition.y] += depositedSediment - takenSediment;

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