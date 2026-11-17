using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

static class Settings
{
    static string FilePath()
    {
        try
        {
            string dir = Path.GetDirectoryName(Application.ExecutablePath);
            if (File.Exists(Path.Combine(dir, "portable.txt"))) return Path.Combine(dir, "settings.ini");
        }
        catch { }
        string app = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZhengShangYou");
        try { if (!Directory.Exists(app)) Directory.CreateDirectory(app); } catch { }
        return Path.Combine(app, "settings.ini");
    }

    public static string LoadLang()
    {
        try
        {
            string p = FilePath();
            if (File.Exists(p))
            {
                foreach (string l in File.ReadAllLines(p))
                {
                    string t = l.Trim();
                    if (t.StartsWith("lang=")) return t.Substring(5).Trim();
                }
            }
        }
        catch { }
        return "zh-CN";
    }

    public static void SaveLang(string code)
    {
        try { File.WriteAllText(FilePath(), "lang=" + code + "\r\n"); } catch { }
    }
}

static class Ui
{
    static readonly StringFormat CenterLtr = MakeCenter(false);
    static readonly StringFormat CenterRtl = MakeCenter(true);
    static readonly StringFormat EllipsisLtr = MakeEllipsis(false);
    static readonly StringFormat EllipsisRtl = MakeEllipsis(true);
    static readonly StringFormat FarLtr = MakeFar(false);
    static readonly StringFormat FarRtl = MakeFar(true);

    public static StringFormat Center { get { return Lang.IsRtl ? CenterRtl : CenterLtr; } }
    public static StringFormat Ellipsis { get { return Lang.IsRtl ? EllipsisRtl : EllipsisLtr; } }
    public static StringFormat Far { get { return Lang.IsRtl ? FarRtl : FarLtr; } }
    public static StringFormat Near { get { return Lang.IsRtl ? FarLtr : FarLtr; } }

    static StringFormat MakeFar(bool rtl)
    {
        StringFormat f = new StringFormat();
        f.Alignment = rtl ? StringAlignment.Near : StringAlignment.Far;
        f.LineAlignment = StringAlignment.Near;
        if (rtl) f.FormatFlags = StringFormatFlags.DirectionRightToLeft;
        return f;
    }

    static StringFormat MakeCenter(bool rtl)
    {
        StringFormat f = new StringFormat();
        f.Alignment = StringAlignment.Center;
        f.LineAlignment = StringAlignment.Center;
        if (rtl) f.FormatFlags = StringFormatFlags.DirectionRightToLeft;
        return f;
    }

    static StringFormat MakeEllipsis(bool rtl)
    {
        StringFormat f = new StringFormat();
        f.Trimming = StringTrimming.EllipsisCharacter;
        f.FormatFlags = StringFormatFlags.NoWrap;
        if (rtl) f.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
        return f;
    }

    public static Font F(float size) { return new Font(Lang.FontFamily, size, FontStyle.Regular); }
    public static Font F(float size, FontStyle st) { return new Font(Lang.FontFamily, size, st); }

    public static Button Btn(string text, int x, int y, int w, int h, float fs)
    {
        Button b = new Button();
        b.Text = text;
        b.Font = F(fs, FontStyle.Bold);
        b.ForeColor = Color.FromArgb(255, 241, 208);
        b.BackColor = Color.FromArgb(23, 110, 73);
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderColor = Color.FromArgb(188, 162, 92);
        b.FlatAppearance.BorderSize = 1;
        b.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 142, 94);
        b.FlatAppearance.MouseDownBackColor = Color.FromArgb(16, 84, 56);
        b.UseVisualStyleBackColor = false;
        b.SetBounds(x, y, w, h);
        b.Cursor = Cursors.Hand;
        return b;
    }

    public static void Felt(Graphics g, int w, int h)
    {
        using (LinearGradientBrush bg = new LinearGradientBrush(new Rectangle(0, 0, w, h), Color.FromArgb(20, 102, 68), Color.FromArgb(4, 34, 22), 90f))
            g.FillRectangle(bg, 0, 0, w, h);
    }

    public static Icon AppIcon()
    {
        try { return Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
        catch { return null; }
    }

    // 自动缩字号，保证长文本（英语/俄语）不撑出画面
    public static Font Fit(Graphics g, string text, float size, FontStyle st, float maxW, float maxH, float minSize)
    {
        Font f = F(size, st);
        for (int i = 0; i < 40; i++)
        {
            SizeF m = g.MeasureString(text, f);
            if ((m.Width <= maxW && m.Height <= maxH) || size <= minSize) break;
            f.Dispose();
            size -= 1f;
            f = F(size, st);
        }
        return f;
    }
}

