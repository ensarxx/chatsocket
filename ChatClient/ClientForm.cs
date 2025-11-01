using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class ClientForm : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected = false;
        private string userName;

        // UI Kontrolleri
        private TextBox txtServerIP;
        private TextBox txtPort;
        private TextBox txtUserName;
        private Button btnConnect;
        private Button btnDisconnect;
        private RichTextBox txtChat;
        private TextBox txtMessage;
        private Button btnSend;
        private Label lblServerIP;
        private Label lblPort;
        private Label lblUserName;
        private Label lblStatus;

        public ClientForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Chat İstemcisi";
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += ClientForm_FormClosing;

            // Server IP Label
            lblServerIP = new Label();
            lblServerIP.Text = "Sunucu IP:";
            lblServerIP.Location = new Point(20, 20);
            lblServerIP.Size = new Size(70, 20);
            this.Controls.Add(lblServerIP);

            // Server IP TextBox
            txtServerIP = new TextBox();
            txtServerIP.Location = new Point(95, 18);
            txtServerIP.Size = new Size(120, 20);
            txtServerIP.Text = "127.0.0.1";
            this.Controls.Add(txtServerIP);

            // Port Label
            lblPort = new Label();
            lblPort.Text = "Port:";
            lblPort.Location = new Point(225, 20);
            lblPort.Size = new Size(35, 20);
            this.Controls.Add(lblPort);

            // Port TextBox
            txtPort = new TextBox();
            txtPort.Location = new Point(265, 18);
            txtPort.Size = new Size(60, 20);
            txtPort.Text = "8888";
            this.Controls.Add(txtPort);

            // UserName Label
            lblUserName = new Label();
            lblUserName.Text = "İsim:";
            lblUserName.Location = new Point(20, 55);
            lblUserName.Size = new Size(70, 20);
            this.Controls.Add(lblUserName);

            // UserName TextBox
            txtUserName = new TextBox();
            txtUserName.Location = new Point(95, 53);
            txtUserName.Size = new Size(120, 20);
            txtUserName.Text = "Kullanıcı";
            this.Controls.Add(txtUserName);

            // Connect Button
            btnConnect = new Button();
            btnConnect.Text = "Bağlan";
            btnConnect.Location = new Point(225, 50);
            btnConnect.Size = new Size(100, 25);
            btnConnect.Click += BtnConnect_Click;
            this.Controls.Add(btnConnect);

            // Disconnect Button
            btnDisconnect = new Button();
            btnDisconnect.Text = "Bağlantıyı Kes";
            btnDisconnect.Location = new Point(335, 50);
            btnDisconnect.Size = new Size(100, 25);
            btnDisconnect.Enabled = false;
            btnDisconnect.Click += BtnDisconnect_Click;
            this.Controls.Add(btnDisconnect);

            // Status Label
            lblStatus = new Label();
            lblStatus.Text = "Durum: Bağlı değil";
            lblStatus.Location = new Point(445, 55);
            lblStatus.Size = new Size(130, 20);
            lblStatus.ForeColor = Color.Red;
            this.Controls.Add(lblStatus);

            // Chat RichTextBox
            txtChat = new RichTextBox();
            txtChat.Location = new Point(20, 90);
            txtChat.Size = new Size(550, 340);
            txtChat.ReadOnly = true;
            txtChat.BackColor = Color.White;
            txtChat.Font = new Font("Segoe UI", 10F);
            this.Controls.Add(txtChat);

            // Message TextBox
            txtMessage = new TextBox();
            txtMessage.Location = new Point(20, 440);
            txtMessage.Size = new Size(450, 25);
            txtMessage.Enabled = false;
            txtMessage.Font = new Font("Segoe UI", 10F);
            txtMessage.KeyPress += TxtMessage_KeyPress;
            this.Controls.Add(txtMessage);

            // Send Button
            btnSend = new Button();
            btnSend.Text = "Gönder";
            btnSend.Location = new Point(480, 438);
            btnSend.Size = new Size(90, 28);
            btnSend.Enabled = false;
            btnSend.Click += BtnSend_Click;
            this.Controls.Add(btnSend);
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                MessageBox.Show("Lütfen bir kullanıcı adı girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtServerIP.Text))
            {
                MessageBox.Show("Lütfen sunucu IP adresini girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtPort.Text, out int port) || port < 1024 || port > 65535)
            {
                MessageBox.Show("Lütfen geçerli bir port numarası girin (1024-65535).", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ConnectToServer();
        }

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            DisconnectFromServer();
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void TxtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                SendMessage();
            }
        }

        private async void ConnectToServer()
        {
            try
            {
                string serverIP = txtServerIP.Text;
                int port = int.Parse(txtPort.Text);
                userName = txtUserName.Text;

                client = new TcpClient();
                await client.ConnectAsync(serverIP, port);
                stream = client.GetStream();
                isConnected = true;

                // İsmi sunucuya gönder
                byte[] nameData = Encoding.UTF8.GetBytes($"NAME:{userName}");
                await stream.WriteAsync(nameData, 0, nameData.Length);

                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
                txtServerIP.Enabled = false;
                txtPort.Enabled = false;
                txtUserName.Enabled = false;
                txtMessage.Enabled = true;
                btnSend.Enabled = true;
                lblStatus.Text = "Durum: Bağlı";
                lblStatus.ForeColor = Color.Green;

                AddMessage("Sunucuya bağlandınız.", Color.Green);

                // Mesajları dinle
                Task.Run(() => ReceiveMessages());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bağlantı hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisconnectFromServer();
            }
        }

        private void DisconnectFromServer()
        {
            isConnected = false;

            try
            {
                stream?.Close();
                client?.Close();
            }
            catch { }

            this.Invoke((MethodInvoker)delegate
            {
                btnConnect.Enabled = true;
                btnDisconnect.Enabled = false;
                txtServerIP.Enabled = true;
                txtPort.Enabled = true;
                txtUserName.Enabled = true;
                txtMessage.Enabled = false;
                btnSend.Enabled = false;
                lblStatus.Text = "Durum: Bağlı değil";
                lblStatus.ForeColor = Color.Red;

                AddMessage("Sunucudan ayrıldınız.", Color.Red);
            });
        }

        private async void ReceiveMessages()
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (isConnected && client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    
                    // Sistem mesajlarını farklı renkte göster
                    if (message.StartsWith("SYSTEM:"))
                    {
                        AddMessage(message.Substring(7), Color.Blue);
                    }
                    else
                    {
                        AddMessage(message, Color.Black);
                    }
                }
            }
            catch (Exception)
            {
                if (isConnected)
                {
                    AddMessage("Sunucu bağlantısı kesildi.", Color.Red);
                }
            }
            finally
            {
                DisconnectFromServer();
            }
        }

        private async void SendMessage()
        {
            if (!isConnected || string.IsNullOrWhiteSpace(txtMessage.Text))
                return;

            try
            {
                string message = txtMessage.Text;
                byte[] data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);

                AddMessage($"Ben: {message}", Color.DarkGreen);
                txtMessage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Mesaj gönderme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisconnectFromServer();
            }
        }

        private void AddMessage(string message, Color color)
        {
            if (txtChat.InvokeRequired)
            {
                txtChat.Invoke((MethodInvoker)delegate { AddMessage(message, color); });
                return;
            }

            int startIndex = txtChat.Text.Length;
            txtChat.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            
            // Renk ayarlama
            txtChat.SelectionStart = startIndex;
            txtChat.SelectionLength = txtChat.Text.Length - startIndex;
            txtChat.SelectionColor = color;
            txtChat.SelectionStart = txtChat.Text.Length;
            txtChat.SelectionColor = txtChat.ForeColor;
            txtChat.ScrollToCaret();
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isConnected)
            {
                DisconnectFromServer();
            }
        }
    }
}

