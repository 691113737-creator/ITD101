using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

// ThaiPass Video App (.exe) — เล่นวิดีโอด้วย MCI (winmm.dll)
// ใช้ AI ThaiPass จำลอง: แสดงชื่อผู้ใช้ + ปุ่ม AI แนะนำวิดีโอ
public class VideoApp : Form
{
    [DllImport("winmm.dll")]
    private static extern long mciSendString(string cmd, StringBuilder ret, int retLen, IntPtr callback);

    private List<string> allVideos = new List<string>();
    private ListBox playlist;
    private Panel videoPanel;
    private Button btnAdd, btnPlay, btnPause, btnStop, btnNext, btnPrev, btnRemove, btnAi, btnFull;
    private Label lblTitle, lblArtist, lblTime, lblCount, lblAi;
    private TextBox txtSearch;
    private TrackBar progressBar, volBar;
    private Timer timer = new Timer();
    private string currentFile = null;
    private bool isPlaying = false;
    private bool dragging = false;
    private Random rnd = new Random();

    // จำลองผู้ใช้ ThaiPass + AI
    private string thaiPassName = "ThaiPass User (Demo)";
    private string thaiPassId = "THAI-1234";

    private Color BG = Color.FromArgb(20, 20, 40);
    private Color CARD = Color.FromArgb(30, 30, 60);
    private Color ACCENT = Color.FromArgb(0, 151, 230);
    private Color BLUE = Color.FromArgb(0, 151, 230);
    private Color GREEN = Color.FromArgb(0, 206, 158);

    public VideoApp()
    {
        this.Text = "ThaiPass Video App";
        this.Size = new Size(560, 860);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.BackColor = BG;
        this.ForeColor = Color.White;
        this.Font = new Font("Segoe UI", 9);

        Console.WriteLine("AI: Hello " + thaiPassName + ", welcome to Video App (.exe)");

        // ===== Header =====
        Panel header = new Panel();
        header.Dock = DockStyle.Top;
        header.Height = 96;
        header.BackColor = BLUE;
        header.Padding = new Padding(16, 12, 16, 12);
        this.Controls.Add(header);

        Label logo = new Label();
        logo.Text = "🎬";
        logo.Font = new Font("Segoe UI", 28);
        logo.ForeColor = Color.White;
        logo.AutoSize = true;
        logo.Location = new Point(14, 14);
        header.Controls.Add(logo);

        Label t1 = new Label();
        t1.Text = "ThaiPass Video";
        t1.Font = new Font("Segoe UI", 18, FontStyle.Bold);
        t1.ForeColor = Color.White;
        t1.AutoSize = true;
        t1.Location = new Point(72, 10);
        header.Controls.Add(t1);

        Label t2 = new Label();
        t2.Text = "👤 " + thaiPassName + "  •  แอปเล่นวิดีโอบนคอม";
        t2.Font = new Font("Segoe UI", 9);
        t2.ForeColor = Color.FromArgb(224, 244, 255);
        t2.AutoSize = true;
        t2.Location = new Point(74, 48);
        header.Controls.Add(t2);

        int y = 108;

        // ===== AI bar =====
        Panel aiPanel = new Panel();
        aiPanel.Location = new Point(14, y);
        aiPanel.Size = new Size(518, 40);
        aiPanel.BackColor = Color.FromArgb(30, 42, 74);
        this.Controls.Add(aiPanel);

        lblAi = new Label();
        lblAi.Text = "🤖 AI ThaiPass: สวัสดี! พร้อมดูวิดีโอแล้ว";
        lblAi.Font = new Font("Segoe UI", 8);
        lblAi.ForeColor = Color.FromArgb(207, 232, 255);
        lblAi.Location = new Point(8, 4);
        lblAi.Size = new Size(370, 32);
        aiPanel.Controls.Add(lblAi);

        btnAi = new Button();
        StyleBtn(btnAi, "✨ AI แนะนำ", Color.FromArgb(108, 92, 231));
        btnAi.Location = new Point(388, 5);
        btnAi.Size = new Size(122, 30);
        btnAi.Click += delegate { AiRecommend(); };
        aiPanel.Controls.Add(btnAi);

        y += 48;

        // ===== Video screen =====
        videoPanel = new Panel();
        videoPanel.Location = new Point(14, y);
        videoPanel.Size = new Size(518, 290);
        videoPanel.BackColor = Color.Black;
        videoPanel.BorderStyle = BorderStyle.FixedSingle;
        videoPanel.Resize += delegate { ResizeVideo(); };
        this.Controls.Add(videoPanel);

        y += 298;

        // ===== Now Playing Card =====
        Panel card = new Panel();
        card.Location = new Point(14, y);
        card.Size = new Size(518, 120);
        card.BackColor = CARD;
        this.Controls.Add(card);
        card.Paint += delegate(object s, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle,
                Color.FromArgb(70, 70, 130), ButtonBorderStyle.Solid);
        };

