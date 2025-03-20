using SkiaSharp;
using System.Runtime.InteropServices;
using NewTek;

namespace eft_dma_radar.UI.ESP
{
    internal static class NDIManager
    {
        private static IntPtr senderPtr = IntPtr.Zero;
        private static IntPtr senderNamePtr = IntPtr.Zero;

        public static void Initialize()
        {
            if (!NDIlib.initialize())
                return;

            senderNamePtr = Marshal.StringToHGlobalAnsi("ESP NDI Stream");
            var senderDesc = new NDIlib.send_create_t
            {
                p_ndi_name = senderNamePtr
            };
            senderPtr = NDIlib.send_create(ref senderDesc);
        }

        public static void SendFrame(SKPixmap pixmap)
        {
            if (senderPtr == IntPtr.Zero || pixmap == null)
                return;

            var videoFrame = new NDIlib.video_frame_v2_t
            {
                xres = pixmap.Width,
                yres = pixmap.Height,
                FourCC = NDIlib.FourCC_type_e.FourCC_type_BGRA,
                frame_rate_N = 60,
                frame_rate_D = 1,
                line_stride_in_bytes = pixmap.RowBytes,
                p_data = pixmap.GetPixels(),
                picture_aspect_ratio = (float)pixmap.Width / pixmap.Height,
                frame_format_type = NDIlib.frame_format_type_e.frame_format_type_progressive
            };

            NDIlib.send_send_video_v2(senderPtr, ref videoFrame);
        }

        public static void Shutdown()
        {
            if (senderPtr != IntPtr.Zero)
            {
                NDIlib.send_destroy(senderPtr);
                senderPtr = IntPtr.Zero;
            }
            if (senderNamePtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(senderNamePtr);
                senderNamePtr = IntPtr.Zero;
            }

            NDIlib.destroy();
        }
    }
}
