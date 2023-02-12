using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MazurkaGameKit.Rope2D
{
    /// <summary>
    /// Use Rope2D To Create Visual Physic base 2D Ropes. You can create preset with scriptable and use it on rope
    /// You can also create rope in runtime, slice it, break it, and acces to rope physic runtime properties.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public partial class Rope2D : MonoBehaviour
    {
        private const float TENSION_LIMITS = 10f;
        
        [SerializeField] private bool _simulate = true;
        [SerializeField] private bool _isBridgeRope;
        [SerializeField] private Transform _startAnchor;
        [SerializeField] private Transform _endAnchor;
        [SerializeField] private Rope2D_RopePreset _ropePreset;
        
        
        [Range(10, 60), SerializeField] private int _ropeSubDivision = 25;
        [Range(-TENSION_LIMITS, TENSION_LIMITS), SerializeField] private float _defaultRopeTension;
        [Range(1, 30), SerializeField] private int _iteration = 12;
        [SerializeField] private float _damp = 3;
        [SerializeField] private Vector3 _gravity = Vector3.down;
        
        [SerializeField] private bool _isExtendable;
        [SerializeField] private bool _isStretchable;
        [SerializeField] private bool _ropeCanBreak;
        [SerializeField, Range(1.01f, 10f)] private float _ropeBreakThreshold = 1.5f;
        
        [SerializeField] private List<Rope2DFixedObject> _allRope2DObjects = new();
        [SerializeField] private LineRenderer lineRenderer;
        
        private bool wasInit;
        private bool isVisible;
      
        
        public Rope2D_RopePreset Preset => _ropePreset;
        public float RopeEnergy => RopeForceSumm.magnitude;
        public Transform EndAnchor => _endAnchor;
        public Transform StartAnchor => _startAnchor;
        public int RopeSubDivision => _ropeSubDivision;
        public bool IsBridgeRope => _isBridgeRope;
        public bool IsExtendable => _isExtendable;
        public bool IsStretchable => _isStretchable;
        public float Distance => Vector3.Distance(_startAnchor.position, _endAnchor.position);

        public bool IsEnable { get; private set; } = true;
        public Vector3[] RopeSegmentsOld { get; private set; }
        public Vector3[] RopeSegmentsNow { get; private set; }
        public Vector3 RopeForceSumm { get; private set; }
        public float RopeDistance { get; private set; }
        public float RopeSegLenght { get; private set; }
        public float Tension { get; private set; }

        public void SetDamp(float value) => _damp = value; 
        public void SetGravity(Vector3 value) => _gravity = value; 


        private Transform lastParent;
        
        public UnityAction<bool> isEnable;
        public UnityAction onBreak;
        public UnityAction onRebuildBridge;
       
        
        public static Rope2D CreateRope(Transform from, Transform to, Rope2D_RopePreset ropePreset, Transform parent, 
            bool isBridgeRope = false,  float defaultTension = 0f, bool isExtendable = false, bool isStretchable = false, bool ropeCanBreak  = false, float ropeBreakThreshold = 1.5f, float damp = 1f, Vector3 gravity = default)
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
            newRope._gravity = gravity;
            newRope._damp = damp;
            newRope._defaultRopeTension = defaultTension;
            newRope._isStretchable = isStretchable;
            newRope._isExtendable = isExtendable;
            newRope._ropeCanBreak = ropeCanBreak;
            newRope._ropeBreakThreshold = ropeBreakThreshold;

            newRope.EnableRope();
            return newRope;
        }
        
        
        #region Mono

        private void OnEnable()
        {
            if (lineRenderer == null) return;
            
            EnableRope();
        }

        private void FixedUpdate()
        {
            if (!_simulate || !isVisible)  return;
            
            SimulateRope();
            DrawRope();
            UpdateObjects();
        }

        private void OnDisable()
        {
            DisableRope();
        }

        private void OnBecameInvisible()
        {
            isVisible = false;
        }

        private void OnBecameVisible()
        {
            isVisible = true;
        }
        
        #endregion
        
        
        #region Initialize
        
        private void InitializeRope()
        {
            //CHECK FOR ALL COMPONENTS
            if (EndAnchor == null || StartAnchor == null || _ropePreset == null)
            {
                DisableRope();
                throw new System.Exception(("Rope is not correctly initialized = " + gameObject.name));
            }

            if (_simulate)CreateRopePoints();
            else CopyRopePoint(lineRenderer);

            DrawRope();
            InitializeRopeObjects();
            
            wasInit = true;
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

        private void CopyRopePoint(LineRenderer lineRenderer)
        {
            RopeDistance = Vector2.Distance(StartAnchor.position, EndAnchor.position) + _defaultRopeTension;
            RopeSegLenght = RopeDistance / (RopeSubDivision * 1f);
            
            int count = lineRenderer.positionCount;
            RopeSegmentsNow = new Vector3[count];
            RopeSegmentsOld = new Vector3[count];
            
            for (int i = 0; i < count; i++)
            {
                RopeSegmentsNow[i] = lineRenderer.GetPosition(i);
                RopeSegmentsOld[i] = lineRenderer.GetPosition(i);
            }
        }

        #endregion
        
        
        #region Utils

        public void EnableRope()
        {
            if (!wasInit) InitializeRope();
            
            lineRenderer.enabled = true;
            IsEnable = true;
            isEnable?.Invoke(true);
          
        }

        public void FreezeRope(bool value) => _simulate = value;

        public void DisableRope()
        {
            IsEnable = false;
            lineRenderer.enabled = false;
            isEnable?.Invoke(false);
            wasInit = false;
        }

        public void BreakRope()
        {
            if(_isBridgeRope)
            {
                lastParent = EndAnchor.parent;
                EndAnchor.transform.SetParent(transform);
                _isBridgeRope = false;
                onBreak?.Invoke();
            }
        }

        public void RebuildBridge()
        {
            EndAnchor.transform.SetParent(lastParent);
            _isBridgeRope = true;
            onRebuildBridge?.Invoke();
        }

        public void SetNewLenght(float newDistance)
        {
            RopeDistance = newDistance + _defaultRopeTension;
            RopeSegLenght = RopeDistance / RopeSubDivision;
        }

        public void ChangeEndAnchor(Transform newAnchor) => _endAnchor = newAnchor;
        
        public void ChangeStartAnchor(Transform newAnchor) => _startAnchor = newAnchor;

        public void SetRopePreset(Rope2D_RopePreset ropePreset)
        {
            _ropePreset = ropePreset;

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
        }
        
        #endregion
        
    }
}
