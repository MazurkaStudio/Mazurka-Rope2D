using UnityEngine;

namespace Mazurka2DGameKit.Rope2D
{
    public class Rope2D_Object : MonoBehaviour
    {


        [SerializeField] private Rope2D rope;

        [SerializeField, Range(0f, 1f)] private float atPercentOfRope = .5f;
        private int atRopeSeg;

        private bool IsActive;

        [SerializeField] private GameObject sprite;

        private void Start()
        {

            FindRopeSegToFollow();
            sprite.SetActive(false);
        }

        private void FindRopeSegToFollow()
        {
            atRopeSeg = Mathf.FloorToInt(rope.Preset.RopeSubDivision * atPercentOfRope);

            if (atRopeSeg >= rope.Preset.RopeSubDivision - 2)
            {
                atRopeSeg = rope.Preset.RopeSubDivision - 2;
            }
        }

        private void Update()
        {
            if (rope != null)
            {
                if (rope.CanBeSimulate)
                {
                    if (!IsActive)
                    {

                        sprite.SetActive(true);
                        IsActive = true;
                    }
                    transform.position = rope.RopeSegmentsNow[atRopeSeg];
                    transform.rotation = Quaternion.LookRotation(rope.RopeSegmentsNow[atRopeSeg + 1] - rope.RopeSegmentsNow[atRopeSeg], Vector3.up);
                    return;
                }
            }

            if (IsActive)
            {
                sprite.SetActive(false);
                IsActive = false;
            }
        }
    }

}

