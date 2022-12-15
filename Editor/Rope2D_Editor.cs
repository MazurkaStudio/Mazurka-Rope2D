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
        private SerializedProperty m_iteration;
        private SerializedProperty m_damp;
        private SerializedProperty m_gravity;
        private SerializedProperty m_defaultRopeTension;
        private SerializedProperty m_isExtendable;
        private SerializedProperty m_isStretchable;
        private SerializedProperty m_ropeCanBreak;
        private SerializedProperty m_ropeBreakThreshold;
        private SerializedProperty m_ropesObject;
        
        private void OnEnable()
        {
            EditorApplication.update += Simulate;
            
            targetRope = (Rope2D)target;
            
            m_drawOnStart = serializedObject.FindProperty("_simulate");
            m_isBridgeRope = serializedObject.FindProperty("_isBridgeRope");
            m_startAnchor = serializedObject.FindProperty("_startAnchor");
            m_endAnchor = serializedObject.FindProperty("_endAnchor");
            m_ropePreset = serializedObject.FindProperty("_ropePreset");
            m_ropeSubDivision = serializedObject.FindProperty("_ropeSubDivision");
            m_iteration = serializedObject.FindProperty("_iteration");
            m_damp = serializedObject.FindProperty("_damp");
            m_gravity = serializedObject.FindProperty("_gravity");
            m_defaultRopeTension = serializedObject.FindProperty("_defaultRopeTension");
            m_isExtendable = serializedObject.FindProperty("_isExtendable");
            m_isStretchable = serializedObject.FindProperty("_isStretchable");
            m_ropeCanBreak = serializedObject.FindProperty("_ropeCanBreak");
            m_ropeBreakThreshold = serializedObject.FindProperty("_ropeBreakThreshold");
            m_ropesObject = serializedObject.FindProperty("_allRope2DObjects");
            
            targetRope.Check();
            
            targetRope.transform.hasChanged = false;
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
            EditorGUILayout.PropertyField(m_iteration);
            EditorGUILayout.PropertyField(m_damp);
            EditorGUILayout.PropertyField(m_gravity);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_defaultRopeTension);
            if (EditorGUI.EndChangeCheck())
            {
                if(canSimulate || Application.isPlaying)
                    targetRope.SetNewLenght(Vector2.Distance(targetRope.StartAnchor.position, targetRope.EndAnchor.position) + m_defaultRopeTension.floatValue);
            }
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
            EditorGUILayout.PropertyField(m_ropesObject);
            if (GUILayout.Button("Get All Child Rope Objects"))
            {
                targetRope.GetAllChildRopeObject();
            }
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

        private const float TENSION_HANDLE_FACTOR = 0.05f;
        
        private void OnSceneGUI()
        {
            Vector3 startPos = targetRope.StartAnchor.position;
            Vector3 endPos = targetRope.EndAnchor.position;

            //ANCHOR HANDLE
            Handles.color = Color.green;
            
            EditorGUI.BeginChangeCheck();
            
            startPos = Handles.FreeMoveHandle(startPos, Quaternion.identity, 0.2f, Vector3.zero,
                Handles.DotHandleCap);
            
            Handles.color = Color.cyan;
            endPos = Handles.FreeMoveHandle(endPos, Quaternion.identity, 0.2f, Vector3.zero,
                Handles.DotHandleCap);
            
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

            //TENSION HANDLE
            float ropeLenght =(startPos -targetRope.EndAnchor.position).magnitude;
            Vector3 ropeDir = (endPos - startPos).normalized;
            Vector3 tensionDefaultPos = endPos + Vector3.up;
            Vector3 tensionPos = tensionDefaultPos + ropeDir * (targetRope.GetDefaultTension * (TENSION_HANDLE_FACTOR * ropeLenght));
            
            EditorGUI.BeginChangeCheck();
            
            Handles.color = Color.magenta;
            tensionPos = Handles.Slider(tensionPos, ropeDir, 0.2f,
                Handles.SphereHandleCap, 0f);

            if (EditorGUI.EndChangeCheck())
            {
                Vector3 handleDir = (tensionPos - tensionDefaultPos);
                float newTension = Mathf.Clamp(handleDir.magnitude * Mathf.RoundToInt(Vector3.Dot(ropeDir, handleDir.normalized)) / (TENSION_HANDLE_FACTOR * ropeLenght), -10f, 10f);
                targetRope.SetDefaultTension(newTension);
                
                if(canSimulate || Application.isPlaying)
                    targetRope.SetNewLenght(ropeLenght + newTension);
            }
            
     
            //DRAW LINE
            Handles.DrawLine(tensionPos, tensionDefaultPos);
            Handles.DrawLine(targetRope.EndAnchor.position, tensionDefaultPos);

            Handles.color = Color.cyan;
            Handles.DrawDottedLine(startPos, endPos, 5f);
            
            
            //CHECK TRANSFORM MOVE
            if (targetRope.transform.hasChanged)
            {
                targetRope.transform.hasChanged = false;
                
                if (!Application.isPlaying && !canSimulate)
                    targetRope.DrawRopeInEditor();
            }
            
            
            //SHORTCUTS
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
