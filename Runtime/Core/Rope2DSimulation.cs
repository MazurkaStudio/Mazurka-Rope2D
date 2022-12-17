using System.Collections.Generic;
using UnityEngine;

namespace MazurkaGameKit.Rope2D
{
    public partial class Rope2D
    {
        private void SimulateRope()
        {
            RopeForceSumm = Vector3.zero;

            for (int i = 1; i < RopeSegmentsNow.Length; i++)
            {
                Vector3 velocity = RopeSegmentsNow[i] - RopeSegmentsOld[i];
                RopeSegmentsOld[i] = RopeSegmentsNow[i];
                RopeSegmentsNow[i] += velocity;
                RopeSegmentsNow[i] += _gravity * Time.fixedDeltaTime;
                RopeSegmentsNow[i] += -velocity * _damp * Time.fixedDeltaTime;
                RopeForceSumm += RopeSegmentsNow[i] - RopeSegmentsOld[i];
            }

            if (_isBridgeRope)
            {
                //CONSTRAINTS
                for (int i = 0; i < _iteration; i++)
                {
                    BridgeRopeConstraints();
                }
            }
            else
            {
                //CONSTRAINTS
                for (int i = 0; i < _iteration; i++)
                {
                    SimpleRopeConstraints();
                }
            }


            float distance = Vector3.Distance(EndAnchor.position, StartAnchor.position);
            Tension = distance / RopeDistance;

            if (Tension > 1f)
            {
                if (_isExtendable) SetNewLenght(distance);
                else if (_ropeCanBreak && Tension > _ropeBreakThreshold)  BreakRope();
            } 
            else if (_isStretchable) SetNewLenght(distance);
        }
        
        private void SimpleRopeConstraints()
        {
            //Constraint to First Point 
            RopeSegmentsNow[0] = StartAnchor.position;

            for (int i = 0; i < RopeSubDivision - 1; i++)
            {

                float dist = (RopeSegmentsNow[i] - RopeSegmentsNow[i + 1]).magnitude;
                float error = Mathf.Abs(dist - RopeSegLenght);

                Vector3 changeDir = Vector2.zero;

                if (dist > RopeSegLenght) changeDir = (RopeSegmentsNow[i] - RopeSegmentsNow[i + 1]).normalized;
                else if (dist < RopeSegLenght) changeDir = (RopeSegmentsNow[i + 1] - RopeSegmentsNow[i]).normalized;

                Vector3 changeAmount = changeDir * error;
                if (i != 0)
                {
                    RopeSegmentsNow[i] -= changeAmount * 0.5f;
                    RopeSegmentsNow[i + 1] += changeAmount * 0.5f;
                }
                else
                {
                    RopeSegmentsNow[i] += changeAmount;
                    RopeSegmentsNow[i + 1] = RopeSegmentsNow[i];
                }
            }

            //Constraint to First Point 
            RopeSegmentsNow[0] = StartAnchor.position;
        }

        private void BridgeRopeConstraints()
        {
            //Constraint to First Point 
            RopeSegmentsNow[0] = StartAnchor.position;

            //Constraint to End Point 
            RopeSegmentsNow[^1] = EndAnchor.position;

            for (int i = 0; i < RopeSubDivision - 1; i++)
            {

                float dist = (RopeSegmentsNow[i] - RopeSegmentsNow[i + 1]).magnitude;
                float error = Mathf.Abs(dist - RopeSegLenght);

                Vector3 changeDir = Vector3.zero;

                if (dist > RopeSegLenght)
                {
                    changeDir = (RopeSegmentsNow[i] - RopeSegmentsNow[i + 1]).normalized;
                }
                else if (dist < RopeSegLenght)
                {
                    changeDir = (RopeSegmentsNow[i + 1] - RopeSegmentsNow[i]).normalized;
                }

                Vector3 changeAmount = changeDir * error;
                if (i != 0)
                {
                    RopeSegmentsNow[i] -= changeAmount * 0.5f;
                    RopeSegmentsNow[i + 1] += changeAmount * 0.5f;
                }
                else
                {
                    RopeSegmentsNow[i] += changeAmount;
                    RopeSegmentsNow[i + 1] = RopeSegmentsNow[i];
                }

                //Constraint to First Point 
                RopeSegmentsNow[0] = StartAnchor.position;

                //Constraint to End Point 
                RopeSegmentsNow[^1] = EndAnchor.position;
            }
        }

        private void DrawRope()
        {
            lineRenderer.SetPositions(RopeSegmentsNow);
        }


        

        public void InitializeRopeObjects()
        {
            foreach (var ropeObject in _allRope2DObjects)
            {
                ropeObject.InitializeRopeObject(this);
            }
        }
        
        private void RegisterRopeObject(Rope2DFixedObject ropeFixedObject)
        {
            _allRope2DObjects.Add(ropeFixedObject);
        }

        private void UnregisterRopeObject(Rope2DFixedObject ropeFixedObject)
        {
            _allRope2DObjects.Remove(ropeFixedObject);
        }
        
        private void UpdateObjects()
        {
            foreach (var ropeObject in _allRope2DObjects)
            {
                ropeObject.UpdateRopeObject();
            }
        }
        
        #if UNITY_EDITOR

        [ContextMenu("Get All Child Rope Objects")]
        public void GetAllChildRopeObject()
        {
            Rope2DFixedObject[] ropeObject = GetComponentsInChildren<Rope2DFixedObject>();

            for (int i = 0; i < ropeObject.Length; i++)
            {
                if (!_allRope2DObjects.Contains(ropeObject[i]))
                {
                    RegisterRopeObject(ropeObject[i]);
                }
            }

            UpdateObjects();
        }
        
        #endif
    }
}