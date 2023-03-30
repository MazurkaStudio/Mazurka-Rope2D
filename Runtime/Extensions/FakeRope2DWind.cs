using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MazurkaGameKit.Rope2D
{
    [RequireComponent(typeof(Rope2D))]
    public class FakeRope2DWind : MonoBehaviour, I2DRopeExternalForce
    {
        [SerializeField] private float windFrequency = 0.5f;
        [SerializeField] private float windAmplitude = 1.0f;
        [SerializeField] private Vector3 windDirection = new Vector3(1, 0, 0);
        
        private Vector3[] windForces;
        private int ropeNodeCount;
        private int seed;

        private void Start()
        {
            seed = Random.Range(0, 1000);        
        }

        private Vector3 CalculateWindForce(float time, float phase)
        {
            float at = (time + phase) * windFrequency;
            float windStrength = Mathf.PerlinNoise(at + seed, at + seed) * windAmplitude;
            Vector3 windForce = windDirection.normalized * windStrength;
            return windForce;
        }

        private void UpdateWind()
        {
            for (int i = 0; i < ropeNodeCount; i++)
            {
                windForces[i] = CalculateWindForce(Time.time, (float)i / (float)ropeNodeCount);
            }
        }

        public void Initialize(Rope2D rope)
        {
            ropeNodeCount = rope.RopeSubDivision;
            
            windForces = new Vector3[rope.RopeSubDivision];
           
            // Initialize the node positions along the rope
            for (int i = 0; i < ropeNodeCount; i++)
            {
                float phase = (float)i / (float)ropeNodeCount;
                windForces[i] = CalculateWindForce(Time.time, phase);
            }
        }

        private void FixedUpdate()
        {
            UpdateWind();
        }

        public bool IsGlobal => false;

        public Vector3 GetExternalForce(int index)
        {
            return windForces[index];
        }

        public Vector3 GetGlobalForce() => Vector3.zero;
    }
}
