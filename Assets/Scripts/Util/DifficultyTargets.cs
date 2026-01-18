using System;
using UnityEngine;

namespace Util
{
    [Serializable]
    public struct DifficultyTargets
    {
        [Header("Time")]
        public float roundLengthSeconds;      // z.B. 720 (= 12min)
        public float easyPhaseSeconds;        // z.B. 120 (= 2min) -> bis hier "leicht"

        [Header("Ripple pressure targets")]
        public int ripplesAtEasyEnd;          // z.B. 6   (gesamt über alle Layer, grob)
        public int ripplesAtRoundEnd;         // z.B. 40  (gesamt über alle Layer, grob)

        [Header("Stability feel")]
        [Range(0.05f, 0.95f)]
        public float stabilityFractionAtEasyEnd;  // z.B. 0.90 (nach easy-phase noch 90%)
        [Range(0.01f, 0.80f)]
        public float stabilityFractionAtRoundEnd; // z.B. 0.15 (am Ende kurz vorm sterben)

        [Header("Player power (healing economy)")]
        public float ripplesSustainablePerOutput;  // z.B. 1.5 (1 Output kann ~1-2 Ripples halten)

        [Header("Curve shape")]
        public float minSpawnIntervalSeconds;      // z.B. 1.8 (untere Grenze)
        public float maxSpawnIntervalSeconds;      // z.B. 8.0 (oberer Startwert)
    }
}