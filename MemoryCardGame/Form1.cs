using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Media;
using System.Windows.Forms;

namespace MemoryCardGame
{
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;

            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true
            );

            this.UpdateStyles();
        }
    }
    public partial class Form1 : Form
    {
        // ── 遊戲設定 ──────────────────────────────────────────────
        private const int COLS = 4;
        private const int ROWS = 4;
        private const int CARD_W = 90;
        private const int CARD_H = 120;
        private const int GAP = 14;
        private const int BOARD_X = 40;
        private const int BOARD_Y = 110;

        // ── 撲克牌資料 ────────────────────────────────────────────
        private static readonly string[] SUITS = { "♠", "♥", "♦", "♣" };
        private static readonly string[] RANKS = { "A", "2", "3", "4", "5", "6", "7", "8" };

        // ── 遊戲狀態 ──────────────────────────────────────────────
        private List<CardData> cards = new List<CardData>();
        private int? firstIdx = null;
        private int? secondIdx = null;
        private bool isChecking = false;
        private int moves = 0;
        private int matchedPairs = 0;
        private int seconds = 0;
        private System.Windows.Forms.Timer gameTimer;
        private System.Windows.Forms.Timer flipTimer;

        // ── 音效 ──────────────────────────────────────────────────
        private SoundPlayer flipSound;
        private SoundPlayer matchSound;
        private SoundPlayer winSound;

        // ── UI 控制項 ─────────────────────────────────────────────
        private Label lblTitle;
        private Label lblMoves;
        private Label lblTimer;
        private Label lblPairs;
        private Button btnRestart;
        private DoubleBufferedPanel boardPanel;

        public Form1()
        {
            InitializeComponent();
            BuildUI();
            InitSounds();
            StartNewGame();
        }

        // ═══════════════════════════════════════════════════════════
        //  UI 建構
        // ═══════════════════════════════════════════════════════════
        private void BuildUI()
        {
            this.DoubleBuffered = true;

            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true
            );

            this.UpdateStyles();
            this.Text = "🃏 記憶翻牌遊戲";
            this.ClientSize = new Size(
                COLS * (CARD_W + GAP) + GAP + BOARD_X * 2,
                ROWS * (CARD_H + GAP) + GAP + BOARD_Y + 70);
            this.BackColor = Color.FromArgb(18, 40, 76);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // 標題
            lblTitle = new Label
            {
                Text = "♠ 記憶翻牌遊戲 ♥",
                Font = new Font("微軟正黑體", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                AutoSize = true,
                Location = new Point(60, 12)
            };
            this.Controls.Add(lblTitle);

            // 步數
            lblMoves = new Label
            {
                Text = "步數：0",
                Font = new Font("微軟正黑體", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(55, 70)
            };
            this.Controls.Add(lblMoves);

            // 計時器
            lblTimer = new Label
            {
                Text = "時間：00:00",
                Font = new Font("微軟正黑體", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 220, 255),
                AutoSize = true,
                Location = new Point(225, 70)
            };
            this.Controls.Add(lblTimer);

            // 配對數
            lblPairs = new Label
            {
                Text = "配對：0 / 8",
                Font = new Font("微軟正黑體", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 255, 160),
                AutoSize = true,
                Location = new Point(405, 70)
            };
            this.Controls.Add(lblPairs);

            // 重新開始按鈕
            btnRestart = new Button
            {
                Text = "重新開始",
                Font = new Font("微軟正黑體", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(255, 215, 0),
                ForeColor = Color.FromArgb(18, 40, 76),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(90, 30),
                Location = new Point(405, 100)
            };
            btnRestart.FlatAppearance.BorderSize = 0;
            btnRestart.Click += (s, e) => StartNewGame();
            this.Controls.Add(btnRestart);

            Label lblHint = new Label
            {
                Text = "遊戲說明：翻兩張相同點數的牌即可配對",
                Font = new Font("微軟正黑體", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 230, 255),
                AutoSize = true,
                Location = new Point(55, 105)
            };
            this.Controls.Add(lblHint);

            // 棋盤面板
            boardPanel = new DoubleBufferedPanel
            {
                Location = new Point(BOARD_X, 140),
                Size = new Size(
                COLS * (CARD_W + GAP) + GAP,
                ROWS * (CARD_H + GAP) + GAP),
                BackColor = Color.FromArgb(12, 28, 60)
            };
            boardPanel.Paint += BoardPanel_Paint;
            boardPanel.MouseClick += BoardPanel_MouseClick;
            this.Controls.Add(boardPanel);

            // 計時器
            gameTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            gameTimer.Tick += (s, e) =>
            {
                seconds++;
                lblTimer.Text = $"時間：{seconds / 60:00}:{seconds % 60:00}";
            };

            flipTimer = new System.Windows.Forms.Timer { Interval = 700 };
            flipTimer.Tick += FlipTimer_Tick;


        }

        // ═══════════════════════════════════════════════════════════
        //  音效初始化（使用 Beep 模擬，不需外部檔案）
        // ═══════════════════════════════════════════════════════════
        private void InitSounds() { /* 使用 Console.Beep 代替 */ }

        private void PlayFlipSound()
        {
            try { System.Media.SystemSounds.Asterisk.Play(); } catch { }
        }
        private void PlayMatchSound()
        {
            try { System.Media.SystemSounds.Exclamation.Play(); } catch { }
        }
        private void PlayWinSound()
        {
            try { System.Media.SystemSounds.Hand.Play(); } catch { }
        }

        // ═══════════════════════════════════════════════════════════
        //  遊戲邏輯
        // ═══════════════════════════════════════════════════════════
        private void StartNewGame()
        {
            flipTimer.Stop();
            gameTimer.Stop();
            seconds = 0;
            moves = 0;
            matchedPairs = 0;
            firstIdx = null;
            secondIdx = null;
            isChecking = false;

            lblMoves.Text = "步數：0";
            lblTimer.Text = "時間：00:00";
            lblPairs.Text = "配對：0 / 8";

            // 建立 8 對牌
            cards.Clear();
            var pool = new List<(string suit, string rank)>();
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 2; j++)
                    pool.Add((SUITS[i % 4], RANKS[i]));

            // 洗牌
            var rng = new Random();
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int k = rng.Next(i + 1);
                (pool[i], pool[k]) = (pool[k], pool[i]);
            }

            for (int r = 0; r < ROWS; r++)
                for (int c = 0; c < COLS; c++)
                {
                    int idx = r * COLS + c;
                    var (suit, rank) = pool[idx];
                    cards.Add(new CardData(suit, rank, c, r));
                }

            gameTimer.Start();
            boardPanel.Invalidate();
        }

        private void BoardPanel_MouseClick(object sender, MouseEventArgs e)
        {
            if (isChecking) return;

            int col = (e.X - GAP / 2) / (CARD_W + GAP);
            int row = (e.Y - GAP / 2) / (CARD_H + GAP);
            int idx = row * COLS + col;

            if (col < 0 || col >= COLS || row < 0 || row >= ROWS) return;
            if (idx < 0 || idx >= cards.Count) return;

            var card = cards[idx];
            if (card.IsMatched || card.IsFaceUp) return;
            if (firstIdx.HasValue && idx == firstIdx.Value) return;

            // 翻牌
            card.IsFaceUp = true;
            PlayFlipSound();
            boardPanel.Invalidate(GetCardRect(card));

            if (!firstIdx.HasValue)
            {
                firstIdx = idx;
            }
            else
            {
                secondIdx = idx;
                moves++;
                lblMoves.Text = $"步數：{moves}";
                isChecking = true;
                flipTimer.Start();
            }
        }

        private void FlipTimer_Tick(object sender, EventArgs e)
        {
            flipTimer.Stop();
            isChecking = false;

            var c1 = cards[firstIdx.Value];
            var c2 = cards[secondIdx.Value];

            if (c1.Rank == c2.Rank)
            {
                // 配對成功
                c1.IsMatched = true;
                c2.IsMatched = true;
                matchedPairs++;
                lblPairs.Text = $"配對：{matchedPairs} / 8";
                PlayMatchSound();

                if (matchedPairs == 8)
                {
                    gameTimer.Stop();
                    PlayWinSound();
                    boardPanel.Invalidate();
                    MessageBox.Show(
                        $"🎉 恭喜完成！\n\n⏱ 用時：{seconds / 60:00}:{seconds % 60:00}\n👆 步數：{moves}",
                        "遊戲結束",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            else
            {
                // 不配對，翻回
                c1.IsFaceUp = false;
                c2.IsFaceUp = false;
            }

            firstIdx = null;
            secondIdx = null;
            boardPanel.Invalidate();
        }

        // ═══════════════════════════════════════════════════════════
        //  繪製
        // ═══════════════════════════════════════════════════════════
        private void BoardPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            for (int i = 0; i < cards.Count; i++)
                DrawCard(g, cards[i], i == firstIdx || i == secondIdx);
        }

        private Rectangle GetCardRect(CardData card)
        {
            int x = GAP + card.Col * (CARD_W + GAP);
            int y = GAP + card.Row * (CARD_H + GAP);

            return new Rectangle(x - 3, y - 3, CARD_W + 6, CARD_H + 6);
        }

        private void DrawCard(Graphics g, CardData card, bool isSelected)
        {
            int x = GAP + card.Col * (CARD_W + GAP);
            int y = GAP + card.Row * (CARD_H + GAP);
            Rectangle rect = new Rectangle(x, y, CARD_W, CARD_H);

            GraphicsPath path = RoundRect(rect, 10);

            if (card.IsMatched)
            {
                using (SolidBrush bg = new SolidBrush(Color.FromArgb(30, 180, 100)))
                {
                    g.FillPath(bg, path);
                }

                using (Pen border = new Pen(Color.FromArgb(80, 220, 140), 2))
                {
                    g.DrawPath(border, path);
                }

                DrawCardFace(g, card, x, y, true);
            }
            else if (card.IsFaceUp)
            {
                Color bgColor;

                if (isSelected)
                    bgColor = Color.FromArgb(255, 250, 220);
                else
                    bgColor = Color.White;

                using (SolidBrush bg = new SolidBrush(bgColor))
                {
                    g.FillPath(bg, path);
                }

                Color borderColor;

                if (isSelected)
                    borderColor = Color.FromArgb(255, 200, 0);
                else
                    borderColor = Color.FromArgb(180, 180, 200);

                using (Pen border = new Pen(borderColor, 2.5f))
                {
                    g.DrawPath(border, path);
                }

                DrawCardFace(g, card, x, y, false);
            }
            else
            {
                DrawCardBack(g, path, x, y);
            }

            path.Dispose();
        }

        private void DrawCardBack(Graphics g, GraphicsPath path, int x, int y)
        {
            using (LinearGradientBrush bg = new LinearGradientBrush(
                new Point(x, y),
                new Point(x + CARD_W, y + CARD_H),
                Color.FromArgb(25, 70, 160),
                Color.FromArgb(10, 40, 100)))
            {
                g.FillPath(bg, path);
            }

            using (Pen pen = new Pen(Color.FromArgb(40, 100, 200), 1))
            {
                for (int dx = 8; dx < CARD_W; dx += 12)
                {
                    g.DrawLine(pen, x + dx, y + 4, x + dx, y + CARD_H - 4);
                }

                for (int dy = 8; dy < CARD_H; dy += 12)
                {
                    g.DrawLine(pen, x + 4, y + dy, x + CARD_W - 4, y + dy);
                }
            }

            using (Font fnt = new Font("Segoe UI", 22, FontStyle.Bold))
            using (SolidBrush br = new SolidBrush(Color.FromArgb(60, 120, 220)))
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                g.DrawString("🂠", fnt, br, x + CARD_W / 2f, y + CARD_H / 2f, sf);

                sf.Dispose();
            }

            using (Pen border = new Pen(Color.FromArgb(50, 100, 180), 2))
            {
                g.DrawPath(border, path);
            }
        }

        private void DrawCardFace(Graphics g, CardData card, int x, int y, bool dimmed)
        {
            bool isRed = card.Suit == "♥" || card.Suit == "♦";

            Color suitColor;

            if (dimmed)
            {
                if (isRed)
                    suitColor = Color.FromArgb(120, 180, 100);
                else
                    suitColor = Color.FromArgb(80, 160, 90);
            }
            else
            {
                if (isRed)
                    suitColor = Color.FromArgb(210, 30, 30);
                else
                    suitColor = Color.FromArgb(20, 20, 20);
            }

            using (Font rankFont = new Font("微軟正黑體", 13, FontStyle.Bold))
            using (Font suitFont = new Font("Segoe UI", 26, FontStyle.Bold))
            using (Font smallSuitFont = new Font("Segoe UI", 10))
            using (SolidBrush brush = new SolidBrush(suitColor))
            {
                // 左上角：點數 + 花色
                g.DrawString(card.Rank, rankFont, brush, x + 5, y + 4);
                g.DrawString(card.Suit, smallSuitFont, brush, x + 5, y + 22);

                // 右下角：旋轉 180 度
                g.TranslateTransform(x + CARD_W - 5, y + CARD_H - 4);
                g.RotateTransform(180);

                g.DrawString(card.Rank, rankFont, brush, 0, 0);
                g.DrawString(card.Suit, smallSuitFont, brush, 0, 18);

                g.ResetTransform();

                // 中央大花色
                StringFormat centerSf = new StringFormat();
                centerSf.Alignment = StringAlignment.Center;
                centerSf.LineAlignment = StringAlignment.Center;

                g.DrawString(
                    card.Suit,
                    suitFont,
                    brush,
                    x + CARD_W / 2f,
                    y + CARD_H / 2f,
                    centerSf
                );

                centerSf.Dispose();
            }
        }

        private static GraphicsPath RoundRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(r.Right - radius * 2, r.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(r.Right - radius * 2, r.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(r.X, r.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  資料類別
    // ═══════════════════════════════════════════════════════════════
    public class CardData
    {
        public string Suit { get; }
        public string Rank { get; }
        public int Col { get; }
        public int Row { get; }
        public bool IsFaceUp { get; set; }
        public bool IsMatched { get; set; }

        public CardData(string suit, string rank, int col, int row)
        {
            Suit = suit; Rank = rank; Col = col; Row = row;
        }
    }
}
