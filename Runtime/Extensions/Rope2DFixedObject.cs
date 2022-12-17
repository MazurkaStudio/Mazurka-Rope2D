using System;
using UnityEngine;

namespace MazurkaGameKit.Rope2D
{
    public class Rope2DFixedObject : MonoBehaviour
    {
        [SerializeField] private bool isHanged;
        
        private Vector3 lookAt;
        private float angle;
        
        
     
        [SerializeField, Range(0f, 1f)] private float positionOnRope = .5f;
        private bool isActive;
        [SerializeField, HideInInspector] private Rope2D rope;
        private int targetSegment;

        public void InitializeRopeObject(Rope2D rope)
        {
            this.rope = rope;
            
            targetSegment = Mathf.FloorToInt(rope.RopeSubDivision * positionOnRope);

            if (targetSegment >= rope.RopeSubDivision - 2)
            {
                targetSegment = rope.RopeSubDivision - 2;
            }

            if (isHanged)
            {
                lookAt = Vector3.right;
                angle = Mathf.Atan2(lookAt.y, lookAt.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }


#if UNITY_EDITOR

            if (!Application.isPlaying)
                return;
#endif
            
            rope.isEnable += EnableObject;
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR

            if (!Application.isPlaying)
                return;
#endif
            
            if(rope != null)
                rope.isEnable -= EnableObject;
        }


        public void UpdateRopeObject()
        { 
            SetPositionOnRope();
                    
            if (!isHanged)
            {
                lookAt = rope.RopeSegmentsNow[targetSegment + 1] - rope.RopeSegmentsNow[targetSegment];
                angle = Mathf.Atan2(lookAt.y, lookAt.x) * Mathf.Rad2Deg;
            }
            else
            {
                lookAt = Vector3.right;
                angle = Mathf.Atan2(lookAt.y, lookAt.x) * Mathf.Rad2Deg;
            }
            
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        private void SetPositionOnRope()
        {
            transform.position = rope.RopeSegmentsNow[targetSegment];
        }

        public void EnableObject(bool value)
        {
            gameObject.SetActive(value);
        }
        
    }

}

