/* HenkChat
 * Copyright (C) 2018  henkje (henkje@pm.me)
 * 
 * MIT license
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.System;
using System.Security.Cryptography;
using System.Text;
using HenkTcp;

namespace HenkChat
{
    public sealed partial class MainPage : Page
    {
        const int MAX_MESSAGES = 500;
        const int POPUP_TIME = 1500;
        const int MESSAGES_LOAD_AT_CONNECT = 50 - 1;

        private HenkTcpClient _Client;
        public byte[] EncryptionKeyServer, EndToEndKey, Salt;

        public MainPage()
        {
            this.InitializeComponent();

            object Ip = Settings.GetIp(), Username = Settings.GetUsername();
            if (Ip != null) ServerName_TB.Text = Ip.ToString();
            if (Username != null) UserName_TB.Text = Username.ToString();
        }

        private void _Connect(object sender, RoutedEventArgs e)
        {
            if (Popup_GRD.Visibility == Visibility.Visible) { Popup_Text.Text = "You are trying to quick"; return; }

            Settings.SetIp(ServerName_TB.Text);
            Settings.SetUsername(UserName_TB.Text);

            _Client = new HenkTcpClient();
            if (!Connection.Connect(ServerName_TB.Text, _Client)) { _ShowPopup("The server could not be found."); return; }

            int x = Connection.Establish(ref EncryptionKeyServer, ref Salt, _Client, Password_PWB.Password, UserName_TB.Text);
            if (x.Equals(0)) _ShowPopup("Server timed out");
            else if (x.Equals(1)) _ShowPopup("Wrong password");
            else if (x.Equals(2)) _ShowPopup("UserName is already taken");
            else if (x.Equals(3)) _Connected();
        }

        private void _Connected()
        {
            EndToEndKey = HenkTcp.Encryption.CreateKey(Aes.Create(), Password_PWB.Password, Encoding.UTF8.GetString(Salt));

            try
            {
                Int64 End = BitConverter.ToInt64(_Client.WriteAndGetReply(new byte[] { 42, 5 }, TimeSpan.FromSeconds(1)).DecryptedData, 0);

                Int64 Start = 1;
                if (End > MESSAGES_LOAD_AT_CONNECT) Start = End - MESSAGES_LOAD_AT_CONNECT;

                for (Int64 i = Start; i <= End; i++)
                {
                    try
                    {
                        byte[] Data = _Client.WriteAndGetReply(Connection.CombineBytes(new byte[] { 42, 5 }, BitConverter.GetBytes(i)), TimeSpan.FromSeconds(1)).Data;
                        Messages_LV.Items.Add(Functions.CreateMessageBox(Encoding.UTF8.GetString(HenkTcp.Encryption.Decrypt(Aes.Create(), Data, EndToEndKey)), UserName_TB.Text));
                    }
                    catch { }
                }

                Messages_LV.Items.Add(Functions.CreateMessageBox($"Welcome to {ServerName_TB.Text}.", UserName_TB.Text));
            }
            catch { _ShowPopup("Error while connecting to the server."); _OnDisconnect(null, null); }

            Chat_GRD.Visibility = Visibility.Visible;
            Connect_GRD.Visibility = Visibility.Collapsed;

            _Client.DataReceived += _DataReceived;
            _Client.OnDisconnect += _OnDisconnect;
        }

        private async void _OnDisconnect(object sender, HenkTcpClient e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Chat_GRD.Visibility = Visibility.Collapsed;
                Connect_GRD.Visibility = Visibility.Visible;

                Messages_LV.Items.Clear();
                Message_TB.Text = string.Empty;

                _Client.DataReceived -= _DataReceived;
                _Client.OnDisconnect -= _OnDisconnect;
            });
        }

        private async void _DataReceived(object sender, Message e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    Messages_LV.Items.Add(Functions.CreateMessageBox(Encoding.UTF8.GetString(HenkTcp.Encryption.Decrypt(Aes.Create(), e.Data, EndToEndKey)), UserName_TB.Text));

                    if (Messages_LV.Items.Count > MAX_MESSAGES) Messages_LV.Items.RemoveAt(0);
                }
                catch { }
            });
        }


        private void _KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e) { if (e.Key == VirtualKey.Enter) _Send(null, null); }
        private void _Send(object sender, RoutedEventArgs e)
        {
            if (Message_TB.Text.Length < 2) return;

            try
            {
                if (Message_TB.Text.StartsWith("!"))
                {
                    if (Message_TB.Text.Equals("!leave")) _Client.Disconnect(true);
                    else
                    {
                        _Client.DataReceived -= _DataReceived;
                        Messages_LV.Items.Add(Functions.CreateMessageBox(Connection.SendCommand(Message_TB.Text, _Client, EncryptionKeyServer, Password_PWB.Password, Salt), UserName_TB.Text));
                        _Client.DataReceived += _DataReceived;
                    }
                }
                else
                {
                    byte[] Message = HenkTcp.Encryption.Encrypt(Aes.Create(), Encoding.UTF8.GetBytes(UserName_TB.Text + ":" + Message_TB.Text), EndToEndKey);
                    if (Message.Length > 1024) { _ShowPopup("Message is too long"); return; }

                    _Client.Write(Message);
                }
                Message_TB.Text = string.Empty;
            }
            catch { _OnDisconnect(null, null); }
        }

        private async void _ShowPopup(string Text)
        {
            Popup_GRD.Visibility = Visibility.Visible;
            Popup_Text.Text = Text;
            await Task.Delay(POPUP_TIME);
            Popup_GRD.Visibility = Visibility.Collapsed;
        }
    }
}
