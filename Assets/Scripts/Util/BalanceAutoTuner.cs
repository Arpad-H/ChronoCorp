using System;
using Backend.Simulation.World;
using UnityEngine;

namespace Util
{
    public static class BalanceAutoTuner
    {
        /// <summary>
        ///     Rechnet Kern-Parameter so, dass:
        ///     - Early: wenig Druck (wenige Ripples, wenig Stability-Drain)
        ///     - Late: hoher Druck (mehr Ripples + höherer Stability-Drain durch Ripple-Anzahl)
        ///     Annahmen:
        ///     - TICKS_PER_SECOND = 50 (bei euch in SimulationStorage)
        ///     - Node-Spawn kommt in Slice0, andere Slices replayen Events => Gesamt-Ripplezahl ~ skaliert mit Layer-Anzahl.
        ///     - Wir approximieren Layer-Effekt über einen einfachen Multiplikator (gut genug fürs Balancing).
        /// </summary>
        ///
        // --- cached from ApplyAutoTune ---
        private static float startSpawnInterval;
        private static float endSpawnInterval;

        private static float startBaseStabilityPerNode;
        private static float endBaseStabilityPerNode;

        private static float startEnergyPacketSpeed;
        private static float endEnergyPacketSpeed;

        private static float roundLengthSeconds;
        private static int   maxLayers;

        private static bool initialized;
        public static void ApplyAutoTune(GameBalance b, DifficultyTargets t)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
            if (t.roundLengthSeconds <= 10) throw new ArgumentException("roundLengthSeconds too small");
            if (t.easyPhaseSeconds <= 1) throw new ArgumentException("easyPhaseSeconds too small");
            if (t.easyPhaseSeconds >= t.roundLengthSeconds) t.easyPhaseSeconds = t.roundLengthSeconds * 0.25f;

            var tps = SimulationStorage.TICKS_PER_SECOND;

            // ----------------------------
            // 1) Layer timing (wann werden neue Layer erzeugt)
            // ----------------------------
            // Idee: der "Gear Shift" kommt nach der Easy-Phase.
            // -> erster zusätzlicher Layer entsteht grob nach easyPhaseSeconds.
            b.layerDuplicationTime = Mathf.Max(1, Mathf.RoundToInt(t.easyPhaseSeconds * tps));

            // maxLayerCount nicht "errechnet", aber wir nutzen ihn als Teil der Approximation.
            var maxLayers = Mathf.Max(1, b.maxLayerCount);
            var totalSeconds = t.roundLengthSeconds;

            // Erwartete Layer-Anzahl über die Runde (approx):
            // +1, weil Layer0 existiert von Anfang an.
            var layersByEnd = Mathf.Clamp(1 + Mathf.FloorToInt(totalSeconds / (b.layerDuplicationTime / (float)tps)), 1,
                maxLayers);

            // ----------------------------
            // 2) Node Spawn Intervall (Haupthebel für "mehr Ripples über Zeit")
            // ----------------------------
            // Wir machen eine lineare Spawn-Speedup-Kurve über Layer:
            // spawnInterval(layer) = startInterval + layer * layerModifier
            // und clampen am Ende bei minSpawnIntervalSeconds.
            //
            // Wir wollen grob:
            // totalRipplesAtEasyEnd ~ t.easyPhaseSeconds / startInterval * layerMultiplierEasy
            // totalRipplesAtRoundEnd ~ integral über Zeit (vereinfacht) -> wir peilen endInterval so an, dass es passt.

            // Layer-Multiplikator (very rough): durchschnittlich aktive Layer in Phase.
            var layersAvgEasy = 1f; // in easy-phase existiert typischerweise nur Layer0
            var layersAvgEnd = Mathf.Max(1f, layersByEnd * 0.65f); // über die ganze Runde mittelt sich das darunter ein

            // Start-Intervall so, dass man bis easyEnd ungefähr ripplesAtEasyEnd erreicht.
            // totalRipples ≈ (easySeconds / startInterval) * layersAvgEasy
            var startInterval = t.easyPhaseSeconds / Mathf.Max(1, t.ripplesAtEasyEnd) / Mathf.Max(0.75f, layersAvgEasy);
            startInterval = Mathf.Clamp(startInterval, 0.2f, t.maxSpawnIntervalSeconds);

