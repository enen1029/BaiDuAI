using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
namespace WindowsFormsEXE
{
    public partial class Form1 : Form
    {
        private TicketManager ticketManager;
        private List<Thread> playerThreads = new List<Thread>();
        private volatile bool isPaused = false;
        private volatile bool isSelling = true;
        private readonly object pauseLock = new object();

        public Form1()
        {
            InitializeComponent();
            InitUI();
        }

        private void InitUI()
        {
            button2.Enabled = false;
            button3.Enabled = false;
            label1.Text = "剩余票数: -";
        }

        private void UpdateUI(string playerName, string status, string waitTime = "-")
        {
            if (listView1.InvokeRequired)
            {
                listView1.Invoke(new Action(() => UpdateUI(playerName, status, waitTime)));
                return;
            }
            var item = new ListViewItem(new[]
            {
                (listView1.Items.Count + 1).ToString(), playerName, status, waitTime
            });
            listView1.Items.Add(item);
            item.EnsureVisible();
            label1.Text = $"剩余票数: {ticketManager.RemainingTickets}";
        }
        private void AddSystemMessage(string message)
        {
            if (listView1.InvokeRequired)
            {
                listView1.Invoke(new Action(() => AddSystemMessage(message)));
                return;
            }
            var item = new ListViewItem(new[]
            {
                "-", "系统", message, "-"
            });
            listView1.Items.Add(item);
            item.EnsureVisible();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            int players = (int)numericUpDown1.Value;
            int tickets = (int)numericUpDown2.Value;

            listView1.Items.Clear();
            playerThreads.Clear();
            isPaused = false;
            isSelling = true;

            ticketManager = new TicketManager(tickets);
            label1.Text = $"剩余票数: {ticketManager.RemainingTickets}";

            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = false;

            for (int i = 1; i <= players; i++)
            {
                var playerName = $"玩家{i}";
                var thread = new Thread(() => PlayerRoutine(playerName));
                thread.Start();
                playerThreads.Add(thread);
            }
        }

        private void PlayerRoutine(string name)
        {
            int waitMs = new Random().Next(300, 800); // 模拟“准备时间”但不影响抢票公平
            Thread.Sleep(waitMs);
            string waitTime = waitMs + "ms";
            lock (pauseLock)
            {
                while (isPaused)
                    Monitor.Wait(pauseLock);
            }

            if (!isSelling)
            {
                UpdateUI(name, "抢票失败,停售", waitTime);
                return;
            }

            bool success;
            lock (ticketManager)
            {
                success = ticketManager.TryGrabTicket();
                if (!success) isSelling = false;
            }

            string status = success ? "抢票成功" : "抢票失败，售罄";

            UpdateUI(name, status, waitTime);

            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            isPaused = true;
            button2.Enabled = false;
            button3.Enabled = true;
            AddSystemMessage("系统暂停售票");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            lock (pauseLock)
            {
                isPaused = false;
                Monitor.PulseAll(pauseLock);
            }
            button2.Enabled = true;
            button3.Enabled = false;
        }

        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void progressBar1_Click(object sender, EventArgs e) { }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e) { }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }

    public class TicketManager
    {
        private int tickets;
        private readonly object lockObj = new object();
        private readonly Random random = new Random();

        public int RemainingTickets => tickets;

        public TicketManager(int total)
        {
            tickets = total;
        }

        public bool TryGrabTicket()
        {
            if (tickets > 0)
            {
                tickets--;
                return true;
            }
            return false;
        }
    }
}