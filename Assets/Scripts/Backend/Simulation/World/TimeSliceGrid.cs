using System;
using System.Collections.Generic;
using NodeBase;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Backend.Simulation.World
{
    public class TimeSliceGrid
    {
        private readonly AbstractNodeInstance[,] _nodes;
        private readonly List<Connection>[,] _connections;
        
        private readonly Dictionary<Guid, List<Vector2Int>> _connectionCellsById = new();
        private readonly int width;
        private readonly int height;
        private readonly float cellSize;

        public TimeSliceGrid(int width, int height, float cellSize)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            _nodes = new AbstractNodeInstance[width, height];
            _connections = new List<Connection>[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _connections[x, y] = new List<Connection>();
                }
            }
        }
        
        #region Occupancy

        public bool IsCellOccupied(Vector2Int cell, out AbstractNodeInstance node, out List<Connection> connection)
        {
            node = null;
            connection = null;

            if (!IsInside(cell.x, cell.y))
                return true;

            node = _nodes[cell.x, cell.y];
            connection = _connections[cell.x, cell.y].Count == 0 ? null : _connections[cell.x, cell.y];

            return node != null || connection != null;
        }
        
        public bool IsCellOccupied(Vector2Int cell)
        {
            if (!IsInside(cell.x, cell.y))
                return true;

            var node = _nodes[cell.x, cell.y];
            var connection = _connections[cell.x, cell.y].Count == 0 ? null : _connections[cell.x, cell.y];

            return node != null || connection != null;
        }
        
        #endregion

        private bool IsInside(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public List<Vector2Int> getCellsOfConnection(Guid connectionId)
        {
            return _connectionCellsById.TryGetValue(connectionId, out var cells) ? cells : new List<Vector2Int>();
        }
        
        private bool IsInside(Vector2Int cell)
        {
            return IsInside(cell.x, cell.y);
        }

        private Vector2Int WorldToCell(Vector2 worldPos)
        {
            var cx = Mathf.FloorToInt(worldPos.x / cellSize);
            var cy = Mathf.FloorToInt(worldPos.y / cellSize);
            return new Vector2Int(cx, cy);
        }

        #region Nodes
        
        public bool Add(AbstractNodeInstance node)
        {
            var cell = WorldToCell(node.Pos);
            
            if (IsCellOccupied(cell, out _, out _))
                return false;
            
            if (!IsInside(cell.x, cell.y))
            {
                throw new ArgumentException("The cell "+cell+" is out of bounds for the grid with "+width+"x"+height);
            }

            // Wenn du Kollision willst, könntest du hier prüfen, ob cells[cell.x, cell.y] != null
            _nodes[cell.x, cell.y] = node;
            return true;
        }

        public bool Remove(AbstractNodeInstance node)
        {
            var cell = WorldToCell(node.Pos);
            if (!IsInside(cell.x, cell.y))
                return false;

            if (_nodes[cell.x, cell.y] == node)
            {
                _nodes[cell.x, cell.y] = null;
                return true;
            }

            return false;
        }
        
        #endregion
        
        #region Connections
        public bool TryAddConnectionCells(
            Connection connection,
            Vector2Int[] cells,
            AbstractNodeInstance endpointA,
            AbstractNodeInstance endpointB,
            int bridgesBuilt)
            
        {
            int numOfConnectionsCrossed = 0;
            foreach (var cell in cells)
            {
                if (!IsInside(new Vector2Int((int)cell.x, (int)cell.y))) {
                    Debug.Log("Is not inside!");
                    return false;
                }

                // Endpunkte dürfen von den beiden Nodes belegt sein
                if (cell == WorldToCell(endpointA.Pos) || cell == WorldToCell(endpointB.Pos))
                    continue;

                if (IsCellOccupied(cell, out _, out _)) {
                    numOfConnectionsCrossed++;
                    if (numOfConnectionsCrossed > bridgesBuilt)
                    {
                        Debug.Log("A cell of the connection is occupied!");
                        return false;
                    }
                }
            }

            var cellsToConnect = new List<Vector2Int>();
            foreach (var cell in cells)
            {
                if (cell == WorldToCell(endpointA.Pos) || cell == WorldToCell(endpointB.Pos))
                    continue;
                
                _connections[cell.x, cell.y].Add( connection);
                cellsToConnect.Add(cell);
            }

            _connectionCellsById[connection.guid] = cellsToConnect;
            return true;
        }

        public void RemoveConnectionCells(Guid connectionId)
        {
            if (!_connectionCellsById.TryGetValue(connectionId, out var cells))
            {
                Debug.Log("Tried to remove connection but was not found in this grid with id: "+connectionId);
                return;
            }

            foreach (var cell in cells)
            {
                if (!IsInside(cell))
                    continue;

                var list = _connections[cell.x, cell.y];
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].guid == connectionId)
                        list.RemoveAt(i);
                }
            }

            _connectionCellsById.Remove(connectionId);
        }

        #endregion

        // --------------------------------------------------
        // Neue Methode: zufällige leere Zelle mit Gewichten
        // --------------------------------------------------

        /// <summary>
        /// Sucht eine zufällige leere Zelle.
        /// Bereiche mit vielen zusammenhängenden leeren Zellen
        /// werden stärker gewichtet (Region-Größe^2).
        /// Gibt true zurück, wenn eine Zelle gefunden wurde.
        /// </summary>
        public bool TryGetRandomEmptyCell(Random random, out Vector2Int cell)
        {
            cell = default;

            // Alle leeren Zellen in Regionen gruppieren
            var visited = new bool[width, height];
            var emptyCells = new List<Vector2Int>();
            var weights = new List<int>();

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                if (IsCellOccupied(new Vector2Int(x, y)) || visited[x, y])
                    continue;

                // Neue leere Region via BFS/DFS finden
                var regionCells = new List<Vector2Int>();
                var queue = new Queue<Vector2Int>();
                var start = new Vector2Int(x, y);

                queue.Enqueue(start);
                visited[x, y] = true;

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    regionCells.Add(current);

                    var cx = current.x;
                    var cy = current.y;

                    // 4er-Nachbarschaft
                    TryEnqueue(cx + 1, cy);
                    TryEnqueue(cx - 1, cy);
                    TryEnqueue(cx, cy + 1);
                    TryEnqueue(cx, cy - 1);
                }

                var regionSize = regionCells.Count;

                // Gewicht = (RegionSize)^2 -> große Flächen stark bevorzugt
                var weight = regionSize * regionSize;

                foreach (var rc in regionCells)
                {
                    emptyCells.Add(rc);
                    weights.Add(weight);
                }

                void TryEnqueue(int nx, int ny)
                {
                    if (!IsInside(nx, ny))
                        return;
                    if (visited[nx, ny])
                        return;
                    
                    if (IsCellOccupied(new Vector2Int(nx, ny)))
                        return;

                    visited[nx, ny] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }

            if (emptyCells.Count == 0)
                return false;

            // Weighted Random Choice
            double totalWeight = 0;
            for (var i = 0; i < weights.Count; i++)
                totalWeight += weights[i];

            var r = random.NextDouble() * totalWeight;

            for (var i = 0; i < emptyCells.Count; i++)
            {
                r -= weights[i];
                if (r <= 0.0)
                {
                    cell = emptyCells[i];
                    return true;
                }
            }

            // Fallback (numerische Rundungsfehler)
            cell = emptyCells[emptyCells.Count - 1];
            return true;
        }
    }
}