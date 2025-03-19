﻿using System.Numerics;
using System.Runtime.InteropServices;

namespace eft_dma_shared.Common.ESP
{
    /// <summary>
    /// Defines a transposed Matrix4x4 for ESP Operations (only contains necessary fields).
    /// </summary>
    public sealed class ViewMatrix
    {
        public float M44;
        public float M14;
        public float M24;

        public Vector3 Translation;
        public Vector3 Right;
        public Vector3 Up;

        public ViewMatrix() { }

        public void Update(ref Matrix4x4 matrix) 
        {
            /// Transpose necessary fields
            M44 = matrix.M44;
            M14 = matrix.M41;
            M24 = matrix.M42;
            Translation.X = matrix.M14;
            Translation.Y = matrix.M24;
            Translation.Z = matrix.M34;
            Right.X = matrix.M11;
            Right.Y = matrix.M21;
            Right.Z = matrix.M31;
            Up.X = matrix.M12;
            Up.Y = matrix.M22;
            Up.Z = matrix.M32;
        }
    }
}
