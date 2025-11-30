using System.Collections.Generic;
using NodeBase;
using UnityEngine;

namespace Backend.Simulation.World
{
    public class SpatialHashGrid
    {
        private readonly Dictionary<Vector2Int, List<AbstractNodeInstance>> cells = new();
        private readonly float cellSize;

        public SpatialHashGrid(float cellSize)
        {
            this.cellSize = cellSize;
        }

        private Vector2Int WorldToCell(Vector2 worldPos)
        {
            var cx = Mathf.FloorToInt(worldPos.x / cellSize);
            var cy = Mathf.FloorToInt(worldPos.y / cellSize);
            return new Vector2Int(cx, cy);
        }

        public void Add(AbstractNodeInstance node)
        {
            var cell = WorldToCell(node.Pos);
            if (!cells.TryGetValue(cell, out var list))
            {
                list = new List<AbstractNodeInstance>();
                cells[cell] = list;
            }

            list.Add(node);
        }

        public bool Remove(AbstractNodeInstance node)
        {
            var existed = false;
            var cell = WorldToCell(node.Pos);
            if (cells.TryGetValue(cell, out var list))
            {
                list.Remove(node);
                existed = true;
                if (list.Count == 0)
                    cells.Remove(cell);
            }

            return existed;
        }

        public bool Remove(Vector2 pos, float tolerance)
        {
            // Finde den Node in Reichweite
            var target = GetBestMatchNode(pos, tolerance);
            if (target == null)
                return false;

            // Entferne Node aus der passenden Grid-Zelle
            var cell = WorldToCell(target.Pos);

            if (cells.TryGetValue(cell, out var list))
            {
                var removed = list.Remove(target);
                if (list.Count == 0)
                    cells.Remove(cell);

                return removed;
            }

            return false;
        }

        public void UpdateNodePosition(AbstractNodeInstance node, Vector2 oldPos, Vector2 newPos)
        {
            var oldCell = WorldToCell(oldPos);
            var newCell = WorldToCell(newPos);
            if (oldCell == newCell) return;

            RemoveFromCell(node, oldCell);
            Add(node); // nutzt newPos in node.Position
        }

        private void RemoveFromCell(AbstractNodeInstance node, Vector2Int cell)
        {
            if (cells.TryGetValue(cell, out var list))
            {
                list.Remove(node);
                if (list.Count == 0)
                    cells.Remove(cell);
            }
        }

        /// <summary>
        ///     Gibt true zurück, wenn in der Nähe der Position
        ///     (Radius = tolerance) irgendein Node existiert.
        /// </summary>
        public bool HasNodeNear(Vector2 pos, float tolerance)
        {
            return GetNodesInRadius(pos, tolerance).Count > 0;
        }

        /// <summary>
        ///     Liefert alle Nodes innerhalb eines Radius r um pos.
        /// </summary>
        public List<AbstractNodeInstance> GetNodesInRadius(Vector2 pos, float radius)
        {
            var result = new List<AbstractNodeInstance>();

            // Wie viele Zellen muss ich nach links/rechts/oben/unten prüfen?
            var cellRadius = Mathf.CeilToInt(radius / cellSize);
            var centerCell = WorldToCell(pos);

            for (var dx = -cellRadius; dx <= cellRadius; dx++)
            for (var dy = -cellRadius; dy <= cellRadius; dy++)
            {
                var cell = new Vector2Int(centerCell.x + dx, centerCell.y + dy);
                if (!cells.TryGetValue(cell, out var list))
                    continue;

                // innerhalb der Zelle die echten Distanzen prüfen
                foreach (var node in list)
                    if (Vector2.SqrMagnitude(node.Pos - pos) <= radius * radius)
                        result.Add(node);
            }

            return result;
        }

        /// <summary>
        ///     Gibt den "besten" Node unter dem Cursor zurück (z.B. den nächsten).
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
    }
}