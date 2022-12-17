using System;
using UnityEngine;
using UnityEditor;


namespace MazurkaGameKit.Rope2D
{
    [CustomEditor(typeof(SwingDecorativeObject))]
    [CanEditMultipleObjects]
    public class SwingDecorativeObjectEditor : Editor
    {
        private const float SWING_OBJECT_SCALE_HANDLES_FACTOR = 1f;
        private const float ROPE_SCALE_HANDLES_FACTOR = 2f;
        
        
        [MenuItem("GameObject/Mazurka GameKit/Rope2D/New Hanged Decorative Object")]
        public static void CreateSwingDecorativeObject()
        {
            GameObject g = new GameObject("New Hanged Decorative Object");
            g.AddComponent<SwingDecorativeObject>();
            Rope2DEditorHelper.FocusObject(g);
        }
        
        private SwingDecorativeObject targetSwingObject;
        
        private SerializedProperty m_isConstant;
        private SerializedProperty m_windAmplitude;
        private SerializedProperty m_windFrequency;
        
        private SerializedProperty m_usedSprite;
        private SerializedProperty m_chainSprite;

        private bool simulate = false;
        private void OnEnable()
        {
            EditorApplication.update += Simulation;
            targetSwingObject = (SwingDecorativeObject)target;
            targetSwingObject.CheckRope();
            InitSerializedProperties();
        }

        private void OnDisable()
        {
            EditorApplication.update -= Simulation;

            simulate = false;
            
            if (targetSwingObject == null)
                return;
            
            targetSwingObject.ResetSwingObject();
        }

        private void InitSerializedProperties()
        {
            m_isConstant = serializedObject.FindProperty("isConstant");
            m_windAmplitude = serializedObject.FindProperty("windAmplitude");
            m_windFrequency = serializedObject.FindProperty("windFrequency");
            
            m_usedSprite = serializedObject.FindProperty("usedSprite");
            m_chainSprite = serializedObject.FindProperty("chainSprite");
        }
        

        public override void OnInspectorGUI()
        {
            DrawSwingProperties();
            
            EditorGUILayout.Space(10f);
            
            DrawVisualProperties();
            
            EditorGUILayout.Space(30f);
            
            DrawSimulationPanel();
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSwingProperties()
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Fake wind properties");
            EditorGUILayout.PropertyField(m_isConstant);
            EditorGUILayout.PropertyField(m_windAmplitude);
            EditorGUILayout.PropertyField(m_windFrequency);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawVisualProperties()
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField("Hanged object visual properties");
            
            //CHANGE OBJECT SPRITE
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(m_usedSprite, new GUIContent("Hanged Object Sprite"));
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                targetSwingObject.SetSprite();
            }

            //CHAIN SPRITE
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(m_chainSprite, new GUIContent("Chain Sprite"));
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                targetSwingObject.SetChainSprite();
            }
            
            EditorGUILayout.EndVertical();
            
            //SHORTCUTS
            Event lastEvent = Event.current;

            if (lastEvent.keyCode == KeyCode.S && lastEvent.type == EventType.KeyDown)
            {
                simulate = !simulate;
            }
        }

        private void DrawSimulationPanel()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.LabelField("Be careful, editor simulation is not always representative of in game simulation");
            EditorGUILayout.LabelField("Shortcut : Keydown S");
            
            simulate = EditorGUILayout.Toggle("Simulate : ", simulate);
            
            if (EditorGUI.EndChangeCheck())
            {
                if(simulate == false)
                    targetSwingObject.ResetSwingObject();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void OnSceneGUI()
        {
            EditorGUI.BeginChangeCheck();

            //ROOT HANDLE
            Handles.color = Color.magenta;
            Handles.DrawSolidDisc(targetSwingObject.GetRootAnchorPosition, Vector3.forward, 0.2f); 
            
            //DISTANCE HANDLE
            Vector3 swingObjectAnchorPosition = targetSwingObject.GetSwingObjectPosition;
            swingObjectAnchorPosition = Handles.Slider(swingObjectAnchorPosition, Vector3.down, .2f, Handles.DotHandleCap, 0f);
            Handles.DrawDottedLine(targetSwingObject.GetRootAnchorPosition, swingObjectAnchorPosition, 5f);


            //SCALE HANDLE
            Vector3 scalePos = targetSwingObject.GetSwingObjectPosition + Vector3.right * (targetSwingObject.SwingObjectSize * SWING_OBJECT_SCALE_HANDLES_FACTOR);
            scalePos = Handles.Slider(scalePos, Vector3.right, 0.1f, Handles.DotHandleCap, 0f);
            float scale = (scalePos.x - targetSwingObject.GetSwingObjectPosition.x);
            Handles.DrawWireDisc(targetSwingObject.GetSwingObjectPosition, Vector3.forward, scale, 0.05f); 
            
            
            //ROPE SCALE HANDLE
            float chainWidth = targetSwingObject.ChainWidth;
            Vector3 centerPos = targetSwingObject.GetSwingObjectPosition + (targetSwingObject.GetRootAnchorPosition - targetSwingObject.GetSwingObjectPosition) / 2;
            Vector3 scalRopePos = centerPos + (Vector3.right * chainWidth * ROPE_SCALE_HANDLES_FACTOR);
            scalRopePos = Handles.Slider(scalRopePos, Vector3.right, 0.1f, Handles.DotHandleCap, 0f);
            chainWidth = (scalRopePos.x - targetSwingObject.GetSwingObjectPosition.x) / ROPE_SCALE_HANDLES_FACTOR;
            Handles.DrawLine(scalRopePos + Vector3.up, scalRopePos + Vector3.down, 0.05f); 
            
            if (EditorGUI.EndChangeCheck())
            {
                targetSwingObject.SetSwingObjectPosition(swingObjectAnchorPosition);
                targetSwingObject.ScaleSprite(scale / SWING_OBJECT_SCALE_HANDLES_FACTOR);
                targetSwingObject.ChainWidth = chainWidth;
                
                EditorUtility.SetDirty(targetSwingObject.gameObject);
            }
            
            
            //SHORTCUTS
            Event lastEvent = Event.current;

            if (lastEvent.keyCode == KeyCode.S && lastEvent.type == EventType.KeyDown)
            {
                simulate = !simulate;
            }
        }

        private void Simulation()
        {
            if(simulate)
                targetSwingObject.Simulate((float)EditorApplication.timeSinceStartup);
        }
        
    }

}