// ============ 语言选择 ============
class LangDialog : Form
{
    public bool Changed;

    public LangDialog()
    {
        Text = Lang.T("lang_title");
        ClientSize = new Size(520, 360);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(10, 58, 40);
        RightToLeft = RightToLeft.No;

        Label l1 = new Label();
        l1.Text = Lang.T("lang_title");
        l1.ForeColor = Color.FromArgb(255, 214, 120);
        l1.BackColor = Color.Transparent;
        l1.Font = Ui.F(16, FontStyle.Bold);
        l1.TextAlign = ContentAlignment.MiddleCenter;
        l1.SetBounds(20, 12, 480, 40);

        for (int i = 0; i < Lang.Names.Length; i++)
        {
            Button b = Ui.Btn(Lang.Names[i], 24 + (i % 2) * 240, 64 + (i / 2) * 66, 220, 54, 11);
            int idx = i;
            if (i == Lang.Idx)
            {
                b.BackColor = Color.FromArgb(40, 160, 106);
                b.FlatAppearance.BorderColor = Color.FromArgb(255, 214, 120);
            }
            b.Click += delegate
            {
                Lang.Idx = idx;
                Settings.SaveLang(Lang.Code);
                Changed = true;
                Close();
            };
            Controls.Add(b);
        }

        Controls.Add(l1);
    }
}

// ============ 主菜单 ============
class StartForm : Form
{
    const int W = 820, H = 620;
    Button btnStart, btnRules, btnExit, btnLang;

    public StartForm()
    {
        Text = Lang.T("game_title");
        Icon = Ui.AppIcon();
        ClientSize = new Size(W, H);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        DoubleBuffered = true;
        BackColor = Color.FromArgb(6, 45, 30);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

        btnStart = Ui.Btn(Lang.T("btn_start"), 260, 352, 300, 58, 16);
        btnStart.Click += OnStart;
        btnRules = Ui.Btn(Lang.T("btn_rules"), 260, 424, 300, 48, 12);
        btnRules.Click += OnRules;
        btnExit = Ui.Btn(Lang.T("btn_exit"), 260, 486, 300, 48, 12);
        btnExit.Click += delegate { Application.Exit(); };
        btnLang = Ui.Btn("Language", 12, 12, 128, 30, 9);
        btnLang.Click += OnLang;
        Controls.Add(btnStart);
        Controls.Add(btnRules);
        Controls.Add(btnExit);
        Controls.Add(btnLang);
    }

    void OnLang(object s, EventArgs e)
    {
        LangDialog d = new LangDialog();
        if (d.ShowDialog(this) == DialogResult.OK || d.Changed)
        {
            StartForm nf = new StartForm();
            nf.Show();
            this.Close();
        }
    }

