using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

public class MusicApp : Form
{
    [DllImport("winmm.dll")]
    private static extern long mciSendString(string cmd, StringBuilder ret, int retLen, IntPtr callback);

    private List<string> allSongs = new List<string>();
    private ListBox playlist;
    private Button btnAdd, btnPlay, btnPause, btnStop, btnNext, btnPrev, btnRemove;
    private Label lblTitle, lblArtist, lblTime, lblCount;
    private TextBox txtSearch;
    private TrackBar progressBar, volBar;
    private Timer timer = new Timer();
    private string currentFile = null;
    private bool isPlaying = false;
    private bool dragging = false;

    private Color BG = Color.FromArgb(26, 26, 46);
    private Color CARD = Color.FromArgb(34, 34, 68);
    private Color ACCENT = Color.FromArgb(233, 69, 96);
    private Color PURPLE = Color.FromArgb(108, 92, 231);
    private Color GREEN = Color.FromArgb(0, 206, 158);

    public MusicApp()
    {
        this.Text = "ThaiPass Music App";
        this.Size = new Size(560, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.BackColor = BG;
        this.ForeColor = Color.White;
        this.Font = new Font("Segoe UI", 9);

        // ===== Header =====
        Panel header = new Panel();
        header.Dock = DockStyle.Top;
        header.Height = 96;
        header.BackColor = PURPLE;
        header.Padding = new Padding(16, 12, 16, 12);
        this.Controls.Add(header);

        Label logo = new Label();
        logo.Text = "🎵";
        logo.Font = new Font("Segoe UI", 28);
        logo.ForeColor = Color.White;
        logo.AutoSize = true;
        logo.Location = new Point(14, 14);
        header.Controls.Add(logo);

        Label t1 = new Label();
        t1.Text = "ThaiPass Music";
        t1.Font = new Font("Segoe UI", 18, FontStyle.Bold);
        t1.ForeColor = Color.White;
        t1.AutoSize = true;
        t1.Location = new Point(72, 10);
        header.Controls.Add(t1);

        Label t2 = new Label();
        t2.Text = "👤 ผู้ใช้ ThaiPass (Demo)  •  แอปเล่นเพลงบนคอม";
        t2.Font = new Font("Segoe UI", 9);
        t2.ForeColor = Color.FromArgb(230, 230, 255);
        t2.AutoSize = true;
        t2.Location = new Point(74, 48);
        header.Controls.Add(t2);

        // ===== Now Playing Card =====
        Panel card = new Panel();
        card.Location = new Point(14, 108);
        card.Size = new Size(518, 168);
        card.BackColor = CARD;
        this.Controls.Add(card);
        PaintCard(card);

        Label cover = new Label();
        cover.Text = "🎶";
        cover.Font = new Font("Segoe UI", 44);
        cover.TextAlign = ContentAlignment.MiddleCenter;
        cover.BackColor = Color.FromArgb(48, 48, 90);
        cover.ForeColor = Color.White;
        cover.Location = new Point(12, 12);
        cover.Size = new Size(110, 110);
        card.Controls.Add(cover);

        lblTitle = new Label();
        lblTitle.Text = "ยังไม่ได้เลือกเพลง";
        lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(134, 12);
        lblTitle.Size = new Size(370, 26);
        card.Controls.Add(lblTitle);

        lblArtist = new Label();
        lblArtist.Text = "กด + เพิ่มเพลง แล้วดับเบิลคลิกเพื่อเล่น";
        lblArtist.Font = new Font("Segoe UI", 9);
        lblArtist.ForeColor = Color.FromArgb(180, 180, 210);
        lblArtist.Location = new Point(134, 40);
        lblArtist.Size = new Size(370, 20);
        card.Controls.Add(lblArtist);

        progressBar = new TrackBar();
        progressBar.Location = new Point(134, 66);
        progressBar.Size = new Size(370, 30);
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
        lblTime.Location = new Point(134, 96);
        lblTime.Size = new Size(370, 18);
        card.Controls.Add(lblTime);

        // ปุ่ม Volume ใน card
        Label vLab = new Label();
        vLab.Text = "🔊";
        vLab.ForeColor = Color.White;
        vLab.AutoSize = true;
        vLab.Location = new Point(12, 132);
        card.Controls.Add(vLab);

        volBar = new TrackBar();
        volBar.Location = new Point(44, 128);
        volBar.Size = new Size(140, 28);
        volBar.Minimum = 0; volBar.Maximum = 100; volBar.Value = 80;
        volBar.TickStyle = TickStyle.None;
        volBar.BackColor = CARD;
        volBar.Scroll += delegate { SetVolume(volBar.Value); };
        card.Controls.Add(volBar);

        lblCount = new Label();
        lblCount.Text = "0 เพลง";
        lblCount.ForeColor = Color.FromArgb(160, 160, 200);
        lblCount.Font = new Font("Segoe UI", 8);
        lblCount.Location = new Point(400, 130);
        lblCount.Size = new Size(104, 18);
        lblCount.TextAlign = ContentAlignment.MiddleRight;
        card.Controls.Add(lblCount);

        // ===== Controls =====
        Panel ctrl = new Panel();
        ctrl.Location = new Point(14, 286);
        ctrl.Size = new Size(518, 64);
        ctrl.BackColor = BG;
        this.Controls.Add(ctrl);

        btnPrev = MkBtn(ctrl, "⏮", 0, Color.FromArgb(60, 60, 110), 78);
        btnPlay = MkBtn(ctrl, "▶", 86, GREEN, 110);
        btnPause = MkBtn(ctrl, "⏸", 204, Color.FromArgb(255, 165, 2), 78);
        btnStop = MkBtn(ctrl, "⏹", 290, Color.FromArgb(120, 120, 150), 78);
        btnNext = MkBtn(ctrl, "⏭", 376, Color.FromArgb(60, 60, 110), 78);

        btnPrev.Click += delegate { Move(-1); };
        btnNext.Click += delegate { Move(1); };
        btnPlay.Click += delegate { PlaySelected(); };
        btnPause.Click += delegate { Pause(); };
        btnStop.Click += delegate { StopAll(); };

        // ปุ่ม Play ใหญ่เด่น
        btnPlay.Font = new Font("Segoe UI", 14, FontStyle.Bold);

        // ===== Search + Add =====
        txtSearch = new TextBox();
        txtSearch.Location = new Point(14, 360);
        txtSearch.Size = new Size(250, 26);
        txtSearch.Font = new Font("Segoe UI", 10);
        txtSearch.BackColor = Color.FromArgb(48, 48, 90);
        txtSearch.ForeColor = Color.White;
        txtSearch.BorderStyle = BorderStyle.FixedSingle;
        txtSearch.Text = "🔍 ค้นหาเพลง...";
        txtSearch.GotFocus += delegate { if (txtSearch.Text.StartsWith("🔍")) txtSearch.Text = ""; };
        txtSearch.TextChanged += delegate { FilterList(); };
        this.Controls.Add(txtSearch);

        btnAdd = new Button();
        StyleBtn(btnAdd, "+ เพิ่มเพลง", PURPLE);
        btnAdd.Location = new Point(274, 358);
        btnAdd.Size = new Size(120, 30);
        btnAdd.Click += delegate { AddSongs(); };
        this.Controls.Add(btnAdd);

        btnRemove = new Button();
        StyleBtn(btnRemove, "ลบ", Color.FromArgb(90, 60, 70));
        btnRemove.Location = new Point(402, 358);
        btnRemove.Size = new Size(130, 30);
        btnRemove.Click += delegate { RemoveSelected(); };
        this.Controls.Add(btnRemove);

        // ===== Playlist =====
        playlist = new ListBox();
        playlist.Location = new Point(14, 396);
        playlist.Size = new Size(518, 220);
        playlist.BackColor = Color.FromArgb(30, 30, 55);
        playlist.ForeColor = Color.White;
        playlist.Font = new Font("Segoe UI", 10);
        playlist.BorderStyle = BorderStyle.None;
        playlist.DrawMode = DrawMode.OwnerDrawFixed;
        playlist.ItemHeight = 30;
        playlist.DrawItem += Playlist_Draw;
        playlist.DoubleClick += delegate { PlaySelected(); };
        this.Controls.Add(playlist);

        Label hint = new Label();
        hint.Text = "รองรับ .mp3 / .wav / .wma  •  ดับเบิลคลิกเพื่อเล่น  •  ลากแถบบนเพื่อกรอเพลง";
        hint.ForeColor = Color.FromArgb(130, 130, 170);
        hint.Font = new Font("Segoe UI", 8);
        hint.AutoSize = true;
        hint.Location = new Point(14, 622);
        this.Controls.Add(hint);

        timer.Interval = 400;
        timer.Tick += delegate { OnTick(); };
        timer.Start();
        this.FormClosing += delegate { StopMci(); };
    }

    private void PaintCard(Panel p)
    {
        p.Paint += delegate(object s, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle,
                Color.FromArgb(70, 70, 130), ButtonBorderStyle.Solid);
        };
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
        Color bg = sel ? PURPLE : (e.Index % 2 == 0 ? Color.FromArgb(30, 30, 55) : Color.FromArgb(36, 36, 66));
        if (isCur && !sel) bg = Color.FromArgb(60, 30, 60);
        using (SolidBrush br = new SolidBrush(bg))
            e.Graphics.FillRectangle(br, e.Bounds);
        string full = playlist.Items[e.Index].ToString();
        string name = Path.GetFileName(full);
        string mark = isCur ? "▶ " : "🎵 ";
        using (SolidBrush tb = new SolidBrush(Color.White))
            e.Graphics.DrawString(mark + name, e.Font, tb, e.Bounds.X + 8, e.Bounds.Y + 6);
        e.DrawFocusRectangle();
    }

