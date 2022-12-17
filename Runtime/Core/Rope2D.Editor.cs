using UnityEditor;
using UnityEngine;

namespace MazurkaGameKit.Rope2D
{
    public partial class Rope2D
    {
#if UNITY_EDITOR
        
        [MenuItem("GameObject/Mazurka GameKit/Rope2D/New Rope2D")]
        public static void CreateRope()
        {
            GameObject g = Instantiate(GetDefaultRope);
            g.name = "New Rope2D";
            Rope2DEditorHelper.FocusObject(g);
        }
        
        public static Rope2D_RopePreset GetDefaultPreset => Resources.Load<Rope2D_RopePreset>("Rope2D/DefaultRope2D");
        private static GameObject GetDefaultRope => Resources.Load<GameObject>("Rope2D/DefaultRope2D");


        public void SetDefaultTension(float value) => _defaultRopeTension = value;
        public float GetDefaultTension => _defaultRopeTension;

        public void Check()
        {
            if (_ropePreset == null) _ropePreset = GetDefaultPreset;
            if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
            
            lineRenderer.textureMode = LineTextureMode.Tile;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.allowOcclusionWhenDynamic = false;
            
            ApplyPreset();
            
            EditorUtility.SetDirty(gameObject);
        }
        
        public void DrawRopeInEditor()
        {
            lineRenderer = GetComponent<LineRenderer>();
            ApplyPreset();
            
            CreateRopePoints();
            SimulateRope();
            DrawRope();
            InitializeRopeObjects();
            UpdateObjects();
        }

        public void SimulateInEditor()
        {
          
            SimulateRope();
            DrawRope();
            UpdateObjects();
        }
        
#endif
    }
}