            // End-Intervall so, dass man bis zum Ende ungefähr ripplesAtRoundEnd erreicht (mit mehr Layern).
            // totalRipples ≈ (roundSeconds / avgInterval) * layersAvgEnd
            // => avgInterval ≈ (roundSeconds * layersAvgEnd) / ripplesAtRoundEnd
            var avgIntervalWanted = t.roundLengthSeconds * layersAvgEnd / Mathf.Max(1, t.ripplesAtRoundEnd);
            // Für eine lineare Kurve ist avgInterval ≈ (startInterval + endInterval)/2
            var endInterval = 2f * avgIntervalWanted - startInterval;
            endInterval = Mathf.Clamp(endInterval, t.minSpawnIntervalSeconds, startInterval);

            b.nodeSpawnIntervalPerSecond = startInterval;

            // Modifier pro Layer, sodass wir von startInterval nach endInterval kommen.
            // Achtung: bei euch heißt es "adds this in seconds" -> für schwerer muss das NEGATIV sein.
            var layerSteps = Mathf.Max(1, layersByEnd - 1);
            b.layerModifierToNodeSpawnInterval = (endInterval - startInterval) / layerSteps;

            // ----------------------------
            // 3) Node HP + Drain + Energy Economy (wie viele Ripples schafft ein Output)
            // ----------------------------
            // Drain pro Sekunde:
            // drainPerSecond = nodeHealthDrainRate * (tps / nodeDrainHealthEveryNTicks)
            var drainPerSecond = b.nodeHealthDrainRate * (tps / Mathf.Max(1f, b.nodeDrainHealthEveryNTicks));

            // Ein Output spawnt packets: packetsPerSecond = 1 / energyPacketSpawnIntervalPerSecond
            // Ein Packet heilt: energyPacketRechargeAmount
            // healingPerSecondPerOutput = packetsPerSecond * rechargeAmount
            //
            // Ziel: healingPerSecondPerOutput ≈ drainPerSecond * ripplesSustainablePerOutput
            // => 1/interval * recharge ≈ drainPerSecond * sustain
            // => interval ≈ recharge / (drainPerSecond * sustain)
            var sustain = Mathf.Max(0.25f, t.ripplesSustainablePerOutput);
            var intervalEnergy = b.energyPacketRechargeAmount / Mathf.Max(0.001f, drainPerSecond * sustain);

            // Clamp: zu kleine Intervalle können das Spiel fluten, zu große macht’s unmöglich.
            b.energyPacketSpawnIntervalPerSecond = Mathf.Clamp(intervalEnergy, 0.15f, 3.0f);

            // Packet speed: sollte groß genug sein, dass Routing nicht „zu spät“ heilt.
            // Faustregel: je später das Game wird (mehr Layer / mehr Wege), desto höher.
            // Wir nehmen eine moderate Skalierung über Layer.
            b.energyPacketSpeed = Mathf.Clamp(6f + layersByEnd * 0.75f, 6f, 18f);

            // ----------------------------
            // 4) Stability: Baseline + pro Ripple
            // ----------------------------
            // Euer Drain pro stability-tick:
            // drain = stabilityDecreaseValue + ripples * baseStabilityDecreasePerNode
            //
            // Wir wählen stabilityDecreaseValue als kleinen konstanten Druck,
            // und baseStabilityDecreasePerNode so, dass bei den Ziel-Ripplezahlen die gewünschte Stability-Fraction passt.
            //
            // Wir lösen das über eine einfache Erwartungsrechnung:
            // totalDrainOverPhase ≈ numStabilitySteps * (stabilityDecreaseValue + avgRipples * basePerRipple)
            //
            // numStabilitySteps = phaseSeconds / (stabilityDecreaseTicks/tps)

            var stepSeconds = b.stabilityDecreaseTicks / (float)tps;
            if (stepSeconds <= 0.01f) stepSeconds = 0.5f; // safety
            var maxStab = Mathf.Max(1, b.stabilityMaxValue);

            // Baseline constant drain: kleines Grundrauschen (2–6% der Max-Stability pro Minute)
            // -> abhängig von MaxValue.
            var baselinePerStep = Mathf.Clamp(maxStab * 0.0008f * stepSeconds, 0.05f, 2.5f);
            b.stabilityDecreaseValue = baselinePerStep;