        lblTitle = new Label();
        lblTitle.Text = "ยังไม่ได้เลือกวิดีโอ";
        lblTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(12, 8);
        lblTitle.Size = new Size(494, 24);
        card.Controls.Add(lblTitle);

        lblArtist = new Label();
        lblArtist.Text = "กด + เพิ่มวิดีโอ แล้วดับเบิลคลิกเพื่อเล่น";
        lblArtist.Font = new Font("Segoe UI", 9);
        lblArtist.ForeColor = Color.FromArgb(180, 180, 210);
        lblArtist.Location = new Point(12, 32);
        lblArtist.Size = new Size(494, 20);
        card.Controls.Add(lblArtist);

        progressBar = new TrackBar();
        progressBar.Location = new Point(8, 54);
        progressBar.Size = new Size(502, 30);
        progressBar.Minimum = 0;
        progressBar.Maximum = 1000;
        progressBar.TickStyle = TickStyle.None;
        progressBar.BackColor = CARD;
        progressBar.MouseDown += delegate { dragging = true; };
        progressBar.MouseUp += delegate { dragging = false; SeekTo(); };
        card.Controls.Add(progressBar);

        lblTime = new Label();
        lblTime.Text = "0:00 / 0:00";
        lblTime.Font = new Font("Segoe UI", 8);
        lblTime.ForeColor = Color.FromArgb(160, 160, 200);
        lblTime.Location = new Point(12, 82);
        lblTime.Size = new Size(200, 18);
        card.Controls.Add(lblTime);

        Label vLab = new Label();
        vLab.Text = "🔊";
        vLab.ForeColor = Color.White;
        vLab.AutoSize = true;
        vLab.Location = new Point(220, 82);
        card.Controls.Add(vLab);

        volBar = new TrackBar();
        volBar.Location = new Point(248, 78);
        volBar.Size = new Size(140, 28);
        volBar.Minimum = 0; volBar.Maximum = 100; volBar.Value = 80;
        volBar.TickStyle = TickStyle.None;
        volBar.BackColor = CARD;
        volBar.Scroll += delegate { SetVolume(volBar.Value); };
        card.Controls.Add(volBar);

        lblCount = new Label();
        lblCount.Text = "0 วิดีโอ";
        lblCount.ForeColor = Color.FromArgb(160, 160, 200);
        lblCount.Font = new Font("Segoe UI", 8);
        lblCount.Location = new Point(400, 82);
        lblCount.Size = new Size(106, 18);
        lblCount.TextAlign = ContentAlignment.MiddleRight;
        card.Controls.Add(lblCount);

        y += 128;

        // ===== Controls =====
        Panel ctrl = new Panel();
        ctrl.Location = new Point(14, y);
        ctrl.Size = new Size(518, 64);
        ctrl.BackColor = BG;
        this.Controls.Add(ctrl);

        btnPrev = MkBtn(ctrl, "⏮", 0, Color.FromArgb(60, 60, 110), 78);
        btnPlay = MkBtn(ctrl, "▶", 86, GREEN, 100);
        btnPause = MkBtn(ctrl, "⏸", 194, Color.FromArgb(255, 165, 2), 78);
        btnStop = MkBtn(ctrl, "⏹", 280, Color.FromArgb(120, 120, 150), 78);
        btnNext = MkBtn(ctrl, "⏭", 366, Color.FromArgb(60, 60, 110), 78);
        btnFull = MkBtn(ctrl, "⛶", 452, ACCENT, 66);

