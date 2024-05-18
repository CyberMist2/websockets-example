using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebsocketClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClientWebSocket _client;

        public MainWindow()
        {
            InitializeComponent();
            _client = new ClientWebSocket();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            Uri serverUri = new Uri("ws://localhost:9090"); // Replace this with your WebSocket server URL
            try
            {
                MessagesListBox.Items.Add($"Connecting to {serverUri}");
                await _client.ConnectAsync(serverUri, CancellationToken.None);
                MessagesListBox.Items.Add("Connected to the WebSocket server.");
                ReceiveMessages();
            }
            catch (Exception ex)
            {
                MessagesListBox.Items.Add($"An error occurred: {ex.Message}");
            }
        }

        private async void ReceiveMessages()
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (_client.State == WebSocketState.Open)
                {
                    var result = await _client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Dispatcher.Invoke(() => MessagesListBox.Items.Add(receivedMessage)); // Update UI
                }
            }
            catch (Exception ex)
            {
                MessagesListBox.Items.Add($"An error occurred: {ex.Message}");
            }
        }

        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = MessageTextBox.Text;
                MessageTextBox.Text = string.Empty;
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                await _client.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                MessagesListBox.Items.Add($"An error occurred: {ex.Message}");
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_client.State == WebSocketState.Open)
            {
                MessagesListBox.Items.Add($"Closing connection..");
                await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }
        }
    }
}
