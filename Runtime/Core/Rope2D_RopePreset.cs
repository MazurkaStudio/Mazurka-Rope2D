using Sirenix.OdinInspector;
using UnityEngine;

namespace MazurkaGameKit.Rope2D
{
    [CreateAssetMenu(fileName = ("New Rope Preset"), menuName = ("Rope2D/Rope Preset"))]
    public class Rope2D_RopePreset : ScriptableObject
    {
        public float ropeWidth = 0.2f;
        public AnimationCurve lineWidth;
        public Gradient lineColor;
        public Material ropeMat;
        
        [Space]
        [Space]
        [Space]
        
        //Rope Simulation
        [Range(1, 30)] public int iteration = 12;
        public Vector2 gravity = Vector2.down;
        public float damp;
        
    }
}

