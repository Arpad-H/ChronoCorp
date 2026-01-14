using System;
using System.Collections.Generic;
using Backend.Simulation.World;
using NodeBase;
using UnityEngine;

namespace Backend.Simulation.SimEvent
{
    public interface ISimEvent
    {
        string Name { get; }
        void Execute(SimulationStorage storage, TimeSlice slice, long globalTick);
    }
    
    public sealed class LoggedSimEvent
    {
        public long GlobalTick { get; }
        public SubTickTime Time { get; }
        public long Sequence { get; }
        public ISimEvent Event { get; }

        public LoggedSimEvent(long globalTick, SubTickTime time, long sequence, ISimEvent ev)
        {
            GlobalTick = globalTick;
            Time = time ?? throw new ArgumentNullException(nameof(time));
            Sequence = sequence;
            Event = ev ?? throw new ArgumentNullException(nameof(ev));
        }
    }
    
    public sealed class SimEventLog
    {
        private readonly List<LoggedSimEvent> _events = new();
        private long _sequence;

        public void Track(long globalTick, ISimEvent ev, SubTickTime time)
        {
            if (ev == null) throw new ArgumentNullException(nameof(ev));
            if (time == null) throw new ArgumentNullException(nameof(time));

            _events.Add(new LoggedSimEvent(globalTick, time, _sequence++, ev));
        }

        public List<LoggedSimEvent> Events => _events;
    }
    
    public sealed class TimeSliceRunner
    {
        private readonly TimeSlice _slice;
        private readonly SimEventLog _log;

        public long BirthGlobalTick { get; }
        public long LocalTick { get; private set; }
        private int _readIndex;

        public TimeSliceRunner(TimeSlice slice, SimEventLog log, long birthGlobalTick)
        {
            _slice = slice ?? throw new ArgumentNullException(nameof(slice));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            BirthGlobalTick = birthGlobalTick;
            LocalTick = 0;
            _readIndex = 0;
        }

        public void Tick(long localTick, SimulationStorage storage)
        {
            var events = _log.Events;

            // Optional: Batch für exakt diesen Tick sammeln (für Sortierung nach Time/Sequence)
            List<LoggedSimEvent> batch = null;

            while (_readIndex < events.Count && events[_readIndex].GlobalTick <= localTick)
            {
                var e = events[_readIndex];

                // Wenn du wirklich exakt bei == ausführen willst:
                // if (e.GlobalTick == visibleGlobalTick) ...
                // Praktisch ist <=, dann kann Slice auch nachholen.

                if (e.GlobalTick == localTick)
                {
                    batch ??= new List<LoggedSimEvent>(4);
                    batch.Add(e);
                }

                _readIndex++;
            }

            if (batch == null || batch.Count == 0)
                return;

            // Stabil erzwingen innerhalb eines Ticks: Time, dann Sequence
            batch.Sort((a, b) =>
            {
                int cmp = a.Time.Value.CompareTo(b.Time.Value);
                if (cmp != 0) return cmp;
                return a.Sequence.CompareTo(b.Sequence);
            });

            for (int i = 0; i < batch.Count; i++)
                batch[i].Event.Execute(storage, _slice, localTick);
        }
    }
    
    /// <summary>
    /// Reihenfolge innerhalb eines Ticks.
    /// Kleinere Werte werden früher ausgeführt.
    /// </summary>
    public sealed class SubTickTime : IComparable<SubTickTime>
    {
        public int Value { get; }

        public SubTickTime(int value)
        {
            Value = value;
        }

        public int CompareTo(SubTickTime other)
        {
            if (other == null) return 1;
            return Value.CompareTo(other.Value);
        }

        public override string ToString() => Value.ToString();

        // Convenience
        public static SubTickTime Zero => new SubTickTime(0);
    }
    
    public sealed class SpawnTimeRippleEvent : ISimEvent
    {
        public string Name => "SpawnTimeRipple";

        private readonly Vector2 _pos;
        private readonly EnergyType _energyType;

        public SpawnTimeRippleEvent(Vector2 pos, EnergyType energyType)
        {
            _pos = pos;
            _energyType = energyType;
        }

        public void Execute(SimulationStorage storage, TimeSlice slice, long globalTick)
        {
            Debug.Log("Spawning time ripple in past time slice: "+slice.SliceNumber);
            slice.spawnRipple(_pos, _energyType, out var newTimeRipple);
            if(newTimeRipple != null)
            {
                storage.Frontend.PlaceNodeVisual(newTimeRipple.guid, newTimeRipple.NodeType.NodeDTO, slice.SliceNumber, new Vector2(_pos.x, _pos.y), _energyType);
            }
            else
            {
                if (slice.TimeSliceGrid.IsCellOccupied(_pos, out var node, out var connection))
                {
                    if (node != null)
                    {
                        slice.spawnBlackHole(_pos, out var newBlackHole);
                        storage.Frontend.PlaceNodeVisual(newBlackHole.guid, newBlackHole.NodeType.NodeDTO, slice.SliceNumber, new Vector2(_pos.x, _pos.y), _energyType);
                    }
                    else if (connection != null)
                    {
                        slice.spawnBlockadeInConnection(connection, _pos, out var newBlockade);
                        storage.Frontend.PlaceNodeVisual(newBlockade.guid, newBlockade.NodeType.NodeDTO, slice.SliceNumber, new Vector2(_pos.x, _pos.y), _energyType);
                    }
                }
            }
        }
    }
}