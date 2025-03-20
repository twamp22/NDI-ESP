using System;
using System.Drawing;
using System.Drawing.Imaging;
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

        public static void SendFrame(Bitmap bmp)
        {
            if (senderPtr == IntPtr.Zero)
                return;

            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            var videoFrame = new NDIlib.video_frame_v2_t
            {
                xres = bmp.Width,
                yres = bmp.Height,
                FourCC = NDIlib.FourCC_type_e.FourCC_type_BGRA,
                frame_rate_N = 60,
                frame_rate_D = 1,
                line_stride_in_bytes = bmpData.Stride,
                p_data = bmpData.Scan0,
                picture_aspect_ratio = (float)bmp.Width / bmp.Height,
                frame_format_type = NDIlib.frame_format_type_e.frame_format_type_progressive  // Force progressive frames
            };

            NDIlib.send_send_video_v2(senderPtr, ref videoFrame);
            bmp.UnlockBits(bmpData);
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
