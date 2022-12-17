using UnityEngine;
using UnityEditor;

namespace MazurkaGameKit.Rope2D
{
    
    [CustomEditor(typeof(SwingPhysicObject))]
    [CanEditMultipleObjects]
    public class SwingPhysicObjectEditor : Editor
    {
        private const float SWING_OBJECT_SCALE_HANDLES_FACTOR = 1f;
        private const float ROPE_SCALE_HANDLES_FACTOR = 2f;
        
        [MenuItem("GameObject/Mazurka GameKit/Hanged Physic Object")]
        public static void CreateSwingPhysicObject()
        {
            GameObject g = new GameObject("New Hanged Physic Object");
            g.AddComponent<SwingPhysicObject>();
            Rope2DEditorHelper.FocusObject(g);
        }
        
        
        private SwingPhysicObject targetSwingObject;
        
        private SerializedProperty m_swingOnStart;
        private SerializedProperty m_swingOnStartForce;
        private SerializedProperty m_swingOnStartSign;
        private SerializedProperty m_usedSprite;
        private SerializedProperty m_ropePreset;
        private SerializedProperty m_chainRopeSprite;

        
        private void OnEnable()
        {
            targetSwingObject = (SwingPhysicObject)target;
            targetSwingObject.CheckRope();
            InitSerializedProperties();
        }
        
        private void InitSerializedProperties()
        {
            m_swingOnStart = serializedObject.FindProperty("swingOnStart");
            m_swingOnStartForce = serializedObject.FindProperty("swingOnStartForce");
            m_swingOnStartSign = serializedObject.FindProperty("swingOnStartSign");
            
            m_usedSprite = serializedObject.FindProperty("usedSprite");
            m_ropePreset = serializedObject.FindProperty("ropePreset");
            m_chainRopeSprite = serializedObject.FindProperty("chainRopeSprite");
        }
        
        
        
        public override void OnInspectorGUI()
        {
            DrawSwingProperties();
            
            EditorGUILayout.Space(10);

            DrawUtilsButton();
            
            EditorGUILayout.Space(10);
            
            DrawVisualProperties();

            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSwingProperties()
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.PropertyField(m_swingOnStart);
            EditorGUILayout.PropertyField(m_swingOnStartForce);
            EditorGUILayout.PropertyField(m_swingOnStartSign);
            
            EditorGUILayout.EndVertical();
        }

        private void DrawUtilsButton()
        {
            EditorGUILayout.BeginVertical("box");
            
            if (GUILayout.Button("Reset root position"))
            {
                targetSwingObject.ResetRootPosition();
            }
            
            //CHANGE SWING ROPE TYPE
            string typeName = targetSwingObject.UseMaxDistanceOnly ? "Switch to straight rope" : "Switch to physic rope";
            if (GUILayout.Button(typeName))
            {
                targetSwingObject.ChangeMaxDistanceOnly();
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawVisualProperties()
        {
            EditorGUILayout.BeginVertical("box");
            
            //CHANGE OBJECT SPRITE
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(m_usedSprite, new GUIContent("Hanged Object Sprite"));
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                targetSwingObject.SetSprite();
            }

            if (targetSwingObject.UseMaxDistanceOnly)
            {
                //ROPE PRESET
                EditorGUI.BeginChangeCheck();
            
                EditorGUILayout.PropertyField(m_ropePreset, new GUIContent("Rope Preset"));
            
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    targetSwingObject.SetRopePreset();
                }
            }
            else
            {
                //CHAIN SPRITE
                EditorGUI.BeginChangeCheck();
            
                EditorGUILayout.PropertyField(m_chainRopeSprite, new GUIContent("Chain Sprite"));
            
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    targetSwingObject.SetChainSprite();
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        
        
        private void OnSceneGUI()
        {
            //ROOT HANDLE
            Handles.color = Color.magenta;
            Handles.DrawSolidDisc(targetSwingObject.GetRootAnchorPosition, Vector3.forward, 0.2f); 
            
            EditorGUI.BeginChangeCheck();

            //DISTANCE HANDLE
            Vector3 swingObjectAnchorPosition = targetSwingObject.GetSwingObjectPosition;
            swingObjectAnchorPosition = Handles.Slider(swingObjectAnchorPosition, Vector3.down, .2f, Handles.DotHandleCap, 0f);
            Handles.DrawDottedLine(targetSwingObject.GetRootAnchorPosition, swingObjectAnchorPosition, 5f);


            //SCALE HANDLE
            Vector3 scalePos = targetSwingObject.GetSwingObjectPosition + Vector3.right * (targetSwingObject.SwingObjectSize * SWING_OBJECT_SCALE_HANDLES_FACTOR);
            scalePos = Handles.Slider(scalePos, Vector3.right, 0.1f, Handles.DotHandleCap, 0f);
            float scale = (scalePos.x - targetSwingObject.GetSwingObjectPosition.x);
            Handles.DrawWireDisc(targetSwingObject.GetSwingObjectPosition, Vector3.forward, scale, 0.05f); 
            
            
            float chainWidth =targetSwingObject.ChainWidth;
            
            //ROPE SCALE HANDLE
            if (!targetSwingObject.UseMaxDistanceOnly)
            {
                Vector3 centerPos = targetSwingObject.GetSwingObjectPosition + (targetSwingObject.GetRootAnchorPosition - targetSwingObject.GetSwingObjectPosition) / 2;
                Vector3 scalRopePos = centerPos + (Vector3.right * chainWidth * ROPE_SCALE_HANDLES_FACTOR);
                scalRopePos = Handles.Slider(scalRopePos, Vector3.right, 0.1f, Handles.DotHandleCap, 0f);
                chainWidth = (scalRopePos.x - targetSwingObject.GetSwingObjectPosition.x) / ROPE_SCALE_HANDLES_FACTOR;
                Handles.DrawLine(scalRopePos + Vector3.up, scalRopePos + Vector3.down, 0.05f); 
            }

            
            if (EditorGUI.EndChangeCheck())
            {
                targetSwingObject.SetSwingObjectPosition(swingObjectAnchorPosition);
                targetSwingObject.ScaleSprite(scale / SWING_OBJECT_SCALE_HANDLES_FACTOR);
                
                if (!targetSwingObject.UseMaxDistanceOnly)
                {
                    targetSwingObject.ChainWidth = chainWidth;
                }
                
                EditorUtility.SetDirty(targetSwingObject.gameObject);
            }
        }
    }
}
