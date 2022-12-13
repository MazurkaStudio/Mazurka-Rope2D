#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;



namespace Mazurka2DGameKit.Rope2D
{
    /// <summary>
    /// Use Rope2D To Create Visual Physic base 2D Ropes. You can create preset with scriptable and use it on rope
    /// You can also create rope in runtime, slice it, break it, and acces to rope physic runtime properties.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class Rope2D : MonoBehaviour
    {
        //INTERFACES
        [SerializeField] private bool drawRopeOnStart = true;
        [SerializeField] private bool _isBridgeRope;
        [SerializeField] private Transform _startAnchor;
        [SerializeField] private Transform _endAnchor;

        public Rope2D_RopePreset Preset => ropePreset;

#if UNITY_EDITOR

        [UnityEditor.MenuItem("GameObject/Mazurka 2D GameKit/Rope2D")]
        public static void CreateRope()
        {
            GameObject g = Instantiate(Resources.Load("Rope2D") as GameObject);
            g.name = "New Rope2D";
            Editor.EditorExtension.FocusSceneObject(g);
        }

#endif

        #region Rope Preset Management

        [SerializeField] private Rope2D_RopePreset ropePreset;

        public void SetRopePreset(Rope2D_RopePreset ropePreset)
        {
            this.ropePreset = ropePreset;

            ApplyPreset();
            
            #if UNITY_EDITOR
            
            UnityEditor.EditorUtility.SetDirty(this);
            
            #endif
        }

        private void ApplyPreset()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
            
            //Render    
            lineRenderer.colorGradient = ropePreset.lineColor;
            lineRenderer.sharedMaterial = ropePreset.ropeMat;
            lineRenderer.sortingLayerName = ropePreset.sortingLayerName;
            //Size
            lineRenderer.widthCurve = ropePreset.lineWidth;
            lineRenderer.widthMultiplier = ropePreset.ropeWidth;

            //Simulation
            GravityForce = ropePreset.gravity;
            currDamp = ropePreset.damp;

            switch (ropePreset.simulationUpdate)
            {
                case SimulationUpdate.FixedUpdate:
                    UsedTimeStep = Time.fixedDeltaTime;
                    break;
            }
        }

        #endregion

        #region Anchor Management

        public Transform EndAnchor => _endAnchor;

        public Transform StartAnchor => _startAnchor;

        public void ChangeEndAnchor(Transform newAnchor)
        {
            _endAnchor = newAnchor;
        }

        public void ChangeStartAnchor(Transform newAnchor)
        {
            _startAnchor = newAnchor;
        }

        #endregion

        #region Vars

        //VIRTUAL ROPE SEGMENTS
        public Vector3[] RopeSegmentsOld { get; private set; }
        public Vector3[] RopeSegmentsNow { get; private set; }

        /// <summary>
        /// All segments velocity summ
        /// </summary>
        public Vector3 GlobalForceSumm { get; private set; }

        /// <summary>
        /// All segments force summ magnitude
        /// </summary>
        public float RopeGlobalVelocity => GlobalForceSumm.magnitude;

        //COMPONENTS
        private LineRenderer lineRenderer;

        //GETTERS
        public float RopeDistance { get; private set; }
        public float RopeSegLenght { get; private set; }
        public Vector2 GravityForce { get; private set; }

        public bool CanBeSimulate { get; private set; }
        public bool IsBridgeRope => _isBridgeRope;
        public float UsedTimeStep { get; private set; }
        public float CurrRopeTension { get; private set; }

        //LOGIC
        private bool wasInit;
        private float overrideTension;
        private float currDamp;