    private void AddSongs()
    {
        using (OpenFileDialog od = new OpenFileDialog())
        {
            od.Filter = "เพลง (*.mp3;*.wav;*.wma)|*.mp3;*.wav;*.wma|ทั้งหมด|*.*";
            od.Multiselect = true;
            if (od.ShowDialog() == DialogResult.OK)
            {
                int c = 0;
                foreach (string f in od.FileNames)
                {
                    if (!allSongs.Contains(f)) { allSongs.Add(f); c++; }
                }
                FilterList();
                MessageBox.Show("เพิ่ม " + c + " เพลงแล้ว!", "สำเร็จ",
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
        foreach (string f in allSongs)
        {
            string n = Path.GetFileName(f).ToLower();
            if (q == "" || n.Contains(q)) playlist.Items.Add(f);
        }
        if (playlist.Items.Count > 0 && playlist.SelectedIndex < 0)
            playlist.SelectedIndex = 0;
        lblCount.Text = playlist.Items.Count + " เพลง";
        playlist.Invalidate();
    }

    private void PlaySelected()
    {
        if (playlist.SelectedIndex < 0)
        {
            if (playlist.Items.Count > 0) playlist.SelectedIndex = 0;
            else { MessageBox.Show("กด + เพิ่มเพลง ก่อนนะ"); return; }
        }
        PlayFile(playlist.SelectedItem.ToString());
    }

    private void PlayFile(string f)
    {
        if (!File.Exists(f)) { MessageBox.Show("ไม่พบไฟล์: " + f); return; }
        StopMci();
        currentFile = f;
        mciSendString("open \"" + f + "\" alias MySong", null, 0, IntPtr.Zero);
        mciSendString("play MySong", null, 0, IntPtr.Zero);
        SetVolume(volBar.Value);
        isPlaying = true;
        lblTitle.Text = Path.GetFileNameWithoutExtension(f);
        lblArtist.Text = "📁 " + Path.GetFileName(f);
        btnPlay.Text = "⏸ กำลังเล่น";
        playlist.Invalidate();
    }

    private void Pause()
    {
        if (currentFile == null) return;
        if (isPlaying)
        {
            mciSendString("pause MySong", null, 0, IntPtr.Zero);
            isPlaying = false;
            lblTitle.Text = "⏸ " + Path.GetFileNameWithoutExtension(currentFile);
            btnPlay.Text = "▶";
        }
        else
        {
            mciSendString("resume MySong", null, 0, IntPtr.Zero);
            isPlaying = true;
            lblTitle.Text = Path.GetFileNameWithoutExtension(currentFile);
            btnPlay.Text = "⏸ กำลังเล่น";
        }
    }

    private void StopAll()
    {
        StopMci();
        btnPlay.Text = "▶";
        lblTitle.Text = "หยุดแล้ว";
        progressBar.Value = 0;
        lblTime.Text = "0:00 / 0:00";
        playlist.Invalidate();
    }

    private void StopMci()
    {
        try { mciSendString("stop MySong", null, 0, IntPtr.Zero); } catch { }
        try { mciSendString("close MySong", null, 0, IntPtr.Zero); } catch { }
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
        allSongs.Remove(f);
        FilterList();
    }

    private void SetVolume(int v)
    {
        mciSendString("setaudio MySong volume to " + (v * 10), null, 0, IntPtr.Zero);
    }

    private int GetLen()
    {
        StringBuilder sb = new StringBuilder(64);
        mciSendString("status MySong length", sb, 64, IntPtr.Zero);
        int n = 0;
        int.TryParse(sb.ToString().Trim(), out n);
        return n;
    }

    private int GetPos()
    {
        StringBuilder sb = new StringBuilder(64);
        mciSendString("status MySong position", sb, 64, IntPtr.Zero);
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
        mciSendString("play MySong from " + pos, null, 0, IntPtr.Zero);
        if (!isPlaying) mciSendString("pause MySong", null, 0, IntPtr.Zero);
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
            // จบเพลง -> เล่นต่ออัตโนมัติ
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
        Application.Run(new MusicApp());
    }
}
