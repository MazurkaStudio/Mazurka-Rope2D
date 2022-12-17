using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MazurkaGameKit.Rope2D
{
    public static class Rope2DEditorHelper 
    {
        public static void FocusObject(GameObject gmaeObject)
        {
            Selection.activeGameObject = gmaeObject;
            SceneView.FrameLastActiveSceneView();
        }
    }
}
