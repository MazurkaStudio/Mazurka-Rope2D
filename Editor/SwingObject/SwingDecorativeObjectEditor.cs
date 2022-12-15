using UnityEngine;
using UnityEditor;


namespace MazurkaGameKit.Rope2D
{
    [CustomEditor(typeof(SwingDecorativeObject))]
    [CanEditMultipleObjects]
    public class SwingDecorativeObjectEditor : Editor
    {
        private SwingDecorativeObject targetSwingObject;

        private bool simulate;
        private Sprite swingObjectSprite;
        private Sprite ropeSprite;
        private float ropeSpriteWidth;

        private void OnEnable()
        {
            targetSwingObject = (SwingDecorativeObject)target;

            targetSwingObject.CreateRope();

            simulate = false;
            swingObjectSprite = targetSwingObject.GetSwingObjectSpriteRenderer.sprite;
            ropeSpriteWidth = targetSwingObject.TryGetRopeSpriteWidth();
            ropeSprite = targetSwingObject.TryGetRopeSprite();
            EditorUtility.SetDirty(targetSwingObject.gameObject);
        }

        private void OnDisable()
        {
            if (targetSwingObject != null)
            {
                targetSwingObject.simulate = false;
                targetSwingObject.ResetSwingObject();
            }
            
            simulate = false;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            
            base.OnInspectorGUI();

            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(50f);
            
            EditorGUILayout.BeginVertical("HelpBox");

            simulate = EditorGUILayout.Toggle("Simulate", simulate);

            if (simulate != targetSwingObject.simulate)
            {
                targetSwingObject.ResetSwingObject();
                targetSwingObject.NewSeed();
                targetSwingObject.simulate = simulate;
            }

            swingObjectSprite = (Sprite)EditorGUILayout.ObjectField("Swing Object Sprite", swingObjectSprite, typeof(Sprite), false);

            if (swingObjectSprite != targetSwingObject.GetSwingObjectSpriteRenderer.sprite)
            {
                targetSwingObject.SetSprite(swingObjectSprite);
                EditorUtility.SetDirty(targetSwingObject.gameObject);
            }
            
            ropeSprite = (Sprite)EditorGUILayout.ObjectField("Rope Sprite", ropeSprite, typeof(Sprite), false);

            if (ropeSprite != targetSwingObject.TryGetRopeSprite())
            {
                targetSwingObject.CreateSpriteRope(ropeSprite, ropeSpriteWidth);
                EditorUtility.SetDirty(targetSwingObject.gameObject);
            }
            
            EditorGUILayout.EndVertical();
        }

        private const float SWING_OBJECT_SCALE_HANDLES_FACTOR = 5f;
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
            Vector3 centerPos = targetSwingObject.GetSwingObjectPosition + (targetSwingObject.GetRootAnchorPosition - targetSwingObject.GetSwingObjectPosition) / 2;
            Vector3 scalRopePos = centerPos + (Vector3.right * targetSwingObject.TryGetRopeSpriteWidth() * ROPE_SCALE_HANDLES_FACTOR);
            scalRopePos = Handles.Slider(scalRopePos, Vector3.right, 0.1f, Handles.DotHandleCap, 0f);
            ropeSpriteWidth = (scalRopePos.x - targetSwingObject.GetSwingObjectPosition.x) / ROPE_SCALE_HANDLES_FACTOR;
            Handles.DrawLine(scalRopePos + Vector3.up, scalRopePos + Vector3.down, 0.05f); 
            
            if (EditorGUI.EndChangeCheck())
            {
                targetSwingObject.SetSwingObjectPosition(swingObjectAnchorPosition);
                targetSwingObject.ScaleSprite(scale / SWING_OBJECT_SCALE_HANDLES_FACTOR);
                targetSwingObject.SetRopeSpriteWidth(ropeSpriteWidth);
                
                EditorUtility.SetDirty(targetSwingObject.gameObject);
            }
        }
        
    }

}
