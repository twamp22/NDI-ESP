using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using NewTek;
using System.Net;

namespace eft_dma_radar.UI.ESP
{
    internal static class NDIManager
    {
        private static IntPtr senderPtr = IntPtr.Zero;
        private static IntPtr senderNamePtr = IntPtr.Zero;

        public static void Initialize(int width, int height, int fps)
        {
            if (!NDIlib.initialize())
                return;

            senderNamePtr = Marshal.StringToHGlobalAnsi("ESP NDI Stream");

            var senderDesc = new NDIlib.send_create_t
            {
                p_ndi_name = senderNamePtr
            };

            senderPtr = NDIlib.send_create(ref senderDesc);

            // Build and send metadata
            string metadataXml = $@"
<ndi_product_metadata>
    <manufacturer>ESP Radar Team</manufacturer>
    <model_name>ESP NDI Streamer</model_name>
    <version>1.0.0</version>
    <serial>0001</serial>
    <ip_address>{GetLocalIPAddress()}</ip_address>
    <os>{Environment.OSVersion}</os>
    <compression>progressive</compression>
    <color_format>BGRA</color_format>
    <frame_rate>{fps}</frame_rate>
    <horizontal_resolution>{width}</horizontal_resolution>
    <vertical_resolution>{height}</vertical_resolution>
    <aspect_ratio>{(float)width / height:F2}</aspect_ratio>
    <bandwidth_mode>high</bandwidth_mode>
</ndi_product_metadata>";

            var metaFrame = new NDIlib.metadata_frame_t
            {
                p_data = Marshal.StringToHGlobalAnsi(metadataXml),
                length = metadataXml.Length,
                timecode = NDIlib.send_timecode_synthesize
            };

            NDIlib.send_add_connection_metadata(senderPtr, ref metaFrame);

            // Free metadata memory
            Marshal.FreeHGlobal(metaFrame.p_data);
        }

        public static void SendFrame(SKPixmap pixmap, int fps)
        {
            if (senderPtr == IntPtr.Zero || pixmap == null)
                return;

            var videoFrame = new NDIlib.video_frame_v2_t
            {
                xres = pixmap.Width,
                yres = pixmap.Height,
                FourCC = NDIlib.FourCC_type_e.FourCC_type_BGRA,
                frame_rate_N = fps,
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

        private static string GetLocalIPAddress()
        {
            string localIP = "127.0.0.1";
            try
            {
                foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch { }
            return localIP;
        }
    }
}