    void OnStart(object s, EventArgs e)
    {
        MessageBox.Show(this, Lang.T("start_msg_body"), Lang.T("start_msg_title"),
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        Hide();
        GameForm gf = new GameForm();
        gf.ShowDialog();
        if (!IsDisposed) Show();
    }

    void OnRules(object s, EventArgs e)
    {
        RulesForm rf = new RulesForm();
        rf.ShowDialog(this);
        if (rf.Changed) this.Text = Lang.T("game_title");
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        Ui.Felt(g, W, H);

        GraphicsPath tp = new GraphicsPath();
        tp.AddEllipse(new Rectangle(40, 30, W - 80, H - 60));
        using (PathGradientBrush pb = new PathGradientBrush(tp))
        {
            pb.CenterColor = Color.FromArgb(26, 124, 82);
            pb.SurroundColors = new Color[] { Color.FromArgb(8, 52, 34) };
            g.FillPath(pb, tp);
        }
        using (Pen pn = new Pen(Color.FromArgb(70, 214, 180, 96), 2f)) g.DrawPath(pn, tp);
        tp.Dispose();

        // 标题（按语言自动缩字号）
        string title = Lang.T("game_title");
        Font ft = Ui.Fit(g, title, 50, FontStyle.Bold, 700, 92, 18);
        RectangleF tr = new RectangleF(0, 52, W, 92);
        g.DrawString(title, ft, new SolidBrush(Color.FromArgb(0, 0, 0)), new RectangleF(3, 56, W, 92), Ui.Center);
        g.DrawString(title, ft, new SolidBrush(Color.FromArgb(255, 214, 122)), tr, Ui.Center);
        ft.Dispose();

        string sub = Lang.T("subtitle");
        Font fs = Ui.Fit(g, sub, 12, FontStyle.Regular, 700, 30, 8);
        g.DrawString(sub, fs, new SolidBrush(Color.FromArgb(206, 234, 216)), new RectangleF(0, 148, W, 30), Ui.Center);
        fs.Dispose();

        // 三张装饰牌（扇形）
        DrawFan(g, new Card(5, 1), 340, 250, -14f);
        DrawFan(g, new Card(16, 4), 410, 238, 0f);
        DrawFan(g, new Card(8, 0), 480, 250, 14f);

        string line = Lang.T("rank_line");
        Font fv = Ui.Fit(g, line, 9, FontStyle.Regular, 760, 24, 6.5f);
        g.DrawString(line, fv, new SolidBrush(Color.FromArgb(150, 205, 180)), new RectangleF(0, 576, W, 24), Ui.Center);
        fv.Dispose();
    }

    void DrawFan(Graphics g, Card c, float cx, float cy, float angle)
    {
        GraphicsState st = g.Save();
        g.TranslateTransform(cx, cy);
        g.RotateTransform(angle);
        CardPainter.Draw(g, new RectangleF(-38f, -53f, 76f, 106f), c, false);
        g.Restore(st);
    }
}

// ============ 规则说明 ============
class RulesForm : Form
{
    TextBox tb;
    public bool Changed;

    public RulesForm()
    {
        Text = Lang.T("rules_title");
        ClientSize = new Size(920, 720);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(10, 58, 40);

        tb = new TextBox();
        tb.Multiline = true;
        tb.ReadOnly = true;
        tb.ScrollBars = ScrollBars.Vertical;
        tb.BorderStyle = BorderStyle.None;
        tb.BackColor = Color.FromArgb(8, 46, 32);
        tb.ForeColor = Color.FromArgb(228, 242, 234);
        tb.Font = Ui.F(11);
        tb.RightToLeft = Lang.IsRtl ? RightToLeft.Yes : RightToLeft.No;
        tb.SetBounds(28, 22, 862, 592);
        tb.Text = Lang.T("rules_body");

        Button b = Ui.Btn(Lang.T("btn_close"), 380, 636, 160, 44, 12);
        b.DialogResult = DialogResult.OK;

        Button bl = Ui.Btn("Language", 28, 636, 150, 44, 10);
        bl.Click += delegate
        {
            LangDialog d = new LangDialog();
            d.ShowDialog(this);
            if (d.Changed)
            {
                Changed = true;
                Text = Lang.T("rules_title");
                tb.Text = Lang.T("rules_body");
                tb.RightToLeft = Lang.IsRtl ? RightToLeft.Yes : RightToLeft.No;
                tb.Font = Ui.F(11);
                b.Text = Lang.T("btn_close");
                b.Font = Ui.F(12, FontStyle.Bold);
            }
        };

        Controls.Add(tb);
        Controls.Add(b);
        Controls.Add(bl);
        AcceptButton = b;
    }
}

// ============ 通用提示窗 ============
class NoticeForm : Form
{
    public NoticeForm(string title, string body, string btnText)
    {
        Text = title;
        ClientSize = new Size(580, 480);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = false;
        BackColor = Color.FromArgb(10, 58, 40);
        RightToLeft = RightToLeft.No;

        Label l1 = new Label();
        l1.Text = title;
        l1.ForeColor = Color.FromArgb(255, 214, 120);
        l1.BackColor = Color.Transparent;
        l1.Font = Ui.F(19, FontStyle.Bold);
        l1.TextAlign = ContentAlignment.MiddleCenter;
        l1.RightToLeft = Lang.IsRtl ? RightToLeft.Yes : RightToLeft.No;
        l1.SetBounds(20, 18, 540, 50);

        Label l2 = new Label();
        l2.Text = body;
        l2.ForeColor = Color.FromArgb(232, 246, 238);
        l2.BackColor = Color.Transparent;
        l2.Font = Ui.F(10.5f);
        l2.TextAlign = ContentAlignment.MiddleCenter;
        l2.RightToLeft = Lang.IsRtl ? RightToLeft.Yes : RightToLeft.No;
        l2.SetBounds(26, 74, 528, 306);

        Button b = Ui.Btn(btnText, 200, 396, 180, 46, 12);
        b.DialogResult = DialogResult.OK;

        Controls.Add(l1);
        Controls.Add(l2);
        Controls.Add(b);
        AcceptButton = b;
    }
}

// ============ 要牌选择窗 ============
class ChooseRankDialog : Form
{
    public int SelectedRank = -1;

