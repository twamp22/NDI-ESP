using eft_dma_radar.Tarkov.EFTPlayer;
using eft_dma_radar.UI.ESP;
using eft_dma_radar.UI.Misc;
using eft_dma_radar.UI.Radar;
using eft_dma_shared.Common.ESP;
using eft_dma_shared.Common.Maps;
using eft_dma_shared.Common.Players;
using eft_dma_shared.Common.Unity;

namespace eft_dma_radar.Tarkov.GameWorld.Explosives
{
    /// <summary>
    /// Represents a 'Hot' grenade in Local Game World.
    /// </summary>
    public sealed class Grenade : IExplosiveItem, IWorldEntity, IMapEntity, IESPEntity
    {
        public static implicit operator ulong(Grenade x) => x.Addr;
        private static readonly uint[] _toPosChain =
            ObjectClass.To_GameObject.Concat(new uint[] { GameObject.ComponentsOffset, 0x8, 0x38 }).ToArray();
        private readonly Stopwatch _sw = Stopwatch.StartNew();
        private readonly ConcurrentDictionary<ulong, IExplosiveItem> _parent;

        /// <summary>
        /// Base Address of Grenade Object.
        /// </summary>
        public ulong Addr { get; }

        /// <summary>
        /// Position Pointer for the Vector3 location of this object.
        /// </summary>
        private ulong PosAddr { get; }

        /// <summary>
        /// True if grenade is currently active.
        /// </summary>
        public bool IsActive => _sw.Elapsed.TotalSeconds < 12f;

        /// <summary>
        /// True if the grenade has detonated.
        /// Doesn't work on smoke grenades.
        /// </summary>
        private bool IsDetonated
        {
            get
            {
                return Memory.ReadValue<bool>(this + Offsets.Grenade.IsDestroyed, false);
            }
        }

        public Grenade(ulong baseAddr, ConcurrentDictionary<ulong, IExplosiveItem> parent)
        {
            Addr = baseAddr;
            _parent = parent;
            if (IsDetonated)
                throw new Exception("Grenade is already detonated.");
            PosAddr = Memory.ReadPtrChain(baseAddr, _toPosChain, false);
            Refresh();
        }

        /// <summary>
        /// Get the updated Position of this Grenade.
        /// </summary>
        public void Refresh()
        {
            if (!this.IsActive)
            {
                return;
            }
            else if (IsDetonated)
            {
                _parent.TryRemove(this, out _);
                return;
            }
            Position = Memory.ReadValue<Vector3>(PosAddr + 0x90, false);
        }

        #region Interfaces

        private Vector3 _position;
        public ref Vector3 Position => ref _position;

        public void Draw(SKCanvas canvas, LoneMapParams mapParams, ILocalPlayer localPlayer)
        {
            if (!IsActive)
                return;
            var circlePosition = Position.ToMapPos(mapParams.Map).ToZoomedPos(mapParams);
            var size = 5 * MainForm.UIScale;
            SKPaints.ShapeOutline.StrokeWidth = SKPaints.PaintExplosives.StrokeWidth + 2f * MainForm.UIScale;
            canvas.DrawCircle(circlePosition, size, SKPaints.ShapeOutline); // Draw outline
            canvas.DrawCircle(circlePosition, size, SKPaints.PaintExplosives); // draw LocalPlayer marker
        }

        public void DrawESP(SKCanvas canvas, LocalPlayer localPlayer)
        {
            if (!IsActive)
                return;
            if (Vector3.Distance(localPlayer.Position, Position) > ESP.Config.GrenadeDrawDistance)
                return;
            if (!CameraManagerBase.WorldToScreen(ref _position, out var scrPos))
                return;
            var circleRadius = 8f * ESP.Config.LineScale;
            canvas.DrawCircle(scrPos, circleRadius, SKPaints.PaintGrenadeESP);
        }

        #endregion
    }
}
