using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MazurkaGameKit.Rope2D
{
    public interface I2DRopeExternalForce
    {
        public void Initialize(Rope2D rope);
        public bool IsGlobal { get; }
        public Vector3 GetExternalForce(int index);
        public Vector3 GetGlobalForce();
    }
}