            // Hilfsfunktion, um basePerRipple zu bestimmen:
            float SolveBasePerRipple(float phaseSeconds, int ripplesTarget, float stabilityFractionTarget,
                float layerMultiplier)
            {
                var steps = phaseSeconds / stepSeconds;

                var startStab = maxStab;
                var targetStab = Mathf.Clamp01(stabilityFractionTarget) * maxStab;
                var wantedLoss = Mathf.Max(0f, startStab - targetStab);

                // avgRipples approx: Hälfte des Targets in der Phase (lineares Wachstum)
                var avgRipples = Mathf.Max(0f, ripplesTarget * 0.5f) * layerMultiplier;

                // wantedLoss ≈ steps * (baseline + avgRipples * base)
                var baseValue = (wantedLoss / Mathf.Max(1f, steps) - b.stabilityDecreaseValue) /
                                Mathf.Max(1f, avgRipples);
                return Mathf.Max(0.0001f, baseValue);
            }
            b.maxLayerCount = 3;

            var baseEasy = SolveBasePerRipple(
                t.easyPhaseSeconds,
                t.ripplesAtEasyEnd,
                t.stabilityFractionAtEasyEnd,
                1f
            );

            var baseLate = SolveBasePerRipple(
                t.roundLengthSeconds,
                t.ripplesAtRoundEnd,
                t.stabilityFractionAtRoundEnd,
                layersAvgEnd
            );

            // Smooth: wir nehmen eher den Late-Wert (damit Late wirklich drückt),
            // aber begrenzen, damit Early nicht sofort explodiert.
            b.baseStabilityDecreasePerNode = Mathf.Clamp(Mathf.Lerp(baseEasy, baseLate, 0.75f), baseEasy, baseLate);

            // Refund/Overcompensation verhindern:
            // Bei euch bestimmt nodeStableThresholdPercentage die maximale Stability-Gain pro Ripple.
            // Faustregel: hoch halten, sonst wird Late mit vielen Ripples "zu leicht".
            b.nodeStableThresholdPercentage =
                Mathf.Clamp(b.nodeStableThresholdPercentage <= 0 ? 0.85f : b.nodeStableThresholdPercentage, 0.7f,
                    0.95f);

            // ----------------------------
            // 5) Malus thresholds (optional automatisch)
            // ----------------------------
            // Sanfte Early: Malus1 erst wenn man deutlich verkackt
            // Mid: Malus2 erreichbar
            // Late: Malus3 fast unvermeidbar wenn man nicht sehr gut ist
            if (b.malusThresholds == null || b.malusThresholds.Length < 3)
                b.malusThresholds = new float[3];

            b.malusThresholds[0] = 0.55f; // Malus1 unter 55%
            b.malusThresholds[1] = 0.35f; // Malus2 unter 35%
            b.malusThresholds[2] = 0.18f; // Malus3 unter 18%
            
            // Cache values for runtime interpolation
            startSpawnInterval = b.nodeSpawnIntervalPerSecond;
            endSpawnInterval   = Mathf.Max(
                t.minSpawnIntervalSeconds,
                b.nodeSpawnIntervalPerSecond + b.layerModifierToNodeSpawnInterval * (b.maxLayerCount - 1)
            );

            startBaseStabilityPerNode = b.baseStabilityDecreasePerNode;
            endBaseStabilityPerNode   = b.baseStabilityDecreasePerNode * 1.35f; // Late pressure multiplier

            startEnergyPacketSpeed = b.energyPacketSpeed;
            endEnergyPacketSpeed   = b.energyPacketSpeed * 1.25f;

            roundLengthSeconds = t.roundLengthSeconds;
            maxLayers = Mathf.Max(1, b.maxLayerCount);

            initialized = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                "=== BALANCE AUTO TUNER RESULT ===\n" +

                // --- TIME / STRUCTURE ---
                "[TIME]\n" +
                $"- Round Length (s): {t.roundLengthSeconds}\n" +
                $"- Easy Phase (s): {t.easyPhaseSeconds}\n" +
                $"- Layer Duplication Time (ticks): {b.layerDuplicationTime}\n" +
                $"- Max Layers: {b.maxLayerCount}\n\n" +

