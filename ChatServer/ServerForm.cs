using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatServer
{
    public partial class ServerForm : Form
    {
        private TcpListener server;
        private List<TcpClient> clients = new List<TcpClient>();
        private Dictionary<TcpClient, string> clientNames = new Dictionary<TcpClient, string>();
        private bool isRunning = false;
        private int port = 8888;

        // UI Kontrolleri
        private TextBox txtLog;
        private TextBox txtPort;
        private Button btnStart;
        private Button btnStop;
        private ListBox lstClients;
        private Label lblStatus;
        private Label lblPort;
        private Label lblClients;

        public ServerForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Chat Sunucusu";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += ServerForm_FormClosing;

            // Port Label
            lblPort = new Label();
            lblPort.Text = "Port:";
            lblPort.Location = new Point(20, 20);
            lblPort.Size = new Size(50, 20);
            this.Controls.Add(lblPort);

            // Port TextBox
            txtPort = new TextBox();
            txtPort.Location = new Point(70, 18);
            txtPort.Size = new Size(80, 20);
            txtPort.Text = "8888";
            this.Controls.Add(txtPort);

            // Start Button
            btnStart = new Button();
            btnStart.Text = "Sunucuyu Başlat";
            btnStart.Location = new Point(160, 15);
            btnStart.Size = new Size(120, 25);
            btnStart.Click += BtnStart_Click;
            this.Controls.Add(btnStart);

            // Stop Button
            btnStop = new Button();
            btnStop.Text = "Sunucuyu Durdur";
            btnStop.Location = new Point(290, 15);
            btnStop.Size = new Size(120, 25);
            btnStop.Enabled = false;
            btnStop.Click += BtnStop_Click;
            this.Controls.Add(btnStop);

            // Status Label
            lblStatus = new Label();
            lblStatus.Text = "Durum: Durduruldu";
            lblStatus.Location = new Point(420, 20);
            lblStatus.Size = new Size(150, 20);
            lblStatus.ForeColor = Color.Red;
            this.Controls.Add(lblStatus);

            // Clients Label
            lblClients = new Label();
            lblClients.Text = "Bağlı Kullanıcılar:";
            lblClients.Location = new Point(20, 55);
            lblClients.Size = new Size(120, 20);
            this.Controls.Add(lblClients);

            // Clients ListBox
            lstClients = new ListBox();
            lstClients.Location = new Point(20, 80);
            lstClients.Size = new Size(200, 350);
            this.Controls.Add(lstClients);

            // Log TextBox
            txtLog = new TextBox();
            txtLog.Location = new Point(230, 55);
            txtLog.Size = new Size(340, 375);
            txtLog.Multiline = true;
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.BackColor = Color.White;
            this.Controls.Add(txtLog);
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtPort.Text, out port) || port < 1024 || port > 65535)
            {
                MessageBox.Show("Lütfen geçerli bir port numarası girin (1024-65535).", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            StartServer();
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            StopServer();
        }

        private void StartServer()
        {
            try
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                isRunning = true;

                btnStart.Enabled = false;
                btnStop.Enabled = true;
                txtPort.Enabled = false;
                lblStatus.Text = "Durum: Çalışıyor";
                lblStatus.ForeColor = Color.Green;

                AddLog($"Sunucu başlatıldı. Port: {port}");
                AddLog($"Yerel IP: {GetLocalIPAddress()}");

                // Yeni bağlantıları dinle
                Task.Run(() => ListenForClients());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sunucu başlatılamadı: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopServer()
        {
            isRunning = false;

            // Tüm istemcileri kapat
            foreach (var client in clients.ToList())
            {
                try
                {
                    client.Close();
                }
                catch { }
            }
            clients.Clear();
            clientNames.Clear();

            // Sunucuyu kapat
            server?.Stop();

            this.Invoke((MethodInvoker)delegate
            {
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                txtPort.Enabled = true;
                lblStatus.Text = "Durum: Durduruldu";
                lblStatus.ForeColor = Color.Red;
                lstClients.Items.Clear();
            });

            AddLog("Sunucu durduruldu.");
        }

        private async void ListenForClients()
        {
            while (isRunning)
            {
                try
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    clients.Add(client);
                    AddLog($"Yeni bağlantı: {((IPEndPoint)client.Client.RemoteEndPoint).Address}");

                    // Her istemci için ayrı bir thread başlat
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    if (isRunning)
                    {
                        AddLog($"Bağlantı hatası: {ex.Message}");
                    }
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096];
            string clientName = "";

            try
            {
                while (isRunning && client.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    
                    // İsim kontrolü (ilk mesaj)
                    if (string.IsNullOrEmpty(clientName))
                    {
                        if (message.StartsWith("NAME:"))
                        {
                            clientName = message.Substring(5);
                            clientNames[client] = clientName;
                            UpdateClientList();
                            AddLog($"{clientName} sohbete katıldı.");
                            BroadcastMessage($"SYSTEM:{clientName} sohbete katıldı.", client);
                            continue;
                        }
                    }

                    AddLog($"{clientName}: {message}");
                    BroadcastMessage($"{clientName}: {message}", client);
                }
            }
            catch (Exception ex)
            {
                AddLog($"İstemci hatası: {ex.Message}");
            }
            finally
            {
                if (!string.IsNullOrEmpty(clientName))
                {
                    AddLog($"{clientName} ayrıldı.");
                    BroadcastMessage($"SYSTEM:{clientName} ayrıldı.", client);
                }

                clients.Remove(client);
                clientNames.Remove(client);
                UpdateClientList();
                client.Close();
            }
        }

        private void BroadcastMessage(string message, TcpClient sender)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);

            foreach (var client in clients.ToList())
            {
                if (client != sender && client.Connected)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(data, 0, data.Length);
                    }
                    catch
                    {
                        // Bağlantı kopmuş
                    }
                }
            }
        }

        private void UpdateClientList()
        {
            if (lstClients.InvokeRequired)
            {
                lstClients.Invoke((MethodInvoker)delegate { UpdateClientList(); });
                return;
            }

            lstClients.Items.Clear();
            foreach (var name in clientNames.Values)
            {
                lstClients.Items.Add(name);
            }
        }

        private void AddLog(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke((MethodInvoker)delegate { AddLog(message); });
                return;
            }

            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
        }

        private string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "IP bulunamadı";
        }

        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isRunning)
            {
                StopServer();
            }
        }
    }
}

