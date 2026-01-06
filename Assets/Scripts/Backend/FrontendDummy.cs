// using System.Collections.Generic;
// using System.Text;
// using Backend.Simulation.World;
// using Interfaces;
// using NodeBase;
// using TMPro;
// using UnityEditor;
// using UnityEngine;
//
// namespace FrontendDummy
// {
//     public class DummyFrontend : MonoBehaviour, IFrontend
//     {
//         [Header("Optional quick test")] public bool runSelfTest = true;
//
//         [Range(1, 240)] public int ticksPerSecond = 50;
//
//         [Header("HUD Text Meshes")] public TextMeshProUGUI generalText;
//
//         public TextMeshProUGUI stabilityText;
//         public TextMeshProUGUI generatorsText;
//         public TextMeshProUGUI connectionsText;
//         public TextMeshProUGUI nodesText;
//         public TextMeshProUGUI energyPacketsText;
//
//         [Tooltip("Refresh HUD at most every X seconds (0 = every change).")] [Range(0f, 1f)]
//         public float hudRefreshInterval = 0.05f;
//
//         private readonly HashSet<StabilityMalusType> _activeMalus = new();
//
//         // Optional (falls ihr Connections später irgendwo meldet)
//         private readonly HashSet<GUID> _knownConnections = new();
//
//         // Extra HUD state
//         private readonly Dictionary<GUID, (int min, int max, int cur)> _nodeHealth = new();
//
//         // Minimal state tracking (purely for debugging / inspection)
//         private readonly Dictionary<GUID, (NodeDTO type, int layer, Vector2 cellPos, EnergyType? energy)>
//             _nodes = new();
//
//         private readonly Dictionary<GUID, EnergyType> _packets = new();
//         private readonly HashSet<int> _timeSlices = new();
//         private bool _hudDirty;
//
//         private float _hudTimer;
//         private (int min, int max, int cur)? _stability;
//
//         private float _tickAccumulator;
//         private long _tickCount;
//
//         public IBackend Backend { get; private set; }
//
//         private void Awake()
//         {
//             Backend = new BackendImpl(this);
//         }
//
//         private void Start()
//         {
//             MarkHudDirty();
//
//             if (!runSelfTest) return;
//
//             // (Simulation bleibt unverändert – ich fasse den Ablauf nicht an)
//             var a = Backend.PlaceNode(NodeDTO.GENERATOR, 0, new Vector2(1, 1), EnergyType.WHITE);
//             var b = Backend.PlaceNode(NodeDTO.RIPPLE, 0, new Vector2(4, 1), EnergyType.BLUE);
//             
//
//
//             if (a != null && b != null)
//             {
//                 _nodes[(GUID)a] = (NodeDTO.GENERATOR, 0, new Vector2(1, 1), EnergyType.WHITE);
//                 _nodes[(GUID)b] = (NodeDTO.RIPPLE, 0, new Vector2(4, 1), EnergyType.BLUE);
//                 var conn = Backend.LinkNodes(a.Value, b.Value);
//                 if (conn != null)
//                 {
//                     // Nur Display-State: wir merken uns die Connection-ID, falls vorhanden
//                     _knownConnections.Add(conn.Value);
//                     MarkHudDirty();
//                 }
//             }
//         }
//
//         private void FixedUpdate()
//         {
//             if (Backend == null) return;
//
//             var dt = Time.fixedDeltaTime;
//             _tickAccumulator += dt;
//
//             var tickInterval = 1f / Mathf.Max(1, ticksPerSecond);
//             while (_tickAccumulator >= tickInterval)
//             {
//                 _tickAccumulator -= tickInterval;
//                 _tickCount++;
//                 Backend.tick(_tickCount, this);
//             }
//
//             // HUD refresh throttling
//             if (hudRefreshInterval <= 0f)
//             {
//                 if (_hudDirty) RebuildHud();
//                 return;
//             }
//
//             _hudTimer += dt;
//             if (_hudDirty && _hudTimer >= hudRefreshInterval)
//             {
//                 _hudTimer = 0f;
//                 RebuildHud();
//             }
//         }
//
//         // ------------------------
//         // IFrontend implementation
//         // ------------------------
//
//         public void GameOver(string reason)
//         {
//             // Nur Display (kein Simulation Change)
//             // Optional: könnt ihr auch rot markieren etc.
//             MarkHudDirty();
//         }
//
//         public bool PlaceNodeVisual(GUID id, NodeDTO nodeDto, int layerNum, Vector2 cellPos, EnergyType energyType)
//         {
//             _nodes[id] = (nodeDto, layerNum, cellPos, energyType);
//             MarkHudDirty();
//             return true;
//         }
//
//         public void SpawnEnergyPacket(GUID guid, EnergyType energyType)
//         {
//             _packets[guid] = energyType;
//             MarkHudDirty();
//         }
//
//         public void DeleteEnergyPacket(GUID guid)
//         {
//             _packets.Remove(guid);
//             MarkHudDirty();
//         }
//
//         public void OnStabilityBarUpdate(int minValue, int maxValue, int currentValue)
//         {
//             _stability = (minValue, maxValue, currentValue);
//             MarkHudDirty();
//         }
//
//         public void OnActivateStabilityMalus(StabilityMalusType stabilityMalusType)
//         {
//             _activeMalus.Add(stabilityMalusType);
//             MarkHudDirty();
//         }
//
//         public void OnDeactivateStabilityMalus(StabilityMalusType stabilityMalusType)
//         {
//             _activeMalus.Remove(stabilityMalusType);
//             MarkHudDirty();
//         }
//
//         public bool AddTimeSlice(int sliceNum)
//         {
//             _timeSlices.Add(sliceNum);
//             MarkHudDirty();
//             return true;
//         }
//
//         public void onNodeHealthChange(GUID id, int minValue, int maxValue, int currentValue)
//         {
//             _nodeHealth[id] = (minValue, maxValue, currentValue);
//             MarkHudDirty();
//         }
//
//         // ------------------------
//         // HUD building
//         // ------------------------
//
//         private void MarkHudDirty()
//         {
//             _hudDirty = true;
//         }
//
//         private void RebuildStability()
//         {
//             if (stabilityText == null) return;
//
//             var sb = new StringBuilder();
//             sb.AppendLine("=== STABILITY ===");
//
//             if (_stability.HasValue)
//             {
//                 var s = _stability.Value;
//                 sb.AppendLine($"Value: {s.cur} (min {s.min} / max {s.max})");
//             }
//             else
//             {
//                 sb.AppendLine("No data yet");
//             }
//
//             sb.AppendLine("Malus:");
//             if (_activeMalus.Count == 0)
//                 sb.AppendLine(" - none");
//             else
//                 foreach (var m in _activeMalus)
//                     sb.AppendLine($" - {m}");
//
//             stabilityText.text = sb.ToString();
//         }
//
//         private void RebuildGenerators()
//         {
//             if (generatorsText == null) return;
//
//             var sb = new StringBuilder();
//             sb.AppendLine("=== GENERATORS ===");
//
//             foreach (var kv in _nodes)
//             {
//                 var (type, layer, pos, energy) = kv.Value;
//                 if (type != NodeDTO.GENERATOR) continue;
//
//                 sb.AppendLine($"- {kv.Key}");
//                 sb.AppendLine($"  Energy: {energy}");
//                 sb.AppendLine($"  Layer: {layer} Pos: {pos}");
//             }
//
//             generatorsText.text = sb.ToString();
//         }
//
//         private void RebuildConnections()
//         {
//             if (connectionsText == null) return;
//
//             var sb = new StringBuilder();
//             sb.AppendLine("=== CONNECTIONS ===");
//
//             if (_knownConnections.Count == 0)
//                 sb.AppendLine("(no tracked connections)");
//             else
//                 foreach (var c in _knownConnections)
//                     sb.AppendLine($"- {c}");
//
//             connectionsText.text = sb.ToString();
//         }
//
//         private void RebuildNodes()
//         {
//             if (nodesText == null) return;
//
//             var sb = new StringBuilder();
//             sb.AppendLine("=== NODES ===");
//
//             foreach (var kv in _nodes)
//             {
//                 var id = kv.Key;
//                 var (type, layer, pos, energy) = kv.Value;
//                 if (type == NodeDTO.GENERATOR) continue;
//
//                 sb.AppendLine($"- {id}");
//                 sb.AppendLine($"  Type: {type}");
//                 sb.AppendLine($"  Energy: {energy}");
//
//                 if (_nodeHealth.TryGetValue(id, out var h))
//                     sb.AppendLine($"  Health: {h.cur} ({h.min}-{h.max})");
//                 else
//                     sb.AppendLine("  Health: ?");
//
//                 sb.AppendLine($"  Layer: {layer} Pos: {pos}");
//             }
//
//             nodesText.text = sb.ToString();
//         }
//
//         private void RebuildEnergyPackets()
//         {
//             if (energyPacketsText == null) return;
//
//             var sb = new StringBuilder();
//             sb.AppendLine("=== ENERGY PACKETS ===");
//
//             if (_packets.Count == 0)
//                 sb.AppendLine("(none)");
//             else
//                 foreach (var kv in _packets)
//                     sb.AppendLine($"- {kv.Key} | {kv.Value}");
//
//             energyPacketsText.text = sb.ToString();
//         }
//
//
//         private void RebuildGeneral()
//         {
//             if (generalText == null) return;
//
//             generalText.text =
//                 "=== GENERAL ===\n" +
//                 $"Tick: {_tickCount}\n" +
//                 $"Nodes: {_nodes.Count}\n" +
//                 $"Packets: {_packets.Count}\n" +
//                 $"TimeSlices: {_timeSlices.Count}";
//         }
//
//         private void RebuildHud()
//         {
//             _hudDirty = false;
//
//             RebuildGeneral();
//             RebuildStability();
//             RebuildGenerators();
//             RebuildConnections();
//             RebuildNodes();
//             RebuildEnergyPackets();
//         }
//
//
//         // Optional helper for tests (ändert Simulation nicht; nur HUD-state)
//         public void RegisterConnection(GUID connectionId)
//         {
//             _knownConnections.Add(connectionId);
//             MarkHudDirty();
//         }
//     }
// }