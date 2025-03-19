﻿namespace eft_dma_shared.Common.Ballistics
{
    internal readonly struct G1DragModel
    {
        public readonly float Mach;
        public readonly float Ballist;

        public G1DragModel(float mach, float ballist)
        {
            Mach = mach;
            Ballist = ballist;
        }
    }
}
