using UnityEngine;

namespace MazurkaGameKit.Rope2D
{
    /// <summary>
    /// Use Rope2D To Create Visual Physic base 2D Ropes. You can create preset with scriptable and use it on rope
    /// You can also create rope in runtime, slice it, break it, and acces to rope physic runtime properties.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public partial class Rope2D : MonoBehaviour
    {
        [SerializeField] private bool _drawRopeOnStart = true;
        [SerializeField] private bool _isBridgeRope;
        [SerializeField] private Transform _startAnchor;
        [SerializeField] private Transform _endAnchor;
        [SerializeField] private Rope2D_RopePreset _ropePreset;
        
        
        [Range(10, 60), SerializeField] private int _ropeSubDivision = 25;
        [Range(-15f, 15f), SerializeField] private float _defaultRopeTension;
        [SerializeField] private bool _isExtendable;
        [SerializeField] private bool _isStretchable;
        [SerializeField] private bool _ropeCanBreak;
        [SerializeField, Range(1.01f, 10f)] private float _ropeBreakThreshold = 1.5f;
        
        private bool wasInit;
        private LineRenderer lineRenderer;
        
        public Rope2D_RopePreset Preset => _ropePreset;
        public float RopeEnergy => RopeForceSumm.magnitude;
        public Transform EndAnchor => _endAnchor;
        public Transform StartAnchor => _startAnchor;
        public int RopeSubDivision => _ropeSubDivision;
        public bool IsBridgeRope => _isBridgeRope;
        public bool IsExtendable => _isExtendable;
        public bool IsStretchable => _isStretchable;
        
        public Vector3[] RopeSegmentsOld { get; private set; }
        public Vector3[] RopeSegmentsNow { get; private set; }
        public Vector3 RopeForceSumm { get; private set; }
        public float RopeDistance { get; private set; }
        public float RopeSegLenght { get; private set; }
        public Vector2 GravityForce { get; private set; }
        public bool CanBeSimulate { get; private set; }

        public float Tension { get; private set; }
       
        

        
        
        public static Rope2D CreateRope(Transform from, Transform to, Rope2D_RopePreset ropePreset, Transform parent, 
            bool isBridgeRope = false,  float defaultTension = 0f, bool isExtendable = false, bool isStretchable = false, bool ropeCanBreak  = false, float ropeBreakThreshold = 1.5f)
        {
            Rope2D newRope = new GameObject("Rope_" + from.name + " to " + to.name).AddComponent<Rope2D>();
            newRope.lineRenderer = newRope.GetComponent<LineRenderer>();
            newRope.lineRenderer.textureMode = LineTextureMode.Tile;
            newRope.transform.SetParent(parent);
            newRope.transform.localPosition = Vector3.zero;
            newRope.transform.localEulerAngles = Vector3.zero;
            newRope.ChangeStartAnchor(from);
            newRope.ChangeEndAnchor(to);
            newRope.SetRopePreset(ropePreset);
            
            newRope._isBridgeRope = isBridgeRope;
            newRope._defaultRopeTension = defaultTension;
            newRope._isStretchable = isStretchable;
            newRope._isExtendable = isExtendable;
            newRope._ropeCanBreak = ropeCanBreak;
            newRope._ropeBreakThreshold = ropeBreakThreshold;
            return newRope;
        }
        
        
        #region Mono
        
        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            InitializeLineRenderer();
        }

        private void OnEnable()
        {
            if (_drawRopeOnStart && !wasInit)
            {
                InitializeRope();
            }
        }
        
        private void Start()
        {
            if (_ropePreset != null)
            {
                ApplyPreset();
            }

            if (_drawRopeOnStart && !wasInit)
            {
                InitializeRope();
            }
            else if (!wasInit)
            {
                lineRenderer.enabled = false;
            }
        }
        
        private void FixedUpdate()
        {
            if (!wasInit || !CanBeSimulate || _ropePreset == null)
            {
                return;
            }
            SimulateRope();
            DrawRope();
        }
        
        private void OnDisable()
        {
            CanBeSimulate = false;
        }
        
        #endregion
        
        
        #region Initialize

        private void InitializeLineRenderer()
        {
            lineRenderer.textureMode = LineTextureMode.Tile;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.allowOcclusionWhenDynamic = false;
        }
        
        /// <summary>
        /// Create points from preset info, and mark as can simulate
        /// </summary>
        /// <param name="tension"></param>
        private void InitializeRope()
        {
            //CHECK FOR ALL COMPONENTS
            if (EndAnchor == null || StartAnchor == null || _ropePreset == null || EndAnchor == StartAnchor)
            {
                gameObject.SetActive(false);
                throw new System.Exception(("Rope is not correctly initialized = " + gameObject.name));
            }

            CreateRopePoints();
            DrawRope();

            CanBeSimulate = true;
            wasInit = true;

            lineRenderer.enabled = true;
        }
        
        private void CreateRopePoints()
        {

            RopeDistance = Vector2.Distance(StartAnchor.position, EndAnchor.position) + _defaultRopeTension;
            RopeSegLenght = RopeDistance / (RopeSubDivision * 1f);

            Vector3 ropeStartPoint = StartAnchor.position;
            Vector3 createPointDir = (EndAnchor.position - ropeStartPoint).normalized;

            RopeSegmentsNow = new Vector3[RopeSubDivision];
            RopeSegmentsOld = new Vector3[RopeSubDivision];

            lineRenderer.positionCount = RopeSubDivision;

            for (int i = 0; i < RopeSubDivision; i++)
            {
                //build pos list
                RopeSegmentsNow[i] = ropeStartPoint;
                RopeSegmentsOld[i] = ropeStartPoint;

                ropeStartPoint += createPointDir * RopeSegLenght;
            }
        }

        #endregion
        
        
        #region Utils

        public void EnableRope()
        {
            if (wasInit)
            {
                lineRenderer.enabled = true;
                CanBeSimulate = true;
            }
        }

        public void FreezeRope() => CanBeSimulate = false;

        public void DisableRope()
        {
            lineRenderer.enabled = false;
            CanBeSimulate = false;
        }

        public void BreakRope()
        {
            if(_isBridgeRope)
            {
                EndAnchor.transform.SetParent(transform);
                _isBridgeRope = false;
            }
        }

        public void RebuildBridge() => _isBridgeRope = true;

        public void SetNewLenght(float newDistance)
        {
            RopeDistance = newDistance + _defaultRopeTension;
            RopeSegLenght = RopeDistance / RopeSubDivision;
        }
        
        public void ResetRopeFromPool()
        {
            if (!wasInit)
            {
                ApplyPreset();
            }
            
            InitializeRope();
        }
        
        public void ChangeEndAnchor(Transform newAnchor) => _endAnchor = newAnchor;
        
        public void ChangeStartAnchor(Transform newAnchor) => _startAnchor = newAnchor;

        public void SetRopePreset(Rope2D_RopePreset ropePreset)
        {
            this._ropePreset = ropePreset;

            ApplyPreset();
            
#if UNITY_EDITOR
            
            UnityEditor.EditorUtility.SetDirty(this);
            
#endif
        }

        public void ApplyPreset()
        {
            if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

            //Render    
            lineRenderer.colorGradient = _ropePreset.lineColor;
            lineRenderer.sharedMaterial = _ropePreset.ropeMat;

            //Size
            lineRenderer.widthCurve = _ropePreset.lineWidth;
            lineRenderer.widthMultiplier = _ropePreset.ropeWidth;

            //Simulation
            GravityForce = _ropePreset.gravity;
        }
        
        #endregion
        
    }
}