    public ChooseRankDialog(string giverName, List<int> cands, int refusals, List<Card> myHand)
    {
        Text = Lang.T("ask_title");
        ClientSize = new Size(520, 420);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(10, 58, 40);
        RightToLeft = RightToLeft.No;

        Label l1 = new Label();
        l1.Text = Lang.F("ask_head", giverName);
        l1.ForeColor = Color.FromArgb(255, 214, 120);
        l1.BackColor = Color.Transparent;
        l1.Font = Ui.F(13, FontStyle.Bold);
        l1.TextAlign = ContentAlignment.MiddleCenter;
        l1.RightToLeft = Lang.IsRtl ? RightToLeft.Yes : RightToLeft.No;
        l1.SetBounds(20, 14, 480, 56);

        Label l2 = new Label();
        l2.Text = Lang.F("ask_refusals", refusals);
        l2.ForeColor = Color.FromArgb(200, 226, 212);
        l2.BackColor = Color.Transparent;
        l2.Font = Ui.F(9);
        l2.TextAlign = ContentAlignment.MiddleCenter;
        l2.RightToLeft = Lang.IsRtl ? RightToLeft.Yes : RightToLeft.No;
        l2.SetBounds(20, 70, 480, 24);

        FlowLayoutPanel fp = new FlowLayoutPanel();
        fp.SetBounds(26, 102, 468, 210);
        fp.BackColor = Color.FromArgb(8, 46, 32);
        fp.AutoScroll = true;
        fp.Padding = new Padding(6);
        fp.FlowDirection = FlowDirection.LeftToRight;

        foreach (int r in cands)
        {
            int have = 0;
            if (myHand != null) foreach (Card c in myHand) if (c.Rank == r) have++;
            string cap = (have >= 2) ? Lang.T("ask_triple") : Lang.T("ask_pair");
            Button b = Ui.Btn(Card.RankTextOf(r) + "\r\n" + cap, 0, 0, 92, 56, 10);
            int rank = r;
            b.Margin = new Padding(6);
            b.Click += delegate
            {
                SelectedRank = rank;
                DialogResult = DialogResult.OK;
                Close();
            };
            fp.Controls.Add(b);
        }

        Button cancel = Ui.Btn(Lang.T("ask_cancel"), 170, 330, 180, 44, 11);
        cancel.DialogResult = DialogResult.Cancel;

        Controls.Add(l1);
        Controls.Add(l2);
        Controls.Add(fp);
        Controls.Add(cancel);
    }
}

// ============ 是否给牌窗 ============
class GiveDialog : Form
{
    public GiveDialog(string title, string sub)
    {
        Text = Lang.T("give_title");
        ClientSize = new Size(540, 250);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(10, 58, 40);
        RightToLeft = RightToLeft.No;

        Label l1 = new Label();
        l1.Text = title;
        l1.ForeColor = Color.FromArgb(255, 214, 120);
        l1.BackColor = Color.Transparent;
        l1.Font = Ui.F(13, FontStyle.Bold);
        l1.TextAlign = ContentAlignment.MiddleCenter;
        l1.RightToLeft = Lang.IsRtl ? RightToLeft.Yes : RightToLeft.No;
        l1.SetBounds(20, 22, 500, 60);

        Label l2 = new Label();
        l2.Text = sub;
        l2.ForeColor = Color.FromArgb(202, 228, 214);
        l2.BackColor = Color.Transparent;
        l2.Font = Ui.F(9);
        l2.TextAlign = ContentAlignment.MiddleCenter;
        l2.RightToLeft = Lang.IsRtl ? RightToLeft.Yes : RightToLeft.No;
        l2.SetBounds(20, 92, 500, 40);

        Button yes = Ui.Btn(Lang.T("give_yes"), 70, 160, 180, 50, 12);
        yes.DialogResult = DialogResult.OK;
        Button no = Ui.Btn(Lang.T("give_no"), 290, 160, 180, 50, 12);
        no.DialogResult = DialogResult.Cancel;

        Controls.Add(l1);
        Controls.Add(l2);
        Controls.Add(yes);
        Controls.Add(no);
        AcceptButton = yes;
        CancelButton = no;
    }
}
