using UnityEngine;

namespace MazurkaGameKit.Rope2D
{
    [ExecuteAlways]
    public partial class SwingDecorativeObject : MonoBehaviour
    {
        [SerializeField] private bool isConstant = true;
        [SerializeField, Range(.1f, 45f)] private float windAmplitude = 3f;
        [SerializeField, Range(0.01f, 5f)] private float windFrequency = .6f;
     
        
        [SerializeField] private Transform swingObjectBody;
        [SerializeField] private SpriteRenderer chainSpriteRenderer;

        private int windSeed;
        
        
        public Vector3 GetRootAnchorPosition => transform.position;
        public Vector3 GetSwingObjectPosition => swingObjectBody.position;
        public SpriteRenderer GetChainSprite => chainSpriteRenderer;

        public float Distance => Vector2.Distance(GetRootAnchorPosition, GetSwingObjectPosition);
        
        private void Start()
        {
            GenerateNewSeed();
            //swingObjectBody.transform.localPosition = new Vector3(0, swingObjectBody.transform.localPosition.y, swingObjectBody.transform.localPosition.z);
            ResetSwingObject();
        }
        
        private void Update()
        {
            #if UNITY_EDITOR

            if (!Application.isPlaying && !simulate)
                return;
            
            #endif
            
            Simulate(Time.time);
        }


        public void Simulate(float time)
        {
            if (isConstant)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, (windAmplitude * Mathf.Sin((time + windSeed) * Mathf.PI * windFrequency)));
            }
            else
            {
                float angle = windAmplitude * Mathf.PerlinNoise(time + windSeed, 0f) * windFrequency - windAmplitude / 2;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }    
            chainSpriteRenderer.transform.rotation = LookAt2D((GetRootAnchorPosition - GetSwingObjectPosition).normalized);
        }

        private Quaternion LookAt2D(Vector2 dir)
        {
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            return Quaternion.AngleAxis(angle + -90f, Vector3.forward);
        }

        public void ResetSwingObject()
        {
            swingObjectBody.localPosition = Vector3.down * Distance;
            transform.eulerAngles = Vector3.zero;
            swingObjectBody.eulerAngles = Vector3.zero;
            chainSpriteRenderer.transform.rotation = LookAt2D((GetRootAnchorPosition - GetSwingObjectPosition).normalized);
        }
        
        public void GenerateNewSeed()
        {
            windSeed = Random.Range(-5000, 5000);
        }
    }
     
}

