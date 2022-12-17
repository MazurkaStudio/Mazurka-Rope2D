using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MazurkaGameKit.Rope2D
{
    [RequireComponent(typeof(Rope2D))]
    public class FakeRope2DWind : MonoBehaviour
    {
        [SerializeField] private Rope2D targetRope;
        [SerializeField, Range(0.01f, 0.1f)] private float windForce = 0.05f;
        [SerializeField, Range(0.01f, 3f)] private float windFreq = 0.5f;

        private int targetSeg;

        private void Reset()
        {
            targetRope = GetComponent<Rope2D>();
        }

        private void Start()
        {
            targetSeg = Random.Range(1, targetRope.RopeSegmentsNow.Length);
        }

        private void FixedUpdate()
        {
            targetRope.RopeSegmentsNow[targetSeg] += Vector3.up * (Mathf.PerlinNoise(Time.time * windFreq, 0f) * windForce - windForce /2f);
        }
    }
}
