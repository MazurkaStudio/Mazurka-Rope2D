using UnityEngine;
using UnityEditor;

namespace MazurkaGameKit.Rope2D
{
    
    [CustomEditor(typeof(SwingPhysicObject))]
    [CanEditMultipleObjects]
    public class SwingPhysicObjectEditor : Editor
    {
        private SwingPhysicObject targetSwingObject;
        
        
        private bool useMaxDistanceOnly = false;
        private Sprite swingObjectSprite;
        private Sprite ropeSprite;
        private float ropeSpriteWidth;
        private Rope2D_RopePreset ropePreset;
        
        private void OnEnable()
        {
            targetSwingObject = (SwingPhysicObject)target;

            targetSwingObject.CreateRope();
            
            useMaxDistanceOnly = targetSwingObject.dJoin.maxDistanceOnly;
            swingObjectSprite = targetSwingObject.GetSwingObjectSpriteRenderer.sprite;
            ropeSpriteWidth = targetSwingObject.TryGetRopeSpriteWidth();
            ropeSprite = targetSwingObject.TryGetRopeSprite();
            ropePreset = targetSwingObject.TryGetRopePreset();

            if (ropePreset == null)
            {
                ropePreset = Resources.Load<Rope2D_RopePreset>("Default Rope2D");
            }
            
            EditorUtility.SetDirty(targetSwingObject.gameObject);
        }
        
        public override void OnInspectorGUI()
        {           
            EditorGUILayout.BeginVertical("Box");
            
            base.OnInspectorGUI();

            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Set Swing Object Root to Current Joint Root"))
            {
                targetSwingObject.transform.position =
                    targetSwingObject.dJoin.connectedBody.position + targetSwingObject.dJoin.connectedAnchor;
            }
            
            EditorGUILayout.Space(50f);
            
            EditorGUILayout.BeginVertical("HelpBox");

            useMaxDistanceOnly = EditorGUILayout.Toggle("Use Max Distance Only", useMaxDistanceOnly);

            if (useMaxDistanceOnly != targetSwingObject.dJoin.maxDistanceOnly)
            {
                if (useMaxDistanceOnly)
                {
                    targetSwingObject.dJoin.maxDistanceOnly = true;
                    targetSwingObject.chainRope.gameObject.SetActive(false);
                    targetSwingObject.CreatePhysicRope(ropePreset);
                    targetSwingObject.physicRope.gameObject.SetActive(true);
                    EditorUtility.SetDirty(targetSwingObject.gameObject);
                }
                else
                {
                    targetSwingObject.dJoin.maxDistanceOnly = false;
                    targetSwingObject.physicRope.gameObject.SetActive(false);
                    targetSwingObject.CreateSpriteRope(ropeSprite, ropeSpriteWidth);
                    targetSwingObject.chainRope.gameObject.SetActive(true);
                    EditorUtility.SetDirty(targetSwingObject.gameObject);
                }
            }
            
            swingObjectSprite = (Sprite)EditorGUILayout.ObjectField("Swing Object Sprite", swingObjectSprite, typeof(Sprite), false);

            if (swingObjectSprite != targetSwingObject.GetSwingObjectSpriteRenderer.sprite)
            {
                targetSwingObject.SetSprite(swingObjectSprite);
                EditorUtility.SetDirty(targetSwingObject.gameObject);
            }

            
            if (useMaxDistanceOnly)
            {
                ropePreset = (Rope2D_RopePreset)EditorGUILayout.ObjectField("Rope PReset", ropePreset, typeof(Rope2D_RopePreset), false);

                if (ropePreset != targetSwingObject.TryGetRopePreset())
                {
                    targetSwingObject.CreatePhysicRope(ropePreset);
                    EditorUtility.SetDirty(targetSwingObject.gameObject);
                }
            }
            else
            {
 
            
                ropeSprite = (Sprite)EditorGUILayout.ObjectField("Rope Sprite", ropeSprite, typeof(Sprite), false);

                if (ropeSprite != targetSwingObject.TryGetRopeSprite())
                {
                    targetSwingObject.CreateSpriteRope(ropeSprite, ropeSpriteWidth);
                    EditorUtility.SetDirty(targetSwingObject.gameObject);
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private const float SWING_OBJECT_SCALE_HANDLES_FACTOR = 1f;
        private const float ROPE_SCALE_HANDLES_FACTOR = 2f;
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
            Vector3 scalePos = targetSwingObject.GetSwingObjectPosition + Vector3.right * (targetSwingObject.swingObjectModel.localScale.x * SWING_OBJECT_SCALE_HANDLES_FACTOR);
            scalePos = Handles.Slider(scalePos, Vector3.right, 0.1f, Handles.DotHandleCap, 0f);
            float scale = (scalePos.x - targetSwingObject.GetSwingObjectPosition.x);
            Handles.DrawWireDisc(targetSwingObject.GetSwingObjectPosition, Vector3.forward, scale, 0.05f); 
            
            
            //ROPE SCALE HANDLE
            if (!targetSwingObject.dJoin.maxDistanceOnly)
            {
                Vector3 centerPos = targetSwingObject.GetSwingObjectPosition + (targetSwingObject.GetRootAnchorPosition - targetSwingObject.GetSwingObjectPosition) / 2;
                Vector3 scalRopePos = centerPos + (Vector3.right * targetSwingObject.TryGetRopeSpriteWidth() * ROPE_SCALE_HANDLES_FACTOR);
                scalRopePos = Handles.Slider(scalRopePos, Vector3.right, 0.1f, Handles.DotHandleCap, 0f);
                ropeSpriteWidth = (scalRopePos.x - targetSwingObject.GetSwingObjectPosition.x) / ROPE_SCALE_HANDLES_FACTOR;
                Handles.DrawLine(scalRopePos + Vector3.up, scalRopePos + Vector3.down, 0.05f); 
            }

            
            if (EditorGUI.EndChangeCheck())
            {
                targetSwingObject.SetSwingObjectPosition(swingObjectAnchorPosition);
                targetSwingObject.ScaleSprite(scale / SWING_OBJECT_SCALE_HANDLES_FACTOR);
                
                if (!targetSwingObject.dJoin.maxDistanceOnly)
                {
                    targetSwingObject.SetRopeSpriteWidth(ropeSpriteWidth);
                }
                
                EditorUtility.SetDirty(targetSwingObject.gameObject);
            }
        }
        
    }

}
