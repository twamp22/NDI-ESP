﻿using eft_dma_shared.Common.Misc.Data;
using SkiaSharp;
using System.Text.Json.Serialization;

namespace eft_dma_shared.Common.Maps
{
    /// <summary>
    /// Defines a .JSON Map Config File
    /// </summary>
    public sealed class LoneMapConfig
    {
        /// <summary>
        /// Name of map (Ex: CUSTOMS)
        /// </summary>
        [JsonIgnore]
        public string Name =>
            GameData.MapNames[MapID[0]].ToUpper();

        /// <summary>
        /// Map ID(s) for this Map.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("mapID")]
        public List<string> MapID { get; private set; }
        /// <summary>
        /// Bitmap 'X' Coordinate of map 'Origin Location' (where Unity X is 0).
        /// </summary>
        [JsonPropertyName("x")]
        public float X { get; set; }
        /// <summary>
        /// Bitmap 'Y' Coordinate of map 'Origin Location' (where Unity Y is 0).
        /// </summary>
        [JsonPropertyName("y")]
        public float Y { get; set; }
        /// <summary>
        /// Arbitrary scale value to align map scale between the Bitmap and Game Coordinates.
        /// </summary>
        [JsonPropertyName("scale")]
        public float Scale { get; set; }
        /// <summary>
        /// How much to scale up the original SVG Image.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("svgScale")]
        public float SvgScale { get; private set; }
        /// <summary>
        /// Contains the Map Layers to load for the current Map Configuration.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("mapLayers")]
        public List<Layer> MapLayers { get; private set; }

        /// <summary>
        /// A single layer of a Multi-Layered Map.
        /// </summary>
        public sealed class Layer
        {
            /// <summary>
            /// Minimum height (Unity Y Coord) for this map layer.
            /// NULL: No minimum height.
            /// </summary>
            [JsonInclude]
            [JsonPropertyName("minHeight")]
            public float? MinHeight { get; private set; }
            /// <summary>
            /// Maximum height (Unity Y Coord) for this map layer.
            /// NULL: No maximum height.
            /// </summary>
            [JsonInclude]
            [JsonPropertyName("maxHeight")]
            public float? MaxHeight { get; private set; }
            /// <summary>
            /// True if this layer can dim the Base Layer when it is in the foreground.
            /// </summary>
            [JsonInclude]
            [JsonPropertyName("dimBaseLayer")]
            public bool DimBaseLayer { get; private set; } = true;
            /// <summary>
            /// Relative File path to this map layer's PNG Image.
            /// </summary>
            [JsonInclude]
            [JsonPropertyName("filename")]
            public string Filename { get; private set; }
        }

        public sealed class LoadedLayer : IDisposable, IComparable<LoadedLayer>
        {
            private readonly Layer _layer;

            /// <summary>
            /// Image for this Map Layer.
            /// </summary>
            public SKImage Image { get; }

            public LoadedLayer(SKImage image, Layer layer)
            {
                this.Image = image;
                _layer = layer;
            }

            /// <summary>
            /// True if this layer can dim the Base Layer when it is in the foreground.
            /// </summary>
            public bool DimBaseLayer => _layer.DimBaseLayer;

            /// <summary>
            /// True if this is a Base Layer, and should always be drawn.
            /// </summary>
            public bool IsBaseLayer => _layer.MinHeight is null && _layer.MaxHeight is null;

            /// <summary>
            /// Minimum Height for this Layer.
            /// </summary>
            private float MinHeight => this._layer.MinHeight ?? float.MinValue;
            /// <summary>
            /// Maximum Height for this Layer.
            /// </summary>
            private float MaxHeight => this._layer.MaxHeight ?? float.MaxValue;

            public static bool operator <(LoadedLayer a, LoadedLayer b)
            {
                if (a.IsBaseLayer && !b.IsBaseLayer)
                {
                    return true;
                }
                return a.MinHeight < b.MinHeight;
            }

            public static bool operator >(LoadedLayer a, LoadedLayer b)
            {
                if (!a.IsBaseLayer && b.IsBaseLayer)
                {
                    return true;
                }
                return a.MinHeight > b.MinHeight;
            }

            /// <summary>
            /// Check if a height is within the range of this layer.
            /// </summary>
            /// <param name="height">Height (usually LocalPlayer height).</param>
            /// <returns>True if within this layer, otherwise False.</returns>
            public bool IsHeightInRange(float height)
            {
                return height > MinHeight && height < MaxHeight;
            }

            public int CompareTo(LoadedLayer other)
            {
                if (this < other)
                    return -1;
                if (this > other)
                    return 1;
                return 0;
            }

            public void Dispose()
            {
                this.Image?.Dispose();
            }
        }
    }
}
