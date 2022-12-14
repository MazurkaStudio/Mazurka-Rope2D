using UnityEngine;

namespace MazurkaGameKit.Rope2D
{
    public class Rope2D_Object : MonoBehaviour
    {


        [SerializeField] private Rope2D rope;
        [SerializeField] private bool simulateGravity;
        [SerializeField] private float gravityImpact = 5f;
        [SerializeField] private float maxDelta = 5f;

        [SerializeField, Range(0f, 1f)] private float atPercentOfRope = .5f;
        private int atRopeSeg;

        private bool IsActive;
        private Vector3 lookAt;
        private Vector3 lastPos;
        private Vector3 deltaPos;
        private float angle;
        [SerializeField] private GameObject sprite;

        private void Start()
        {
            FindRopeSegToFollow();
            sprite.SetActive(false);

        }

        public void SetPositionOnRope()
        {
            transform.position = rope.RopeSegmentsNow[atRopeSeg];
            deltaPos = transform.position - lastPos;
            lastPos = transform.position;
        }
        
        private void FindRopeSegToFollow()
        {
            atRopeSeg = Mathf.FloorToInt(rope.RopeSubDivision * atPercentOfRope);

            if (atRopeSeg >= rope.RopeSubDivision - 2)
            {
                atRopeSeg = rope.RopeSubDivision - 2;
            }
        }

        private void FixedUpdate()
        {
            if (rope != null)
            {
                if (rope.CanBeSimulate)
                {
                    if (!IsActive)
                    {
                        sprite.SetActive(true);
                        IsActive = true;
                    }

                    SetPositionOnRope();
                    
                    if (!simulateGravity)
                    {
                        lookAt = rope.RopeSegmentsNow[atRopeSeg + 1] - rope.RopeSegmentsNow[atRopeSeg];
                        angle = Mathf.Atan2(lookAt.y, lookAt.x) * Mathf.Rad2Deg;
                    }
                    else
                    {
                        return;
                        //Simulate grabity from delta position
                        lookAt= Vector3.Lerp(Vector3.right, deltaPos, deltaPos.magnitude / (maxDelta * Time.fixedDeltaTime));
                        //lookAt = Vector3.MoveTowards(lookAt, h, gravityImpact * Time.fixedDeltaTime);
                        angle = Mathf.Atan2(lookAt.y, lookAt.x) * Mathf.Rad2Deg;
                    }
                    
                    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                    return;
                }
            }

            if (IsActive)
            {
                sprite.SetActive(false);
                IsActive = false;
            }
        }
    }

}

