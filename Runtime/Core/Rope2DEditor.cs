using UnityEditor;
using UnityEngine;

namespace MazurkaGameKit.Rope2D
{
    public partial class Rope2D
    {
        [MenuItem("GameObject/Mazurka GameKit/Rope2D")]
        public static void CreateRope()
        {
            GameObject g = Instantiate(GetDefaultRope);
            g.name = "New Rope2D";
        }
        
        private static Rope2D_RopePreset GetDefaultPreset => Resources.Load<Rope2D_RopePreset>("Rope2D/DefaultRope2D");
        private static GameObject GetDefaultRope => Resources.Load<GameObject>("Rope2D/DefaultRope2D");
        
        #if UNITY_EDITOR

        public void Check()
        {
            if (_ropePreset == null) _ropePreset = GetDefaultPreset;
            
            ApplyPreset();
            
            lineRenderer.textureMode = LineTextureMode.Tile;
            
            EditorUtility.SetDirty(gameObject);
        }
        

        
        //[Button("DrawRopeInEditor")]
        public void DrawRopeInEditor()
        {
            lineRenderer = GetComponent<LineRenderer>();
            ApplyPreset();
            CreateRopePoints();
            SimulateRope();
            DrawRope();
        }

        public void SimulateInEditor()
        {
            SimulateRope();
            DrawRope();
        }
        
        #endif
    }
}