                // --- NODE SPAWNING ---
                "[NODE SPAWNING]\n" +
                $"- Start Spawn Interval (s): {b.nodeSpawnIntervalPerSecond:F2}\n" +
                $"- Layer Modifier Spawn Interval (s/layer): {b.layerModifierToNodeSpawnInterval:F3}\n" +
                $"- Min Target Spawn Interval (s): {t.minSpawnIntervalSeconds}\n" +
                $"- Max Target Spawn Interval (s): {t.maxSpawnIntervalSeconds}\n\n" +

                // --- NODE HEALTH / DRAIN ---
                "[NODE HEALTH & DRAIN]\n" +
                $"- Node Health Drain Rate: {b.nodeHealthDrainRate}\n" +
                $"- Node Drain Every N Ticks: {b.nodeDrainHealthEveryNTicks}\n" +
                $"- Drain Per Second (approx): {b.nodeHealthDrainRate * (SimulationStorage.TICKS_PER_SECOND / (float)b.nodeDrainHealthEveryNTicks):F3}\n\n" +

                // --- ENERGY ECONOMY ---
                "[ENERGY ECONOMY]\n" +
                $"- Energy Packet Spawn Interval (s): {b.energyPacketSpawnIntervalPerSecond:F2}\n" +
                $"- Energy Packet Recharge Amount: {b.energyPacketRechargeAmount}\n" +
                $"- Energy Packet Speed: {b.energyPacketSpeed:F2}\n" +
                $"- Ripples Sustainable Per Output (target): {t.ripplesSustainablePerOutput}\n\n" +

                // --- STABILITY ---
                "[STABILITY]\n" +
                $"- Stability Max Value: {b.stabilityMaxValue}\n" +
                $"- Stability Decrease Every N Ticks: {b.stabilityDecreaseTicks}\n" +
                $"- Base Stability Decrease Value: {b.stabilityDecreaseValue:F3}\n" +
                $"- Base Stability Decrease Per Ripple: {b.baseStabilityDecreasePerNode:F5}\n" +
                $"- Node Stable Threshold Percentage: {b.nodeStableThresholdPercentage:F2}\n\n" +

                // --- TARGET CHECKPOINTS ---
                "[TARGET CHECKPOINTS]\n" +
                $"- Ripples at Easy End (target): {t.ripplesAtEasyEnd}\n" +
                $"- Stability at Easy End (target): {t.stabilityFractionAtEasyEnd:P0}\n" +
                $"- Ripples at Round End (target): {t.ripplesAtRoundEnd}\n" +
                $"- Stability at Round End (target): {t.stabilityFractionAtRoundEnd:P0}\n\n" +

                // --- MALUS ---
                "[MALUS THRESHOLDS]\n" +
                $"- Malus 1 Threshold: {b.malusThresholds[0]:P0}\n" +
                $"- Malus 2 Threshold: {b.malusThresholds[1]:P0}\n" +
                $"- Malus 3 Threshold: {b.malusThresholds[2]:P0}\n\n" +
                "=== END BALANCE AUTO TUNER ==="
            );
#endif
        }
        
        public static void Tick(long tickCount, SimulationStorage storage)
        {
            if (!initialized) return;

            var b = BalanceProvider.Balance;
            int tps = SimulationStorage.TICKS_PER_SECOND;

            float elapsedSeconds = tickCount / (float)tps;
            float timeProgress = Mathf.Clamp01(elapsedSeconds / roundLengthSeconds);

            // Optional: Ease-Out (am Anfang schnell Richtung "langsamer", später flacht es ab)
            // Wenn du willst, dass es erst später richtig hochgeht: nimm ease-in (p*p)
            float eased = 1f - (1f - timeProgress) * (1f - timeProgress); // ease-out

            // 10 -> 60 Sekunden
            float intervalByTime = Mathf.Lerp(startSpawnInterval, endSpawnInterval, eased);

            // Layer-based extra slowdown (weil mehr slices sowieso Druck machen)
            int currentLayers = storage.timeSlices.Count;      // includes base slice
            int extraLayers = Mathf.Clamp(currentLayers - 1, 0, maxLayers - 1);

            // pro extra layer +10% mehr Intervall (optional, kannst du auch 0 lassen)
            float layerSlowdownFactor = 1f + extraLayers * 0.10f;

            b.nodeSpawnIntervalPerSecond = intervalByTime * layerSlowdownFactor;
        }
    }
    
    
}