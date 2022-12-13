using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Mazurka2DGameKit.Rope2D
{
    [CreateAssetMenu(fileName = ("New Rope Preset"), menuName = ("Rope2D/Rope Preset"))]
    public class Rope2D_RopePreset : ScriptableObject
    {

        [FoldoutGroup("Rope Width")] public float ropeWidth = 0.2f;
        [FoldoutGroup("Rope Width")] public AnimationCurve lineWidth;
        [FoldoutGroup("Rope Render")] public Gradient lineColor;
        [FoldoutGroup("Rope Render")] public Material ropeMat;
        [FoldoutGroup("Rope Render"), ValueDropdown("SortingLayers")] public string sortingLayerName = "MID1";

#if UNITY_EDITOR
        private void OnEnable()
        {
            SortingLayers = Editor.EditorExtension.GetSortingLayerNames();
        }

        private string[] SortingLayers;
#endif

        [FoldoutGroup("Rope Lenght")][Range(5, 40)] public int RopeSubDivision = 25;
        [FoldoutGroup("Rope Lenght")][Range(-4f, 4f)] public float defaultRopeTension = 0f;
        [PropertyTooltip("When currDistance great maxDistance, maxdistance and segments lenght are update")]
        [FoldoutGroup("Rope Lenght")] public bool isExtendable = false;
        [PropertyTooltip("When currDistance less maxDistance, maxdistance and segments lenght are update")]
        [FoldoutGroup("Rope Lenght")] public bool isStretchable = false;
        [FoldoutGroup("Rope Simulation")][Range(1, 30)] public int iteration = 12;
        [FoldoutGroup("Rope Simulation")] public Vector2 gravity = Vector2.down;
        [FoldoutGroup("Rope Simulation")] public float damp = 0f;
        [FoldoutGroup("Rope Simulation")] public SimulationUpdate simulationUpdate = SimulationUpdate.FixedUpdate;
        [FoldoutGroup("Rope Simulation")] public SimulationUpdate renderUpdate = SimulationUpdate.FixedUpdate;
        [FoldoutGroup("Rope Simulation")] public bool ropeCanBreak = false;
        [FoldoutGroup("Rope Simulation")][Range(1f, 10f)] public float ropeBreakThreshold = 1.5f;
    }
}

