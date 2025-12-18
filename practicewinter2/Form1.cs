using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace practicewinter2
{
    public partial class Form1 : Form
    {
        class Player
        {
            public Rectangle Bounds { get; set; }
            public int Speed { get; set; } = 5;

            public Player(Point location)
            {
                Bounds = new Rectangle(location, new Size(50, 50));
            }

            public void Move(int deltaX, int deltaY, Panel gamePanel)
            {
                int newX = Bounds.X + deltaX;
                int newY = Bounds.Y + deltaY;

                if (newX >= 0 && newX + Bounds.Width <= gamePanel.Width)
                    Bounds = new Rectangle(newX, Bounds.Y, Bounds.Width, Bounds.Height);

                if (newY >= 0 && newY + Bounds.Height <= gamePanel.Height)
                    Bounds = new Rectangle(Bounds.X, newY, Bounds.Width, Bounds.Height);
            }
        }

        class Collectible
        {
            public Rectangle Bounds { get; set; }
            public bool IsCollected { get; set; }

            public Collectible(Random random, Panel gamePanel)
            {
                int x = random.Next(0, gamePanel.Width - 15);
                int y = random.Next(0, gamePanel.Height - 15);
                Bounds = new Rectangle(x, y, 15, 15);
            }
        }

        private Player player;
        private List<Collectible> collectibles;
        private Random random;
        private int score;
        private int gameTime;
        private bool isGameActive;
        private DateTime lastSpawnTime;
        private int spawnInterval = 2000;
        private System.Collections.Hashtable keyTable = new System.Collections.Hashtable();

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;

            InitializeGame();
        }

        private void InitializeGame()
        {
            random = new Random();
            collectibles = new List<Collectible>();
            score = 0;
            gameTime = 60;
            isGameActive = false;
            spawnInterval = 2000;

            player = new Player(new Point(
                panel1.Width / 2 - 10,
                panel1.Height / 2 - 10
            ));

            UpdateLabels();
            panel1.Paint += Panel1_Paint;

        }

        private void UpdateLabels()
        {
            label1.Text = "Счет: " + score;
            label2.Text = "Время: " + gameTime;
        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(panel1.BackColor);

            using (Brush playerBrush = new SolidBrush(Color.LightSeaGreen))
            {
                e.Graphics.FillRectangle(playerBrush, player.Bounds);
                e.Graphics.DrawRectangle(Pens.Black, player.Bounds);
            }

            foreach (var obj in collectibles)
            {
                if (!obj.IsCollected)
                {
                    using (Brush objBrush = new SolidBrush(Color.Magenta))
                    {
                        e.Graphics.FillEllipse(objBrush, obj.Bounds);
                        e.Graphics.DrawEllipse(Pens.Black, obj.Bounds);
                    }
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (isGameActive)
            {
                if (keyData == Keys.Left || keyData == Keys.Right ||
                    keyData == Keys.Up || keyData == Keys.Down)
                {
                    keyTable[keyData] = true;
                    return true; 
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (keyTable.ContainsKey(e.KeyCode))
            {
                keyTable.Remove(e.KeyCode);
            }
        }

        private void StartGame()
        {
            collectibles.Clear();
            score = 0;
            gameTime = 60;
            spawnInterval = 2000;
            isGameActive = true;

            keyTable.Clear();

            player.Bounds = new Rectangle(
                panel1.Width / 2 - 10,
                panel1.Height / 2 - 10,
                50, 50
            );

            for (int i = 0; i < 3; i++)
            {
                collectibles.Add(new Collectible(random, panel1));
            }

            lastSpawnTime = DateTime.Now;

            timer1.Start();
            timer2.Start();
            UpdateLabels();
            this.Focus();
            panel1.Invalidate();
        }

        private void EndGame()
        {
            isGameActive = false;
            timer1.Stop();
            timer2.Stop();
            keyTable.Clear();

            MessageBox.Show("Игра окончена! Ваш счет: " + score,
                "Результат",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartGame();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!isGameActive) return;

            if (keyTable.ContainsKey(Keys.Left))
                player.Move(-player.Speed, 0, panel1);
            if (keyTable.ContainsKey(Keys.Right))
                player.Move(player.Speed, 0, panel1);
            if (keyTable.ContainsKey(Keys.Up))
                player.Move(0, -player.Speed, panel1);
            if (keyTable.ContainsKey(Keys.Down))
                player.Move(0, player.Speed, panel1);

            for (int i = collectibles.Count - 1; i >= 0; i--)
            {
                if (player.Bounds.IntersectsWith(collectibles[i].Bounds))
                {
                    collectibles.RemoveAt(i);
                    score++;
                    UpdateLabels();
                }
            }

            if ((DateTime.Now - lastSpawnTime).TotalMilliseconds > spawnInterval)
            {
                collectibles.Add(new Collectible(random, panel1));
                lastSpawnTime = DateTime.Now;

                if (spawnInterval > 500)
                    spawnInterval -= 50;
            }

            panel1.Invalidate();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (!isGameActive) return;

            gameTime--;
            UpdateLabels();

            if (gameTime <= 0)
            {
                EndGame();
            }
        }
    }
}