        btnPrev.Click += delegate { Move(-1); };
        btnNext.Click += delegate { Move(1); };
        btnPlay.Click += delegate { PlaySelected(); };
        btnPause.Click += delegate { Pause(); };
        btnStop.Click += delegate { StopAll(); };
        btnFull.Click += delegate { ToggleFull(); };
        btnPlay.Font = new Font("Segoe UI", 14, FontStyle.Bold);

        y += 70;

        // ===== Search + Add =====
        txtSearch = new TextBox();
        txtSearch.Location = new Point(14, y);
        txtSearch.Size = new Size(250, 26);
        txtSearch.Font = new Font("Segoe UI", 10);
        txtSearch.BackColor = Color.FromArgb(48, 48, 90);
        txtSearch.ForeColor = Color.White;
        txtSearch.BorderStyle = BorderStyle.FixedSingle;
        txtSearch.Text = "🔍 ค้นหาวิดีโอ...";
        txtSearch.GotFocus += delegate { if (txtSearch.Text.StartsWith("🔍")) txtSearch.Text = ""; };
        txtSearch.TextChanged += delegate { FilterList(); };
        this.Controls.Add(txtSearch);

        btnAdd = new Button();
        StyleBtn(btnAdd, "+ เพิ่มวิดีโอ", BLUE);
        btnAdd.Location = new Point(274, y - 2);
        btnAdd.Size = new Size(120, 30);
        btnAdd.Click += delegate { AddVideos(); };
        this.Controls.Add(btnAdd);

        btnRemove = new Button();
        StyleBtn(btnRemove, "ลบ", Color.FromArgb(90, 60, 70));
        btnRemove.Location = new Point(402, y - 2);
        btnRemove.Size = new Size(130, 30);
        btnRemove.Click += delegate { RemoveSelected(); };
        this.Controls.Add(btnRemove);

        y += 38;

        // ===== Playlist =====
        playlist = new ListBox();
        playlist.Location = new Point(14, y);
        playlist.Size = new Size(518, 150);
        playlist.BackColor = Color.FromArgb(30, 30, 55);
        playlist.ForeColor = Color.White;
        playlist.Font = new Font("Segoe UI", 10);
        playlist.BorderStyle = BorderStyle.None;
        playlist.DrawMode = DrawMode.OwnerDrawFixed;
        playlist.ItemHeight = 30;
        playlist.DrawItem += Playlist_Draw;
        playlist.DoubleClick += delegate { PlaySelected(); };
        this.Controls.Add(playlist);

        y += 156;

        Label hint = new Label();
        hint.Text = "รองรับ .mp4 / .avi / .wmv / .mpg / .mov  •  ดับเบิลคลิกเพื่อเล่น  •  ลากแถบเพื่อกรอวิดีโอ";
        hint.ForeColor = Color.FromArgb(130, 130, 170);
        hint.Font = new Font("Segoe UI", 8);
        hint.AutoSize = true;
        hint.Location = new Point(14, y);
        this.Controls.Add(hint);

