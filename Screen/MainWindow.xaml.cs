using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Screen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket Client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        static IPAddress remote_ip = IPAddress.Parse("127.0.0.1");
        static int remote_port = 4000;

        EndPoint remote_endpoint = new IPEndPoint(remote_ip, remote_port);

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
  

            SetImageSource();
        }

        private async void SetImageSource()
        {

            while (true)
            {
                string message = "Send Screenshoot";

                byte[] buffer = Encoding.Default.GetBytes(message);

                Client.SendTo(buffer, remote_endpoint);


                try
                {
                    // Use asynchronous methods for network operations
                    var chunkSize = 1024; // Set your desired chunk size
                    var receiveBuffer = new List<byte>();  // Gelen Byte-lar Burda Toplanir [1024 , 1024 , 640]
                    int bytesRead; // Oxunan Byte-in olchusu bura yazilir 1024 , 450 , 230

                    do
                    {
                        var chunk = new byte[chunkSize];
                        bytesRead = await Task.Run(() =>
                        {
                            return Client.ReceiveFrom(chunk, SocketFlags.None, ref remote_endpoint);
                        });

                        if (bytesRead > 0)
                        {

                            receiveBuffer.AddRange(chunk.Take(bytesRead));
                        }

                    } while (bytesRead == chunkSize);

                    var image = LoadImage(receiveBuffer.ToArray());

                    // Use Dispatcher.InvokeAsync for UI updates
                    await Dispatcher.InvokeAsync(() => { ImageFrame.Source = image; });
                }
                catch (Exception ex)
                {
                    // Handle exceptions more specifically or log them
                    MessageBox.Show($"Error loading image: {ex.Message}");
                }

            }
        }



        private static BitmapImage? LoadImage(byte[] imageData)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = new MemoryStream(imageData);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            return image;
        }


    }
}