        #endregion

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            InitializeLineRenderer();
        }

        /// <summary>
        /// Set default info for lineRender
        /// </summary>
        private void InitializeLineRenderer()
        {
            lineRenderer.textureMode = LineTextureMode.Tile;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.allowOcclusionWhenDynamic = false;
        }

        // Start is called before the first frame update
        private void Start()
        {
            if (ropePreset != null)
            {
                ApplyPreset();
            }

            if (drawRopeOnStart && !wasInit)
            {
                InitializeRope(_isBridgeRope);
            }
            else if (!wasInit)
            {
                lineRenderer.enabled = false;
            }
        }

        /// <summary>
        /// Create points from preset info, and mark as can simulate
        /// </summary>
        /// <param name="overrideTension"></param>
        private void InitializeRope(bool isBridgeRope = false, float overrideTension = 0f)
        {
            //CHECK FOR ALL COMPONENTS
            if (EndAnchor == null || StartAnchor == null || ropePreset == null || EndAnchor == StartAnchor)
            {
                gameObject.SetActive(false);
                throw new System.Exception(("Rope is not correctly initialized = " + gameObject.name));
            }

            this.overrideTension = overrideTension;
            _isBridgeRope = isBridgeRope;

         //   tile = new Vector2(ropePreset.RopeSubDivision, ropePreset.ropeWidth);

            CreateRopePoints();
            DrawRope();

            CanBeSimulate = true;
            wasInit = true;

            lineRenderer.enabled = true;
         
            return;
            Invoke(nameof(DelayedRender), Time.deltaTime);
        }

        private void DelayedRender()
        {
            if (CanBeSimulate)
            {
                lineRenderer.enabled = true;
            }
        }


        private void CreateRopePoints()
        {

            RopeDistance = Vector2.Distance(StartAnchor.position, EndAnchor.position) + ropePreset.defaultRopeTension + overrideTension;
            RopeSegLenght = RopeDistance / (ropePreset.RopeSubDivision * 1f);

            Vector3 ropeStartPoint = StartAnchor.position;
            Vector3 createPointDir = (EndAnchor.position - ropeStartPoint).normalized;

            RopeSegmentsNow = new Vector3[ropePreset.RopeSubDivision];
            RopeSegmentsOld = new Vector3[ropePreset.RopeSubDivision];

            lineRenderer.positionCount = ropePreset.RopeSubDivision;

            for (int i = 0; i < ropePreset.RopeSubDivision; i++)
            {
                //build pos list
                RopeSegmentsNow[i] = ropeStartPoint;
                RopeSegmentsOld[i] = ropeStartPoint;

                ropeStartPoint += createPointDir * RopeSegLenght;
            }
        }

        #region Runtime Management

        public void EnableRope()
        {
            if (wasInit)
            {
                lineRenderer.enabled = true;
                CanBeSimulate = true;
            }
        }

        public void FreezeRope()
        {
            CanBeSimulate = false;
        }

        public void DisableRope()
        {
            lineRenderer.enabled = false;
            CanBeSimulate = false;
        }

        public void Break()
        {
            if(_isBridgeRope)
            {
                EndAnchor.transform.SetParent(this.transform);
                _isBridgeRope = false;
            }
            
        }

        public void Rebuild()
        {
            _isBridgeRope = true;
        }

        private void OnEnable()
        {
            if (drawRopeOnStart && !wasInit)
            {
                InitializeRope(_isBridgeRope);
            }
        }

        private void OnDisable()
        {
            CanBeSimulate = false;
        }

        #endregion

        #region Simulation

        private void FixedUpdate()
        {
            if (!wasInit || !CanBeSimulate || ropePreset == null)
            {
                return;
            }

            if (ropePreset.simulationUpdate == SimulationUpdate.FixedUpdate)
            {
                SimulateRope();
            }

            if (ropePreset.renderUpdate == SimulationUpdate.FixedUpdate)
            {
                DrawRope();
            }
        }

        private void Update()
        {
            if (!wasInit || !CanBeSimulate || ropePreset == null)
            {
                return;
            }


            if (ropePreset.simulationUpdate == SimulationUpdate.Update)
            {
                UsedTimeStep = Time.deltaTime;
                SimulateRope();
            }

            if (ropePreset.renderUpdate == SimulationUpdate.Update)
            {
                DrawRope();
            }
        }

        private void LateUpdate()
        {
            if (!wasInit || !CanBeSimulate || ropePreset == null)
            {
                return;
            }

            if (ropePreset.simulationUpdate == SimulationUpdate.LateUpdate)
            {
                UsedTimeStep = Time.deltaTime;
                SimulateRope();
            }

            if (ropePreset.renderUpdate == SimulationUpdate.LateUpdate)
            {
                DrawRope();
            }
        }

        private void SimulateRope()
        {
            GlobalForceSumm = Vector3.zero;

            for (int i = 1; i < RopeSegmentsNow.Length; i++)
            {
                Vector3 velocity = RopeSegmentsNow[i] - RopeSegmentsOld[i];
                RopeSegmentsOld[i] = RopeSegmentsNow[i];
                RopeSegmentsNow[i] += velocity;
                RopeSegmentsNow[i] += (Vector3)GravityForce * UsedTimeStep;
                RopeSegmentsNow[i] += -velocity * ropePreset.damp * UsedTimeStep;
                GlobalForceSumm += RopeSegmentsNow[i] - RopeSegmentsOld[i];
            }

            if (_isBridgeRope)
            {
                //CONSTRAINTS
                for (int i = 0; i < ropePreset.iteration; i++)
                {
                    BridgeRopeConstraintes();
                }
            }
            else
            {
                //CONSTRAINTS
                for (int i = 0; i < ropePreset.iteration; i++)
                {
                    SimpleRopeConstraintes();
                }
            }


            float distance = Vector3.Distance(EndAnchor.position, StartAnchor.position);
            CurrRopeTension = distance / RopeDistance;

            if (CurrRopeTension > 1f)
            {
                if (ropePreset.isExtendable)
                {

                    UpdateRopeLenght(distance);
                }
                else if (ropePreset.ropeCanBreak && CurrRopeTension > ropePreset.ropeBreakThreshold)
                {
                    Break();
                    return;
                }


            }
            else if (ropePreset.isStretchable)
            {
                UpdateRopeLenght(distance);
            }

        }

        public void UpdateRopeLenght(float newDistance)
        {
            RopeDistance = newDistance + ropePreset.defaultRopeTension + overrideTension;
            RopeSegLenght = RopeDistance / ropePreset.RopeSubDivision;
        }

        private void SimpleRopeConstraintes()
        {
            //Constraint to First Point 
            RopeSegmentsNow[0] = StartAnchor.position;

            for (int i = 0; i < ropePreset.RopeSubDivision - 1; i++)
            {

                float dist = (RopeSegmentsNow[i] - RopeSegmentsNow[i + 1]).magnitude;
                float error = Mathf.Abs(dist - RopeSegLenght);

                Vector3 changeDir = Vector2.zero;

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
            }

            //Constraint to First Point 
            RopeSegmentsNow[0] = StartAnchor.position;
        }

        private void BridgeRopeConstraintes()
        {
            //Constraint to First Point 
            RopeSegmentsNow[0] = StartAnchor.position;

            //Constraint to End Point 
            RopeSegmentsNow[RopeSegmentsNow.Length - 1] = EndAnchor.position;

            for (int i = 0; i < ropePreset.RopeSubDivision - 1; i++)
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
                RopeSegmentsNow[RopeSegmentsNow.Length - 1] = EndAnchor.position;
            }
        }

        #endregion



        public static Rope2D CreateRope(Transform from, Transform to, Rope2D_RopePreset ropePreset, Transform parent, bool bridgeRope = false,  float overrideTension = 0f)
        {
            Rope2D newRope = new GameObject("Rope_" + from.name + " to " + to.name).AddComponent<Rope2D>();
            newRope.lineRenderer = newRope.GetComponent<LineRenderer>();
            newRope.transform.SetParent(parent);
            newRope.transform.localPosition = Vector3.zero;
            newRope.transform.localEulerAngles = Vector3.zero;
            newRope.ChangeStartAnchor(from);
            newRope.ChangeEndAnchor(to);
            newRope.SetRopePreset(ropePreset);
            newRope.InitializeRope(bridgeRope, overrideTension);
            return newRope;
        }

        /// <summary>
        /// Just reset rope with new position
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void ResetRopeFromPool()
        {
            if (!wasInit)
            {
                ApplyPreset();
            }
            InitializeRope(IsBridgeRope, overrideTension);
        }

        #region Render

        private Vector2 tile;

        private void DrawRope()
        {
            lineRenderer.SetPositions(RopeSegmentsNow);

            //if(Application.isPlaying)
                //lineRenderer.material.SetTextureScale("_MainTex", tile);
        }

#endregion

#if UNITY_EDITOR

        [Button("DrawRopeInEditor")]
        public void DrawRopeInEditor()
        {
            lineRenderer = GetComponent<LineRenderer>();
            ApplyPreset();
            CreateRopePoints();

            if (_isBridgeRope)
                BridgeRopeConstraintes();
            else
                SimpleRopeConstraintes();

            DrawRope();
        }
#endif
    }

    public enum SimulationUpdate
    {
        FixedUpdate, Update, LateUpdate
    }

    public struct RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos)
        {
            posNow = pos;
            posOld = pos;
        }
    }
}