        timer.Interval = 400;
        timer.Tick += delegate { OnTick(); };
        timer.Start();
        this.FormClosing += delegate { StopMci(); };
    }

    private void AiSay(string msg)
    {
        lblAi.Text = "🤖 AI ThaiPass: " + msg;
        Console.WriteLine("AI ThaiPass (" + thaiPassId + "): " + msg);
    }

    private void AiRecommend()
    {
        if (playlist.Items.Count == 0) { AiSay("เพิ่มวิดีโอก่อนนะ แล้วเดี๋ยวแนะนำให้!"); return; }
        int i = rnd.Next(playlist.Items.Count);
        playlist.SelectedIndex = i;
        PlaySelected();
        AiSay("ขอแนะนำ \"" + Path.GetFileNameWithoutExtension(playlist.Items[i].ToString()) + "\" ให้คุณ " + thaiPassName + " 🎉");
    }

    private Button MkBtn(Panel parent, string txt, int x, Color bg, int w)
    {
        Button b = new Button();
        b.Text = txt;
        b.Location = new Point(x, 6);
        b.Size = new Size(w, 52);
        StyleBtn(b, txt, bg);
        b.Font = new Font("Segoe UI", 13, FontStyle.Bold);
        parent.Controls.Add(b);
        return b;
    }

    private void StyleBtn(Button b, string txt, Color bg)
    {
        b.Text = txt;
        b.BackColor = bg;
        b.ForeColor = Color.White;
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bg);
        b.Cursor = Cursors.Hand;
        b.Font = new Font("Segoe UI", 9, FontStyle.Bold);
    }

    private void Playlist_Draw(object sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;
        bool sel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        bool isCur = playlist.Items[e.Index].ToString() == currentFile;
        Color bg = sel ? BLUE : (e.Index % 2 == 0 ? Color.FromArgb(30, 30, 55) : Color.FromArgb(36, 36, 66));
        if (isCur && !sel) bg = Color.FromArgb(22, 50, 74);
        using (SolidBrush br = new SolidBrush(bg))
            e.Graphics.FillRectangle(br, e.Bounds);
        string full = playlist.Items[e.Index].ToString();
        string name = Path.GetFileName(full);
        string mark = isCur ? "▶ " : "🎬 ";
        using (SolidBrush tb = new SolidBrush(Color.White))
            e.Graphics.DrawString(mark + name, e.Font, tb, e.Bounds.X + 8, e.Bounds.Y + 6);
        e.DrawFocusRectangle();
    }

    private void AddVideos()
    {
        using (OpenFileDialog od = new OpenFileDialog())
        {
            od.Filter = "วิดีโอ (*.mp4;*.avi;*.wmv;*.mpg;*.mpeg;*.mov)|*.mp4;*.avi;*.wmv;*.mpg;*.mpeg;*.mov|ทั้งหมด|*.*";
            od.Multiselect = true;
            if (od.ShowDialog() == DialogResult.OK)
            {
                int c = 0;
                foreach (string f in od.FileNames)
                {
                    if (!allVideos.Contains(f)) { allVideos.Add(f); c++; }
                }
                FilterList();
                AiSay("เพิ่มวิดีโอใหม่แล้ว " + c + " ไฟล์ ✅");
                MessageBox.Show("เพิ่ม " + c + " วิดีโอแล้ว!", "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    private void FilterList()
    {
        string q = txtSearch.Text;
        if (q.StartsWith("🔍")) q = "";
        q = q.Trim().ToLower();
        playlist.Items.Clear();
        foreach (string f in allVideos)
        {
            string n = Path.GetFileName(f).ToLower();
            if (q == "" || n.Contains(q)) playlist.Items.Add(f);
        }
        if (playlist.Items.Count > 0 && playlist.SelectedIndex < 0)
            playlist.SelectedIndex = 0;
        lblCount.Text = playlist.Items.Count + " วิดีโอ";
        playlist.Invalidate();
    }

    private void PlaySelected()
    {
        if (playlist.SelectedIndex < 0)
        {
            if (playlist.Items.Count > 0) playlist.SelectedIndex = 0;
            else { MessageBox.Show("กด + เพิ่มวิดีโอ ก่อนนะ"); return; }
        }
        PlayFile(playlist.SelectedItem.ToString());
    }

    private void PlayFile(string f)
    {
        if (!File.Exists(f)) { MessageBox.Show("ไม่พบไฟล์: " + f); return; }
        StopMci();
        currentFile = f;
        // เปิดวิดีโอแบบฝังใน panel
        int hwnd = (int)videoPanel.Handle;
        mciSendString("open \"" + f + "\" type mpegvideo alias MyVideo parent " + hwnd + " style child", null, 0, IntPtr.Zero);
        ResizeVideo();
        mciSendString("play MyVideo", null, 0, IntPtr.Zero);
        SetVolume(volBar.Value);
        isPlaying = true;
        lblTitle.Text = Path.GetFileNameWithoutExtension(f);
        lblArtist.Text = "📁 " + Path.GetFileName(f);
        btnPlay.Text = "⏸";
        AiSay("กำลังเล่น " + Path.GetFileNameWithoutExtension(f));
        playlist.Invalidate();
    }

    private void ResizeVideo()
    {
        if (currentFile == null) return;
        try
        {
            int w = Math.Max(50, videoPanel.ClientSize.Width);
            int h = Math.Max(50, videoPanel.ClientSize.Height);
            mciSendString("put MyVideo window at 0 0 " + w + " " + h, null, 0, IntPtr.Zero);
        }
        catch { }
    }

    private void Pause()
    {
        if (currentFile == null) return;
        if (isPlaying)
        {
            mciSendString("pause MyVideo", null, 0, IntPtr.Zero);
            isPlaying = false;
            lblTitle.Text = "⏸ " + Path.GetFileNameWithoutExtension(currentFile);
            btnPlay.Text = "▶";
        }
        else
        {
            mciSendString("resume MyVideo", null, 0, IntPtr.Zero);
            isPlaying = true;
            lblTitle.Text = Path.GetFileNameWithoutExtension(currentFile);
            btnPlay.Text = "⏸";
        }
    }

    private void StopAll()
    {
        StopMci();
        btnPlay.Text = "▶";
        lblTitle.Text = "หยุดแล้ว";
        progressBar.Value = 0;
        lblTime.Text = "0:00 / 0:00";
        videoPanel.Invalidate();
        playlist.Invalidate();
    }

    private void ToggleFull()
    {
        if (this.FormBorderStyle == FormBorderStyle.None)
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.WindowState = FormWindowState.Normal;
        }
        else
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            ResizeVideo();
        }
    }

    private void StopMci()
    {
        try { mciSendString("stop MyVideo", null, 0, IntPtr.Zero); } catch { }
        try { mciSendString("close MyVideo", null, 0, IntPtr.Zero); } catch { }
        isPlaying = false;
    }

    private void Move(int d)
    {
        if (playlist.Items.Count == 0) return;
        int i = playlist.SelectedIndex;
        if (i < 0) i = 0;
        i = (i + d + playlist.Items.Count) % playlist.Items.Count;
        playlist.SelectedIndex = i;
        PlaySelected();
    }

    private void RemoveSelected()
    {
        if (playlist.SelectedIndex < 0) return;
        string f = playlist.SelectedItem.ToString();
        if (f == currentFile) { StopMci(); currentFile = null; }
        allVideos.Remove(f);
        FilterList();
    }

    private void SetVolume(int v)
    {
        mciSendString("setaudio MyVideo volume to " + (v * 10), null, 0, IntPtr.Zero);
    }

    private int GetLen()
    {
        StringBuilder sb = new StringBuilder(64);
        mciSendString("status MyVideo length", sb, 64, IntPtr.Zero);
        int n = 0;
        int.TryParse(sb.ToString().Trim(), out n);
        return n;
    }

    private int GetPos()
    {
        StringBuilder sb = new StringBuilder(64);
        mciSendString("status MyVideo position", sb, 64, IntPtr.Zero);
        int n = 0;
        int.TryParse(sb.ToString().Trim(), out n);
        return n;
    }

    private string Fmt(int ms)
    {
        int s = ms / 1000;
        return (s / 60) + ":" + (s % 60).ToString("00");
    }

    private void SeekTo()
    {
        if (currentFile == null) return;
        int len = GetLen();
        if (len <= 0) return;
        int pos = (int)((progressBar.Value / 1000.0) * len);
        mciSendString("seek MyVideo to " + pos, null, 0, IntPtr.Zero);
        mciSendString("play MyVideo", null, 0, IntPtr.Zero);
        if (!isPlaying) mciSendString("pause MyVideo", null, 0, IntPtr.Zero);
        ResizeVideo();
    }

    private void OnTick()
    {
        if (currentFile == null) return;
        int len = GetLen();
        int pos = GetPos();
        if (len > 0 && !dragging)
        {
            progressBar.Value = Math.Min(1000, Math.Max(0, (int)((pos * 1000.0) / len)));
            lblTime.Text = Fmt(pos) + " / " + Fmt(len);
            if (isPlaying && pos >= len - 600)
            {
                if (playlist.Items.Count > 1) Move(1);
                else { StopAll(); }
            }
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new VideoApp());
    }
}
