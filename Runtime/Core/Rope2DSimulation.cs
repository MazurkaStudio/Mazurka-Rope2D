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
                RopeSegmentsNow[i] += (Vector3)GravityForce * Time.fixedDeltaTime;
                RopeSegmentsNow[i] += -velocity * _ropePreset.damp * Time.fixedDeltaTime;
                RopeForceSumm += RopeSegmentsNow[i] - RopeSegmentsOld[i];
            }

            if (_isBridgeRope)
            {
                //CONSTRAINTS
                for (int i = 0; i < _ropePreset.iteration; i++)
                {
                    BridgeRopeConstraints();
                }
            }
            else
            {
                //CONSTRAINTS
                for (int i = 0; i < _ropePreset.iteration; i++)
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
    }
}