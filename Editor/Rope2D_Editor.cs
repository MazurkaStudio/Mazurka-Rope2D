using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace MazurkaGameKit.Rope2D
{
    [CustomEditor(typeof(Rope2D)), CanEditMultipleObjects]
    public class Rope2D_Editor : Editor
    {
        private Rope2D targetRope;
        
        private SerializedProperty m_drawOnStart;
        private SerializedProperty m_isBridgeRope;
        private SerializedProperty m_startAnchor;
        private SerializedProperty m_endAnchor;
        private SerializedProperty m_ropePreset;
        private SerializedProperty m_ropeSubDivision;
        private SerializedProperty m_defaultRopeTension;
        private SerializedProperty m_isExtendable;
        private SerializedProperty m_isStretchable;
        private SerializedProperty m_ropeCanBreak;
        private SerializedProperty m_ropeBreakThreshold;
        
        private void OnEnable()
        {
            EditorApplication.update += Simulate;
            targetRope = (Rope2D)target;
            
            m_drawOnStart = serializedObject.FindProperty("_drawRopeOnStart");
            m_isBridgeRope = serializedObject.FindProperty("_isBridgeRope");
            m_startAnchor = serializedObject.FindProperty("_startAnchor");
            m_endAnchor = serializedObject.FindProperty("_endAnchor");
            m_ropePreset = serializedObject.FindProperty("_ropePreset");
            m_ropeSubDivision = serializedObject.FindProperty("_ropeSubDivision");
            m_defaultRopeTension = serializedObject.FindProperty("_defaultRopeTension");
            m_isExtendable = serializedObject.FindProperty("_isExtendable");
            m_isStretchable = serializedObject.FindProperty("_isStretchable");
            m_ropeCanBreak = serializedObject.FindProperty("_ropeCanBreak");
            m_ropeBreakThreshold = serializedObject.FindProperty("_ropeBreakThreshold");

            targetRope.Check();
        }


        private void OnDisable()
        {     
            EditorApplication.update -= Simulate;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(m_drawOnStart);
            EditorGUILayout.PropertyField(m_isBridgeRope);

            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(m_startAnchor);
            EditorGUILayout.PropertyField(m_endAnchor);
            
            if (GUILayout.Button("Center Root"))
            {
                Vector3 center = (targetRope.StartAnchor.position + targetRope.EndAnchor.position) / 2f;
                Vector3 diff = center - targetRope.transform.position;
                targetRope.transform.position = center;
                targetRope.StartAnchor.position -= diff;
                targetRope.EndAnchor.position -= diff;
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10f);
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(m_ropePreset);
            EditorGUILayout.PropertyField(m_ropeSubDivision);
            EditorGUILayout.EndVertical();
            
            if (EditorGUI.EndChangeCheck())
            {
                canSimulate = false;
                serializedObject.ApplyModifiedProperties();
                targetRope.DrawRopeInEditor();
            }
            
            EditorGUILayout.Space(10f);
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(m_defaultRopeTension);
            EditorGUILayout.PropertyField(m_isExtendable);
            EditorGUILayout.PropertyField(m_isStretchable);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10f);
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(m_ropeCanBreak);
            EditorGUILayout.PropertyField(m_ropeBreakThreshold);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(30f);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Be careful, editor simulation is not always representative of in game simulation");
            EditorGUILayout.LabelField("Shortcut : Keydown S");
            if (GUILayout.Button("Simulate"))
            {
                targetRope.DrawRopeInEditor();
                canSimulate = true;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            
            serializedObject.ApplyModifiedProperties();

            
            Event lastEvent = Event.current;

            if (lastEvent.keyCode == KeyCode.S && lastEvent.type == EventType.KeyDown)
            {
                targetRope.DrawRopeInEditor();
                canSimulate = true;
            }
        }

        private bool canSimulate;

        private void OnSceneGUI()
        {
            Vector3 startPos = targetRope.StartAnchor.position;
            Vector3 endPos = targetRope.EndAnchor.position;

            Handles.color = Color.green;
            
            EditorGUI.BeginChangeCheck();
            
            startPos = Handles.FreeMoveHandle(startPos, Quaternion.identity, 0.5f, Vector3.zero,
                Handles.DotHandleCap);
            
            Handles.color = Color.cyan;
            endPos = Handles.FreeMoveHandle(endPos, Quaternion.identity, 0.5f, Vector3.zero,
                Handles.DotHandleCap);
            
            Handles.DrawDottedLine(startPos, endPos, 5f);

            if (EditorGUI.EndChangeCheck())
            {   
                canSimulate = false;
                targetRope.StartAnchor.position = startPos;
                targetRope.EndAnchor.position = endPos;

                if (!Application.isPlaying)
                {
                    targetRope.DrawRopeInEditor();
                    EditorUtility.SetDirty(targetRope.gameObject);
                }
            }
            
            if (targetRope.transform.hasChanged)
            {
                targetRope.transform.hasChanged = false;
                
                if (!Application.isPlaying && !canSimulate)
                    targetRope.DrawRopeInEditor();
            }
            
            Event lastEvent = Event.current;

            if (lastEvent.keyCode == KeyCode.S && lastEvent.type == EventType.KeyDown)
            {
                targetRope.DrawRopeInEditor();
                canSimulate = true;
            }
        }
        
        void Simulate()
        {
            if (!Application.isPlaying && canSimulate)
                targetRope.SimulateInEditor();
               
        }
    }
}
