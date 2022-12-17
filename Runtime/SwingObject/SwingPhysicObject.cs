using UnityEngine;

namespace MazurkaGameKit.Rope2D
{
    public partial class SwingPhysicObject : MonoBehaviour
    {
        //SWING ON START
        [SerializeField] private bool swingOnStart;
        [SerializeField] private float swingOnStartForce = 20f;
        [SerializeField] private int swingOnStartSign = 1;
        
        
        //PROPERTIES
        [SerializeField] private Rigidbody2D swingObjectBody;
        [SerializeField] private Rigidbody2D rootBody;
        [SerializeField] private DistanceJoint2D dJoin;
        [SerializeField] private Rope2D physicRope;
        [SerializeField] private SpriteRenderer chainRope;

        
        public Vector3 GetRootAnchorPosition => rootBody.position;
        public Vector3 GetSwingObjectPosition => swingObjectBody.position;
        public Rigidbody2D GetSwingObjectBody => swingObjectBody;
        public Rigidbody2D GetRootObjectBody => rootBody;
        public DistanceJoint2D GetDistanceJoint => dJoin;
        public Rope2D GetRope => physicRope;
        public SpriteRenderer GetChainSprite => chainRope;
        
        
        private void Start()
        {
            if(swingOnStart) Swing(swingOnStartSign, swingOnStartForce);
        }

        private void Update()
        {
            if(!dJoin.maxDistanceOnly)
            {
                chainRope.transform.rotation = LookAt2D((dJoin.connectedBody.position  - swingObjectBody.position).normalized);
            }
        }
        
        private Quaternion LookAt2D(Vector2 dir)
        {
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            return Quaternion.AngleAxis(angle + -90f, Vector3.forward);
        }
        
        public void Swing(int sign, float force)
        {
            swingObjectBody.AddForce(Vector2.right * sign * force * swingObjectBody.mass, ForceMode2D.Impulse);
        }
    }
}

