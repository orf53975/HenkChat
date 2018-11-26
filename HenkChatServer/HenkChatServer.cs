/* HenkChatServer
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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HenkTcp;

namespace HenkChat
{
    class HenkChatServer
    {
        public HenkChatServer(string ServerFolder) => this.ServerFolder = ServerFolder;

        public HenkTcpServer Server = new HenkTcpServer();
        public Dictionary<int, User> UserList = new Dictionary<int, User>();

        public string S_AdminPassword, S_Name, ServerFolder;
        public byte[] S_Salt, S_Password, S_NameBytes;
        public bool AdvancedLog = false;

        public async Task Start(List<string> BannedIps, string Ip)
        {
            int Port, MaxConnections;
            try
            {
                Console.WriteLine($"[{new System.IO.DirectoryInfo(ServerFolder).Name} {DateTime.Now.ToString("h:mm:ss")}] Reading config...");
                new ConfigLoader(ServerFolder, this, out Port, out MaxConnections);
                Server.BannedIps = BannedIps;
                if (Server == null) return;

                ClientHandler ClientHandler = new ClientHandler(this);
                Server.ClientConnected += ClientHandler.ClientConnected;
                Server.ClientDisconnected += ClientHandler.ClientDisconnected;
                Server.DataReceived += new DataHandler(this,ServerFolder).DataReceived;
                Server.OnError += OnError;
                if(AdvancedLog)
                {
                    Server.ClientConnected += (object sender, TcpClient e) => Functions.Log($"{((IPEndPoint)e.Client.RemoteEndPoint).Address.ToString()} connected",this);
                    Server.ClientDisconnected += (object sender, TcpClient e) => Functions.Log($"{((IPEndPoint)e.Client.RemoteEndPoint).Address.ToString()} disconnected", this);
                    Server.DataReceived += (object sender, Message e) => Functions.Log($"{((IPEndPoint)e.TcpClient.Client.RemoteEndPoint).Address.ToString()} sends {e.Data.Length} bytes", this);
                }

                Functions.Print("Starting server...",this,ConsoleColor.White);                
                Server.Start(Ip, Port, MaxConnections);
                if (Server != null) { Functions.Print($"Server is online on {Ip}:{Port}", this, ConsoleColor.Green); Program.Servers.Add(Encoding.UTF8.GetString(new Rfc2898DeriveBytes(S_Name, new byte[] { 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16 }, 10000).GetBytes(20)), Port); }

                await Task.Delay(-1);
            }
            catch (Exception ex) { Functions.Error(ex, this, true); }
        }

        public void OnError(object sender, Exception e) => Functions.Error(e, this, true);       
    }
}
