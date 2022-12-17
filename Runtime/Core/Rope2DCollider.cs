using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MazurkaGameKit.Rope2D
{
    [RequireComponent(typeof(Rope2D))]
    [RequireComponent(typeof(EdgeCollider2D))]
    public class Rope2DCollider : MonoBehaviour
    {
        [SerializeField] private EdgeCollider2D edgeCollider;
        [SerializeField] private Rope2D rope;

        private void Reset()
        {
            rope = GetComponent<Rope2D>();
            edgeCollider = GetComponent<EdgeCollider2D>();
        }

        private void Start()
        {
            rope.isEnable += Enable;
        }

        private void OnDestroy()
        {
            rope.isEnable -= Enable;
        }


        private void Enable(bool value)
        {
            gameObject.SetActive(value);
        }
        private void FixedUpdate()
        {
            List<Vector2> v2Positions = rope.RopeSegmentsNow.ToList().Select((v3) => (Vector2)v3).ToList();
            edgeCollider.SetPoints(v2Positions);
        }
    }
}
