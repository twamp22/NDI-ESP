﻿using eft_dma_shared.Common.Misc;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Svg.Skia;
using System.IO.Compression;
using System.Numerics;

namespace eft_dma_shared.Common.Maps
{
    /// <summary>
    /// SVG Map Implementation.
    /// </summary>
    public sealed class LoneSvgMap : ILoneMap
    {
        private readonly LoneMapConfig.LoadedLayer[] _layers;

        public string ID { get; }
        public LoneMapConfig Config { get; }

        public LoneSvgMap(ZipArchive zip, string id, LoneMapConfig config)
        {
            ID = id;
            Config = config;
            var layers = new List<LoneMapConfig.LoadedLayer>();
            try
            {
                using var paint = new SKPaint()
                {
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.High
                };
                foreach (var layer in config.MapLayers) // Load resources for new map
                {
                    using var stream = zip.Entries.First(x => x.Name
                            .Equals(layer.Filename,
                                StringComparison.OrdinalIgnoreCase))
                        .Open();
                    using var svg = SKSvg.CreateFromStream(stream);
                    // Create an image info with the desired dimensions
                    var scaleInfo = new SKImageInfo(
                        (int)Math.Round(svg.Picture!.CullRect.Width * config.SvgScale),
                        (int)Math.Round(svg.Picture!.CullRect.Height * config.SvgScale));
                    // Create a surface to draw on
                    using (var surface = SKSurface.Create(scaleInfo))
                    {
                        // Clear the surface
                        surface.Canvas.Clear(SKColors.Transparent);
                        // Apply the scale and draw the SVG picture
                        surface.Canvas.Scale(config.SvgScale);
                        surface.Canvas.DrawPicture(svg.Picture, paint);
                        layers.Add(new LoneMapConfig.LoadedLayer(surface.Snapshot(), layer));
                    }
                }
                _layers = layers.Order().ToArray();
            }
            catch
            {
                foreach (var layer in layers) // Unload any partially loaded layers
                {
                    layer.Dispose();
                }
                throw;
            }
        }

        public void Draw(SKCanvas canvas, float playerHeight, SKRect mapBounds, SKRect windowBounds)
        {
            var layers = _layers // Use overridden equality operators
                .Where(layer => layer.IsHeightInRange(playerHeight))
                .Order()
                .ToArray();
            foreach (var layer in layers)
            {
                SKPaint paint;
                if (layers.Length > 1 && layer != layers.Last() && !(layer.IsBaseLayer && layers.Any(x => !x.DimBaseLayer)))
                {
                    paint = SharedPaints.PaintBitmapAlpha;
                }
                else
                {
                    paint = SharedPaints.PaintBitmap;
                }
                canvas.DrawImage(layer.Image, mapBounds, windowBounds, paint);
            }
        }

        /// <summary>
        /// Provides miscellaneous map parameters used throughout the entire render.
        /// </summary>
        public LoneMapParams GetParameters(SKGLControl control, int zoom, ref Vector2 localPlayerMapPos)
        {
            var zoomWidth = _layers[0].Image.Width * (.01f * zoom);
            var zoomHeight = _layers[0].Image.Height * (.01f * zoom);

            var bounds = new SKRect(localPlayerMapPos.X - zoomWidth / 2,
                    localPlayerMapPos.Y - zoomHeight / 2,
                    localPlayerMapPos.X + zoomWidth / 2,
                    localPlayerMapPos.Y + zoomHeight / 2)
                .AspectFill(control.CanvasSize);

            return new LoneMapParams
            {
                Map = Config,
                Bounds = bounds,
                XScale = control.Width / bounds.Width, // Set scale for this frame
                YScale = control.Height / bounds.Height // Set scale for this frame
            };
        }

        public void Dispose()
        {
            for (int i = 0; i < _layers.Length; i++)
            {
                _layers[i]?.Dispose();
                _layers[i] = null;
            }
        }
    }
}
