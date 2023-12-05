using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Diagnostics;
#pragma warning disable

namespace Server
{
    public class Server
    {

        static void Main(string[] args)
        {
            var Listener = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp
            );


            var IP = IPAddress.Parse("127.0.0.1");
            var Port = 4000;

            var EndPoint = new IPEndPoint(IP, Port);

            var msg = "";
            var len = 0;

            Listener.Bind(EndPoint);  // Server 127.0.0.1:4000 EndPoint-ne Bind() Funksiyasi Ile Qosulur

            EndPoint remote_EndPoint = new IPEndPoint(IPAddress.Any, 0);  // Remote EndPoint Server-le Elaqe Quracaq Client-in EndPoint-dir
                                                                          // IPAddress.Any -> Istenilen IP addressin , 0 -> Istenilen Portu
            while (true)
            {
                var buffer = new byte[ushort.MaxValue - 29];
                len = Listener.ReceiveFrom(buffer, ref remote_EndPoint);

                msg = Encoding.Default.GetString(buffer, 0, len);

                if (msg == "Send Screenshoot")
                {
                    Console.WriteLine($"{remote_EndPoint.ToString()}: {msg}");

                    var bitmap_image = CaptureMyScreen();

                    var bytes_image = ImageToByte(bitmap_image);

                    Console.WriteLine(bytes_image.Length);

                    // Fragmentation
                    int chunkSize = 1024; // Set your desired chunk size
                    for (int i = 0; i < bytes_image.Length; i += chunkSize)
                    {
                        int remainingBytes = Math.Min(chunkSize, bytes_image.Length - i); // 1024 chixir her defe ta ki , alinan reqem 1024-den kicik olsun
                        byte[] chunk = new byte[remainingBytes]; // chixilan olchude byte massivi yaradilir .
                        Buffer.BlockCopy(bytes_image, i, chunk, 0, remainingBytes); // Ve Boyuk Image-den 1024 ve ya daha kick byte yaradilan massive kocurulur

                        // Send each chunk
                        try
                        {
                            Listener.SendTo(chunk, SocketFlags.None, remote_EndPoint);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error sending data: {ex.Message}");
                            // Handle the error as needed
                        }
                        Console.WriteLine(chunk.Length);

                    }


                }
            }

        }

        static private Bitmap CaptureMyScreen()
        {
            try
            {
                var width = Screen.PrimaryScreen.Bounds.Width;
                var height = Screen.PrimaryScreen.Bounds.Height;
                Bitmap Capture_bitmap = new Bitmap(width, height);

                Rectangle Capture_rectangle = Screen.AllScreens[0].Bounds;

                Graphics Capture_graphics = Graphics.FromImage(Capture_bitmap);
                Capture_graphics.CopyFromScreen(Capture_rectangle.Left, Capture_rectangle.Top, 0, 0, Capture_rectangle.Size);

                return Capture_bitmap;

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        static byte[] ImageToByte(Image img)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }
    }
}