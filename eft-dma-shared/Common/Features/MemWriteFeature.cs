﻿using eft_dma_shared.Common.DMA.ScatterAPI;
using System.Diagnostics;

namespace eft_dma_shared.Common.Features
{
    public abstract class MemWriteFeature<T> : IFeature, IMemWriteFeature
        where T : IMemWriteFeature
    {
        /// <summary>
        /// Singleton Instance.
        /// </summary>
        public static T Instance { get; }

        private readonly Stopwatch _sw = Stopwatch.StartNew();

        static MemWriteFeature()
        {
            Instance = Activator.CreateInstance<T>();
            IFeature.Register(Instance);
        }

        public virtual bool Enabled { get; set; }

        protected virtual TimeSpan Delay => TimeSpan.FromMilliseconds(10);

        protected bool DelayElapsed => Delay == TimeSpan.Zero || _sw.Elapsed >= Delay;

        public virtual bool CanRun
        {
            get
            {
                if (!Memory.InRaid || !Memory.RaidHasStarted)
                    return false;
                if (!DelayElapsed)
                    return false;
                return true;
            }
        }

        public virtual void TryApply(ScatterWriteHandle writes)
        {
        }

        public void OnApply()
        {
            if (Delay != TimeSpan.Zero)
            {
                _sw.Restart();
            }
        }

        public virtual void OnGameStart()
        {
        }

        public virtual void OnRaidStart()
        {
        }

        public virtual void OnRaidEnd()
        {
        }

        public virtual void OnGameStop()
        {
        }
    }
}
