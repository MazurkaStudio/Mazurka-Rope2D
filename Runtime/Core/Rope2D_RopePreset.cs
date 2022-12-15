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
    }
}

