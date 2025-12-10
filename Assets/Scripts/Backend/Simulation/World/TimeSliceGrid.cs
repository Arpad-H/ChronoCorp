using System;
using System.Collections.Generic;
using NodeBase;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Backend.Simulation.World
{
    public class TimeSliceGrid
    {
        private readonly AbstractNodeInstance[,] cells;
        private readonly int width;
        private readonly int height;
        private readonly float cellSize;

        public TimeSliceGrid(int width, int height, float cellSize)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            cells = new AbstractNodeInstance[width, height];
        }

        private bool IsInside(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        private Vector2Int WorldToCell(Vector2 worldPos)
        {
            var cx = Mathf.FloorToInt(worldPos.x / cellSize);
            var cy = Mathf.FloorToInt(worldPos.y / cellSize);
            return new Vector2Int(cx, cy);
        }

        // Optional helper, falls du mal die Zellmitte brauchst
        private Vector2 CellToWorldCenter(Vector2Int cell)
        {
            return new Vector2((cell.x + 0.5f) * cellSize, (cell.y + 0.5f) * cellSize);
        }

        // --------------------------------------------------
        // API ähnlich zur alten SpatialHashGrid
        // --------------------------------------------------

        public void Add(AbstractNodeInstance node)
        {
            var cell = WorldToCell(node.Pos);
            if (!IsInside(cell.x, cell.y))
            {
                throw new ArgumentException("The cell "+cell+" is out of bounds for the grid with "+width+"x"+height);
            }

            // Wenn du Kollision willst, könntest du hier prüfen, ob cells[cell.x, cell.y] != null
            cells[cell.x, cell.y] = node;
        }

        public bool Remove(AbstractNodeInstance node)
        {
            var cell = WorldToCell(node.Pos);
            if (!IsInside(cell.x, cell.y))
                return false;

            if (cells[cell.x, cell.y] == node)
            {
                cells[cell.x, cell.y] = null;
                return true;
            }

            return false;
        }

        public bool Remove(Vector2 pos, float tolerance)
        {
            // Finde den Node in Reichweite
            var target = GetBestMatchNode(pos, tolerance);
            if (target == null)
                return false;

            var cell = WorldToCell(target.Pos);
            if (!IsInside(cell.x, cell.y))
                return false;

            if (cells[cell.x, cell.y] == target)
            {
                cells[cell.x, cell.y] = null;
                return true;
            }

            return false;
        }

        public void UpdateNodePosition(AbstractNodeInstance node, Vector2 oldPos, Vector2 newPos)
        {
            var oldCell = WorldToCell(oldPos);
            var newCell = WorldToCell(newPos);

            if (oldCell == newCell)
                return;

            if (IsInside(oldCell.x, oldCell.y) && cells[oldCell.x, oldCell.y] == node)
                cells[oldCell.x, oldCell.y] = null;

            if (IsInside(newCell.x, newCell.y))
                cells[newCell.x, newCell.y] = node;
        }

        /// <summary>
        /// Gibt true zurück, wenn in der Nähe der Position
        /// (Radius = tolerance) irgendein Node existiert.
        /// </summary>
        public bool HasNodeNear(Vector2 pos, float tolerance)
        {
            return GetNodesInRadius(pos, tolerance).Count > 0;
        }

        /// <summary>
        /// Liefert alle Nodes innerhalb eines Radius r um pos.
        /// </summary>
        public List<AbstractNodeInstance> GetNodesInRadius(Vector2 pos, float radius)
        {
            var result = new List<AbstractNodeInstance>();

            var cellRadius = Mathf.CeilToInt(radius / cellSize);
            var centerCell = WorldToCell(pos);
            var radiusSqr = radius * radius;

            for (var dx = -cellRadius; dx <= cellRadius; dx++)
            for (var dy = -cellRadius; dy <= cellRadius; dy++)
            {
                var cx = centerCell.x + dx;
                var cy = centerCell.y + dy;
                if (!IsInside(cx, cy))
                    continue;

                var node = cells[cx, cy];
                if (node == null)
                    continue;

                if ((node.Pos - pos).sqrMagnitude <= radiusSqr)
                    result.Add(node);
            }

            return result;
        }

        /// <summary>
        /// Gibt den "besten" Node unter dem Cursor zurück (z.B. den nächsten).
        /// </summary>
        public AbstractNodeInstance GetBestMatchNode(Vector2 pos, float tolerance)
        {
            var candidates = GetNodesInRadius(pos, tolerance);
            if (candidates.Count == 0)
                return null;

            AbstractNodeInstance best = null;
            var bestSqrDist = float.MaxValue;

            foreach (var node in candidates)
            {
                var sqrDist = (node.Pos - pos).sqrMagnitude;
                if (sqrDist < bestSqrDist)
                {
                    bestSqrDist = sqrDist;
                    best = node;
                }
            }

            return best;
        }

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
                if (cells[x, y] != null || visited[x, y])
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
                    if (cells[nx, ny] != null)
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