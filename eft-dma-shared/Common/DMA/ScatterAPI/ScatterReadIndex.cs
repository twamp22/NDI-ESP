﻿using eft_dma_shared.Common.Misc.Pools;
using System.Runtime.CompilerServices;

namespace eft_dma_shared.Common.DMA.ScatterAPI
{
    /// <summary>
    /// Single scatter read index. May contain multiple child entries.
    /// </summary>
    public sealed class ScatterReadIndex : IPooledObject<ScatterReadIndex>
    {
        /// <summary>
        /// All read entries for this index.
        /// [KEY] = ID
        /// [VALUE] = IScatterEntry
        /// </summary>
        internal Dictionary<int, IScatterEntry> Entries { get; } = new();
        /// <summary>
        /// Callback to execute on completion.
        /// NOTE: Exceptions will be automatically handled.
        /// </summary>
        public Action<ScatterReadIndex> Callbacks { get; set; }

        [Obsolete("You must rent this object via IPooledObject!")]
        public ScatterReadIndex() { }

        /// <summary>
        /// Execute the User Specified Callback.
        /// </summary>
        internal void ExecuteCallback()
        {
            var cbs = Callbacks;
            if (cbs is not null)
            {
                foreach (var del in cbs.GetInvocationList())
                {
                    try
                    {
                        if (del is Action<ScatterReadIndex> cb)
                        {
                            cb.Invoke(this);
                        }
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Add a scatter read entry to this index.
        /// </summary>
        /// <typeparam name="T">Type to read.</typeparam>
        /// <param name="id">Unique ID for this entry.</param>
        /// <param name="address">Virtual Address to read from.</param>
        /// <param name="cb">Count of bytes to read.</param>
        public ScatterReadEntry<T> AddEntry<T>(int id, ulong address, int cb = 0)
        {
            var entry = ScatterReadEntry<T>.Get(address, cb);
            Entries.Add(id, entry);
            return entry;
        }

        /// <summary>
        /// Try obtain a result from the requested Entry ID.
        /// </summary>
        /// <typeparam name="TOut">Result Type <typeparamref name="TOut"/></typeparam>
        /// <param name="id">ID for entry to lookup.</param>
        /// <param name="result">Result field to populate.</param>
        /// <returns>True if successful, otherwise False.</returns>
        public bool TryGetResult<TOut>(int id, out TOut result)
        {
            if (Entries.TryGetValue(id, out var entry) && entry is ScatterReadEntry<TOut> casted && !casted.IsFailed)
            {
                result = casted.Result;
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Try obtain a ref from the requested Entry ID.
        /// WARNING: Must check the returned ref result for NULLPTR.
        /// </summary>
        /// <typeparam name="TOut">Result Type <typeparamref name="TOut"/></typeparam>
        /// <param name="id">ID for entry to lookup.</param>
        /// <returns>Ref if successful, otherwise NULL.</returns>
        public ref TOut GetRef<TOut>(int id)
        {
            if (Entries.TryGetValue(id, out var entry) && entry is ScatterReadEntry<TOut> casted && !casted.IsFailed)
            {
                return ref casted.Result;
            }
            return ref Unsafe.NullRef<TOut>();
        }

        public void Dispose()
        {
            IPooledObject<ScatterReadIndex>.Return(this);
        }

        public void SetDefault()
        {
            foreach (var entry in Entries.Values)
                entry.Dispose();
            Entries.Clear();
            Callbacks = default;
        }
    }
}
