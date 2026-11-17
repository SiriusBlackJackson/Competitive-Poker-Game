using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

static class Program
{
    static List<Card> gfHandForShot()
    {
        List<Card> h = new List<Card>();
        h.Add(new Card(8, 1));
        h.Add(new Card(12, 0));
        h.Add(new Card(12, 2));
        h.Add(new Card(5, 1));
        return h;
    }

    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        string langArg = null;
        foreach (string a in args) if (a != null && a.StartsWith("-lang=")) langArg = a.Substring(6);
        if (langArg != null) Settings.SaveLang(langArg);
        Lang.SetCode(langArg != null ? langArg : Settings.LoadLang());

        if (args.Length > 0 && args[0] == "-preview")
        {
            CardPainter.RenderPreview();
            return;
        }
        if (args.Length > 0 && args[0] == "-selftest")
        {
            GameForm gf = new GameForm();
            gf.RunSilentTest();
            return;
        }
        if (args.Length > 0 && args[0] == "-snapshot")
        {
            StartForm sf = new StartForm();
            sf.Show();
            Application.DoEvents();
            Bitmap b1 = new Bitmap(sf.Width, sf.Height);
            sf.DrawToBitmap(b1, new Rectangle(0, 0, sf.Width, sf.Height));
            b1.Save("start.png", System.Drawing.Imaging.ImageFormat.Png);
            b1.Dispose();
            sf.Hide();

            GameForm gf = new GameForm();
            gf.PrepareSnapshot();
            gf.Show();
            Application.DoEvents();
            Bitmap b2 = new Bitmap(gf.Width, gf.Height);
            gf.DrawToBitmap(b2, new Rectangle(0, 0, gf.Width, gf.Height));
            b2.Save("game.png", System.Drawing.Imaging.ImageFormat.Png);
            b2.Dispose();
            gf.Close();

            NoticeForm nf = new NoticeForm(Lang.F("notice_trade", 2),
                Lang.F("tb_give1", Lang.T("name_bot2"), "5♥", Lang.T("name_you")) + "\r\n" +
                Lang.F("tb_give2", Lang.T("name_you"), Lang.T("rank_big_joker"), Lang.T("name_bot1")) + "\r\n" +
                Lang.F("log_ask", Lang.T("name_you"), Lang.T("name_bot1")) + "\r\n" +
                Lang.F("log_want_absent", Lang.T("name_you"), "8", Lang.T("name_bot1")) + "\r\n" +
                Lang.F("log_gave", Lang.T("name_bot1"), Lang.T("name_you"), "8") + "\r\n" +
                Lang.F("log_ask", Lang.T("name_bot2"), Lang.T("name_you")) + "\r\n" +
                Lang.F("log_gave", Lang.T("name_you"), Lang.T("name_bot2"), "Q♣") + "\r\n\r\n" +
                Lang.F("trade_hands", Lang.T("name_you"), 18, Lang.T("name_bot1"), 18, Lang.T("name_bot2"), 18),
                Lang.T("btn_start_play"));
            nf.Show();
            Application.DoEvents();
            Bitmap b3 = new Bitmap(nf.Width, nf.Height);
            nf.DrawToBitmap(b3, new Rectangle(0, 0, nf.Width, nf.Height));
            b3.Save("dialog.png", System.Drawing.Imaging.ImageFormat.Png);
            b3.Dispose();
            nf.Close();

            List<int> cands = new List<int>(new int[] { 14, 13, 12, 8, 5 });
            ChooseRankDialog cd = new ChooseRankDialog(Lang.T("name_bot1"), cands, 1, gfHandForShot());
            cd.Show();
            Application.DoEvents();
            Bitmap b4 = new Bitmap(cd.Width, cd.Height);
            cd.DrawToBitmap(b4, new Rectangle(0, 0, cd.Width, cd.Height));
            b4.Save("rank.png", System.Drawing.Imaging.ImageFormat.Png);
            b4.Dispose();
            cd.Close();

            RulesForm rf = new RulesForm();
            rf.Show();
            Application.DoEvents();
            Bitmap b5 = new Bitmap(rf.Width, rf.Height);
            rf.DrawToBitmap(b5, new Rectangle(0, 0, rf.Width, rf.Height));
            b5.Save("rules.png", System.Drawing.Imaging.ImageFormat.Png);
            b5.Dispose();
            rf.Close();

            LangDialog ld = new LangDialog();
            ld.Show();
            Application.DoEvents();
            Bitmap b6 = new Bitmap(ld.Width, ld.Height);
            ld.DrawToBitmap(b6, new Rectangle(0, 0, ld.Width, ld.Height));
            b6.Save("lang.png", System.Drawing.Imaging.ImageFormat.Png);
            b6.Dispose();
            ld.Close();

            sf.Dispose();
            return;
        }
        Application.Run(new StartForm());
    }
}

class GameForm : Form
{
    const int PW = 1280;
    const int PH = 800;

    string[] names { get { return new string[] { Lang.T("name_you"), Lang.T("name_bot1"), Lang.T("name_bot2") }; } }
    List<Card>[] hands = new List<Card>[3];
    int roundNo;
    int[] prevRoles = new int[] { -1, -1, -1 };
    List<int[]> roundRanks = new List<int[]>();
    int[] finishOrder = new int[3];
    int finishCount;

    int current;
    int lastPlayer = -1;
    Play lastPlay;
    int passCount;
    Play centerPlay;
    int centerSeat = -1;
    string[] lastAction = new string[3];
    bool roundFirstPlay = true;
    bool roundOver;

    List<Card> selected = new List<Card>();
    List<RectangleF> handRects = new List<RectangleF>();
    List<string> logLines = new List<string>();
    string tipText = "";
    int remainSec = 30;

    Timer secTimer, botTimer, tipTimer;
    Button btnPlay, btnPass, btnRules, btnQuit, btnLang;
    Random rnd = new Random();

    public bool Silent;
    public bool SilentFinished;
    public StringBuilder Notes = new StringBuilder();

    public GameForm()
    {
        Text = Lang.T("win_title");
        Icon = Ui.AppIcon();
        ClientSize = new Size(PW, PH);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        DoubleBuffered = true;
        BackColor = Color.FromArgb(6, 45, 30);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

        btnPlay = Ui.Btn(Lang.T("btn_play"), 1046, 626, 108, 46, 11);
        btnPlay.Click += OnPlayClick;
        btnPass = Ui.Btn(Lang.T("btn_pass"), 1164, 626, 108, 46, 11);
        btnPass.Click += OnPassClick;
        btnRules = Ui.Btn(Lang.T("btn_rules"), 1046, 684, 108, 34, 9);
        btnRules.Click += OnRulesClick;
        btnQuit = Ui.Btn(Lang.T("btn_quit_round"), 1164, 684, 108, 34, 9);
        btnQuit.Click += OnQuitClick;
        btnLang = Ui.Btn("Language", 10, 8, 96, 26, 8);
        btnLang.Click += OnLangClick;
        Controls.Add(btnPlay);
        Controls.Add(btnPass);
        Controls.Add(btnRules);
        Controls.Add(btnQuit);
        Controls.Add(btnLang);

        secTimer = new Timer();
        secTimer.Interval = 1000;
        secTimer.Tick += SecTick;
        botTimer = new Timer();
        botTimer.Interval = 1100;
        botTimer.Tick += BotTick;
        tipTimer = new Timer();
        tipTimer.Interval = 2700;
        tipTimer.Tick += TipTick;

        for (int i = 0; i < 3; i++) hands[i] = new List<Card>();
        Shown += delegate { if (!Silent) StartGame(); };
    }

    // 生成一张界面截图用（不进入真实对局）
    public void PrepareSnapshot()
    {
        Silent = true;
        roundNo = 2;
        prevRoles = new int[] { 1, 0, 2 };
        Deal();
        current = 0;
        remainSec = 24;
        roundOver = false;
        roundFirstPlay = false;
        selected.Clear();

        Play sample = null;
        foreach (IGrouping<int, Card> g in hands[1].GroupBy(c => c.Rank))
        {
            List<Card> cs = g.ToList();
            if (cs.Count >= 2 && !cs[0].IsJoker)
            {
                sample = new Play(PlayType.Pair, new List<Card>(new Card[] { cs[0], cs[1] }), g.Key);
                break;
            }
        }
        if (sample == null && hands[1].Count > 0)
            sample = new Play(PlayType.Single, new List<Card>(new Card[] { hands[1][0] }), hands[1][0].Rank);
        if (sample != null)
        {
            centerPlay = sample;
            centerSeat = 1;
            lastPlay = sample;
            lastPlayer = 1;
            lastAction[1] = "play";
        }

        logLines.Add(Lang.F("log_trade_header", 2));
        logLines.Add(Lang.F("log_tribute1", Lang.T("name_bot2"), "5♥", Lang.T("name_you")));
        logLines.Add(Lang.F("log_tribute2", Lang.T("name_you"), Lang.T("rank_big_joker"), Lang.T("name_bot1")));
        logLines.Add(Lang.F("log_gave", Lang.T("name_bot1"), Lang.T("name_you"), "8"));
        logLines.Add(Lang.F("log_gave", Lang.T("name_you"), Lang.T("name_bot2"), "Q♣"));
        logLines.Add(Lang.F("log_play", Lang.T("name_bot1"), sample != null ? sample.TypeName : "", sample != null ? sample.Text : ""));
        if (hands[0].Count > 4) selected.Add(hands[0][4]);
        UpdateUI();
        Invalidate();
    }

    // ================= 流程 =================
    void StartGame()
    {
        roundNo = 0;
        prevRoles = new int[] { -1, -1, -1 };
        roundRanks.Clear();
        logLines.Clear();
        NextRound();
    }

    void NextRound()
    {
        roundNo++;
        roundOver = false;
        finishCount = 0;
        lastPlay = null;
        lastPlayer = -1;
        passCount = 0;
        selected.Clear();
        centerPlay = null;
        centerSeat = -1;
        for (int i = 0; i < 3; i++) lastAction[i] = null;
        logLines.Clear();

        Deal();

        int leader = (roundNo == 1) ? FindHeart4Seat() : prevRoles[0];
        if (leader < 0) leader = 0;
        current = leader;
        roundFirstPlay = true;
        remainSec = 30;

        UpdateUI();

        if (roundNo > 1)
        {
            AddLog(Lang.F("log_trade_header", roundNo));
            RunTrades();
        }
        else
        {
            AddLog(Lang.F("log_h4_leader", names[leader]));
        }

        selected.Clear();
        UpdateUI();
        StartTurn();
    }

    void Deal()
    {
        List<Card> deck = Card.BuildDeck();
        Card.Shuffle(deck, rnd);
        for (int i = 0; i < 3; i++)
        {
            hands[i] = new List<Card>();
            for (int k = 0; k < 18; k++) hands[i].Add(deck[i * 18 + k]);
            hands[i].Sort(new Comparison<Card>(Card.CompareDesc));
        }
    }

    int FindHeart4Seat()
    {
        for (int i = 0; i < 3; i++)
            foreach (Card c in hands[i])
                if (c.Rank == 4 && c.Suit == 1) return i;
        return 0;
    }

    void StartTurn()
    {
        UpdateUI();
        if (roundOver) return;
        if (Silent) return;
        if (current == 0)
        {
            remainSec = 30;
            btnPlay.Enabled = true;
            btnPass.Enabled = (lastPlay != null && lastPlayer != 0);
            secTimer.Stop();
            secTimer.Start();
            if (roundFirstPlay && roundNo == 1) Tip(Lang.T("tip_h4_first"));
        }
        else
        {
            btnPlay.Enabled = false;
            btnPass.Enabled = false;
            secTimer.Stop();
            botTimer.Stop();
            botTimer.Start();
        }
        Invalidate();
    }

    void UpdateUI()
    {
        bool my = (current == 0) && !roundOver;
        btnPlay.Enabled = my && (hands[0].Count > 0);
        btnPass.Enabled = my && lastPlay != null && lastPlayer != 0;
        Invalidate();
    }

    // ================= 计时 =================
    void SecTick(object s, EventArgs e)
    {
        if (roundOver || current != 0)
        {
            secTimer.Stop();
            return;
        }
        remainSec--;
        if (remainSec <= 0)
        {
            secTimer.Stop();
            Tip(Lang.T("tip_timeout"));
            AutoHuman();
            return;
        }
        Invalidate();
    }

    void BotTick(object s, EventArgs e)
    {
        botTimer.Stop();
        if (roundOver || current == 0) return;
        int seat = current;
        Play choice;
        bool mustH4 = roundFirstPlay && roundNo == 1 && seat == current;
        if (lastPlay == null) choice = ChooseLead(hands[seat], mustH4);
        else choice = ChooseFollow(hands[seat], lastPlay);
        if (choice == null) DoPass(seat);
        else ApplyPlay(seat, choice);
    }

    void TipTick(object s, EventArgs e)
    {
        tipTimer.Stop();
        tipText = "";
        Invalidate();
    }

    void Tip(string t)
    {
        tipText = t;
        tipTimer.Stop();
        tipTimer.Start();
        Invalidate();
    }

    // ================= 玩家操作 =================
    void OnPlayClick(object s, EventArgs e)
    {
        if (roundOver || current != 0) return;
        if (selected.Count == 0) { Tip(Lang.T("tip_select")); return; }

        Play p = PlayRules.Parse(selected, hands[0].Count);
        if (p == null) { Tip(Lang.T("tip_bad_type")); return; }
        if (lastPlay != null && lastPlayer != 0 && !PlayRules.Beats(p, lastPlay))
        {
            Tip(Lang.T("tip_must_beat"));
            return;
        }
        if (roundFirstPlay && roundNo == 1)
        {
            bool has = false;
            foreach (Card c in selected) if (c.Rank == 4 && c.Suit == 1) has = true;
            if (!has) { Tip(Lang.T("tip_need_h4")); return; }
        }
        ApplyPlay(0, p);
    }

    void OnPassClick(object s, EventArgs e)
    {
        if (roundOver || current != 0) return;
        if (lastPlay == null) { Tip(Lang.T("tip_cant_pass")); return; }
        DoPass(0);
    }

    void OnRulesClick(object s, EventArgs e)
    {
        RulesForm rf = new RulesForm();
        rf.ShowDialog(this);
        if (rf.Changed) ApplyLang();
    }

    void OnLangClick(object s, EventArgs e)
    {
        LangDialog d = new LangDialog();
        d.ShowDialog(this);
        if (d.Changed) ApplyLang();
    }

    void ApplyLang()
    {
        Text = Lang.T("win_title");
        btnPlay.Text = Lang.T("btn_play");
        btnPass.Text = Lang.T("btn_pass");
        btnRules.Text = Lang.T("btn_rules");
        btnQuit.Text = Lang.T("btn_quit_round");
        btnPlay.Font = Ui.F(11, FontStyle.Bold);
        btnPass.Font = Ui.F(11, FontStyle.Bold);
        btnRules.Font = Ui.F(9, FontStyle.Bold);
        btnQuit.Font = Ui.F(9, FontStyle.Bold);
        btnLang.Font = Ui.F(8, FontStyle.Bold);
        tipText = "";
        UpdateUI();
        Invalidate();
    }

    void OnQuitClick(object s, EventArgs e)
    {
        if (MessageBox.Show(this, Lang.T("quit_confirm_msg"), Lang.T("quit_confirm_title"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            Close();
    }

    void AutoHuman()
    {
        if (roundOver || current != 0) return;
        if (lastPlay == null)
        {
            Play p = ChooseLead(hands[0], roundFirstPlay && roundNo == 1);
            if (p != null) ApplyPlay(0, p);
            return;
        }
        DoPass(0);
    }

    // ================= 出牌 =================
    void ApplyPlay(int seat, Play p)
    {
        if (p == null) return;
        secTimer.Stop();
        botTimer.Stop();

        if (Silent && roundFirstPlay && roundNo == 1)
        {
            bool hasH4 = false;
            foreach (Card c in p.Cards) if (c.Rank == 4 && c.Suit == 1) hasH4 = true;
            AddLog(hasH4 ? "CHECK·首局首手含红桃4 √" : "CHECK·首局首手未含红桃4 ×");
            Notes.AppendLine(hasH4 ? "CHECK·首局首手含红桃4 √" : "CHECK·首局首手未含红桃4 ×");
        }

        foreach (Card c in p.Cards) hands[seat].Remove(c);
        if (seat == 0) selected.Clear();
        centerPlay = p;
        centerSeat = seat;
        lastAction[seat] = "play";
        lastPlay = p;
        lastPlayer = seat;
        passCount = 0;
        roundFirstPlay = false;
        AddLog(Lang.F("log_play", names[seat], p.TypeName, p.Text));

        int next = NextActive(seat);
        bool empty = (hands[seat].Count == 0);
        if (empty)
        {
            finishOrder[finishCount++] = seat;
            lastAction[seat] = "done";
            lastPlay = null;
            lastPlayer = -1;
            passCount = 0;
            AddLog(Lang.F("log_finish", names[seat], finishCount == 1 ? Lang.T("finish_up") : Lang.T("finish_mid")));
            if (finishCount == 2)
            {
                EndRound();
                return;
            }
        }
        current = next;
        UpdateUI();
        StartTurn();
    }

    void DoPass(int seat)
    {
        passCount++;
        lastAction[seat] = "pass";
        AddLog(Lang.F("log_pass", names[seat]));
        int active = ActiveCount();
        if (passCount >= active - 1)
        {
            lastPlay = null;
            lastPlayer = -1;
            passCount = 0;
        }
        current = NextActive(seat);
        UpdateUI();
        StartTurn();
    }

    Play ChooseLead(List<Card> hand, bool mustH4)
    {
        List<Play> plays = PlayRules.AllPlays(hand);
        if (mustH4)
        {
            List<Play> keep = new List<Play>();
            foreach (Play p in plays)
            {
                bool has = false;
                foreach (Card c in p.Cards) if (c.Rank == 4 && c.Suit == 1) has = true;
                if (has) keep.Add(p);
            }
            plays = keep;
        }
        if (plays.Count == 0) return null;
        plays.Sort(delegate(Play a, Play b)
        {
            int cmp = Card.GameValue(a.Key).CompareTo(Card.GameValue(b.Key));
            if (cmp != 0) return cmp;
            return b.Cards.Count.CompareTo(a.Cards.Count);
        });
        return plays[0];
    }

    Play ChooseFollow(List<Card> hand, Play last)
    {
        List<Play> cands = PlayRules.AllPlays(hand).Where(p => p.Type == last.Type && PlayRules.Beats(p, last)).ToList();
        if (cands.Count == 0) return null;
        cands.Sort(delegate(Play a, Play b)
        {
            int cmp = Card.GameValue(a.Key).CompareTo(Card.GameValue(b.Key));
            if (cmp != 0) return cmp;
            return a.Cards.Count.CompareTo(b.Cards.Count);
        });
        return cands[0];
    }

    int NextActive(int from)
    {
        for (int k = 1; k <= 3; k++)
        {
            int s = (from + k) % 3;
            if (hands[s].Count > 0) return s;
        }
        return from;
    }

    int ActiveCount()
    {
        int n = 0;
        for (int i = 0; i < 3; i++) if (hands[i].Count > 0) n++;
        return n;
    }

    // ================= 一局结束 =================
    void EndRound()
    {
        roundOver = true;
        secTimer.Stop();
        botTimer.Stop();
        btnPlay.Enabled = false;
        btnPass.Enabled = false;

        int remaining = -1;
        for (int i = 0; i < 3; i++) if (hands[i].Count > 0) remaining = i;
        int[] ranks = new int[] { finishOrder[0], finishOrder[1], remaining };
        prevRoles = new int[] { ranks[0], ranks[1], ranks[2] };
        roundRanks.Add(ranks);
        AddLog(Lang.F("log_round_result", roundNo, names[ranks[0]], names[ranks[1]], names[ranks[2]]));
        Invalidate();

        StringBuilder body = new StringBuilder();
        body.AppendLine(Lang.F("notice_round_rank", roundNo));
        body.AppendLine();
        body.AppendLine(Lang.F("lbl_up", names[ranks[0]]));
        body.AppendLine(Lang.F("lbl_mid", names[ranks[1]]));
        body.AppendLine(Lang.F("lbl_down", names[ranks[2]]));
        body.AppendLine();
        if (roundNo >= 5) body.AppendLine(Lang.T("msg_all_done"));
        else body.AppendLine(Lang.T("msg_next_hint"));

        bool last = roundNo >= 5;
        if (Silent)
        {
            if (last) { SilentFinished = true; return; }
            NextRound();
            return;
        }
        NoticeForm nf = new NoticeForm(Lang.F("notice_round_end", roundNo), body.ToString(), last ? Lang.T("btn_total") : Lang.T("btn_next"));
        nf.ShowDialog(this);

        if (last)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < roundRanks.Count; i++)
            {
                int[] rk = roundRanks[i];
                sb.AppendLine(Lang.F("total_line", i + 1, names[rk[0]], names[rk[1]], names[rk[2]]));
            }
            int[] cnt = new int[3];
            foreach (int[] rk in roundRanks) cnt[rk[0]]++;
            sb.AppendLine();
            sb.AppendLine(Lang.F("total_wins", names[0], cnt[0], names[1], cnt[1], names[2], cnt[2]));
            NoticeForm ff = new NoticeForm(Lang.T("total_title"), sb.ToString(), Lang.T("btn_menu"));
            ff.ShowDialog(this);
            Close();
            return;
        }
        NextRound();
    }

    // ================= 换牌 =================
    void RunTrades()
    {
        int startIdx = logLines.Count;
        int up = prevRoles[0];
        int mid = prevRoles[1];
        int down = prevRoles[2];
        if (up < 0 || mid < 0 || down < 0) return;

        // ===== 第一步：上贡（必须先完成交牌，才进入要牌） =====
        // 两张牌都按"上贡前的手牌"取，避免中游把刚收到的牌再转手贡出去
        Card c1 = BiggestCard(hands[down]);
        Card c2 = BiggestCard(hands[mid]);

        if (c1 != null)
        {
            MoveCard(hands[down], hands[mid], c1);
            AddLog(Lang.F("log_tribute1", names[down], c1.Display, names[mid]));
        }
        if (c2 != null)
        {
            MoveCard(hands[mid], hands[up], c2);
            AddLog(Lang.F("log_tribute2", names[mid], c2.Display, names[up]));
        }

        selected.Clear();
        UpdateUI();
        Invalidate();

        if (Silent)
        {
            Notes.AppendLine("上贡：" + (c1 != null ? names[down] + "→" + names[mid] + " " + c1.Display : "无")
                + "；" + (c2 != null ? names[mid] + "→" + names[up] + " " + c2.Display : "无"));
        }
        else
        {
            StringBuilder tb = new StringBuilder();
            tb.AppendLine(Lang.T("tb_step1"));
            tb.AppendLine();
            if (c1 != null) tb.AppendLine(Lang.F("tb_give1", names[down], c1.Display, names[mid]));
            if (c2 != null) tb.AppendLine(Lang.F("tb_give2", names[mid], c2.Display, names[up]));
            tb.AppendLine();
            tb.AppendLine(Lang.T("tb_step2"));
            tb.AppendLine(Lang.F("tb_ask1", names[mid], names[up]));
            tb.AppendLine(Lang.F("tb_ask2", names[down], names[mid]));
            tb.AppendLine(Lang.T("tb_note"));
            NoticeForm tn = new NoticeForm(Lang.F("notice_tribute", roundNo), tb.ToString(), Lang.T("btn_enter_ask"));
            tn.ShowDialog(this);
        }

        // ===== 第二步：要牌（禁止索回刚上贡的牌 / 对方手里最大的牌） =====
        int forbidMid = (c1 != null) ? c1.Rank : -1;
        int forbidUp = (c2 != null) ? c2.Rank : -1;

        AddLog(Lang.T("log_step2"));
        AddLog(Lang.F("log_ask", names[mid], names[up]));
        RequestStep(mid, up, forbidUp);
        AddLog(Lang.F("log_ask", names[down], names[mid]));
        RequestStep(down, mid, forbidMid);

        selected.Clear();
        for (int i = 0; i < 3; i++) hands[i].Sort(new Comparison<Card>(Card.CompareDesc));
        UpdateUI();
        Invalidate();
        if (Silent)
        {
            int tot = TotalCards();
            Notes.AppendLine("换牌后 手牌 " + hands[0].Count + "/" + hands[1].Count + "/" + hands[2].Count + " 合计 " + tot + (tot == 54 ? " √" : " ×"));
            return;
        }

        StringBuilder sb = new StringBuilder();
        for (int i = startIdx; i < logLines.Count; i++)
            sb.AppendLine(logLines[i]);
        sb.AppendLine();
        sb.AppendLine(Lang.F("trade_hands", names[0], hands[0].Count, names[1], hands[1].Count, names[2], hands[2].Count));
        NoticeForm nf = new NoticeForm(Lang.F("notice_trade", roundNo), sb.ToString(), Lang.T("btn_start_play"));
        nf.ShowDialog(this);
    }

    Card BiggestCard(List<Card> hand)
    {
        Card best = null;
        foreach (Card c in hand)
        {
            if (best == null) best = c;
            else if (c.Value > best.Value) best = c;
        }
        return best;
    }

    void RequestStep(int asker, int giver, int forbidRank)
    {
        secTimer.Stop();
        botTimer.Stop();
        int refusals = 0;
        List<int> asked = new List<int>();

        while (refusals < 3)
        {
            List<int> cands = RequestCandidates(hands[asker], hands[giver], forbidRank);
            cands.RemoveAll(delegate(int r) { return asked.Contains(r); });
            if (cands.Count == 0)
            {
                AddLog(Lang.F("log_no_idea", names[asker]));
                return;
            }

            int want = -1;
            if (asker == 0 && !Silent)
            {
                ChooseRankDialog dlg = new ChooseRankDialog(names[giver], cands, refusals, hands[asker]);
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    AddLog(Lang.T("log_you_gaveup"));
                    return;
                }
                want = dlg.SelectedRank;
            }
            else
            {
                want = cands[0];
            }
            if (want < 0) return;
            asked.Add(want);

            if (CountRank(hands[giver], want) == 0)
            {
                AddLog(Lang.F("log_want_absent", names[asker], Card.RankTextOf(want), names[giver]));
                continue;
            }

            bool give;
            if (giver == 0 && !Silent)
            {
                GiveDialog gd = new GiveDialog(Lang.F("give_head", names[asker], Card.RankTextOf(want)),
                    Lang.F("give_sub", refusals));
                give = (gd.ShowDialog(this) == DialogResult.OK);
            }
            else
            {
                give = BotDecideGive(hands[giver], want);
            }

            if (give)
            {
                Card card = hands[giver].First(c => c.Rank == want);
                MoveCard(hands[giver], hands[asker], card);
                AddLog(Lang.F("log_gave", names[giver], names[asker], card.Display));
                UpdateUI();
                Invalidate();
                return;
            }

            refusals++;
            AddLog(Lang.F("log_refuse", names[giver], refusals));
            if (refusals >= 3)
            {
                Card rc = hands[giver][rnd.Next(hands[giver].Count)];
                MoveCard(hands[giver], hands[asker], rc);
                AddLog(Lang.F("log_force_give", names[giver], names[asker], rc.Display));
                UpdateUI();
                Invalidate();
                return;
            }
        }
    }

    List<int> RequestCandidates(List<Card> askerHand, List<Card> giverHand, int forbidRank)
    {
        List<int> res = new List<int>();
        if (giverHand.Count == 0) return res;

        // 对方手里最大的一张不能被要走（含刚上贡的那张）
        Card top = BiggestCard(giverHand);
        int topRank = (top != null) ? top.Rank : -1;

        Dictionary<int, int> mine = new Dictionary<int, int>();
        foreach (Card c in askerHand)
        {
            if (!mine.ContainsKey(c.Rank)) mine[c.Rank] = 0;
            mine[c.Rank] = mine[c.Rank] + 1;
        }
        foreach (Card c in giverHand)
        {
            if (c.IsJoker) continue;
            if (c.Rank == forbidRank) continue;
            if (c.Rank == topRank) continue;
            int cnt = mine.ContainsKey(c.Rank) ? mine[c.Rank] : 0;
            if ((cnt == 1 || cnt == 2) && !res.Contains(c.Rank)) res.Add(c.Rank);
        }
        res.Sort(delegate(int a, int b) { return Card.GameValue(b).CompareTo(Card.GameValue(a)); });
        return res;
    }

    bool BotDecideGive(List<Card> hand, int rank)
    {
        int cnt = CountRank(hand, rank);
        if (cnt >= 2) return true;
        return Card.GameValue(rank) <= 8;
    }

    int CountRank(List<Card> hand, int rank)
    {
        int n = 0;
        foreach (Card c in hand) if (c.Rank == rank) n++;
        return n;
    }

    void MoveCard(List<Card> from, List<Card> to, Card c)
    {
        from.Remove(c);
        to.Add(c);
        to.Sort(new Comparison<Card>(Card.CompareDesc));
    }

    void AddLog(string s)
    {
        logLines.Add(s);
        while (logLines.Count > 40) logLines.RemoveAt(0);
        Invalidate();
    }

    int TotalCards()
    {
        int t = 0;
        for (int i = 0; i < 3; i++) t += hands[i].Count;
        return t;
    }

    // ================= 自动跑全局（自检用） =================
    public void RunSilentTest()
    {
        Silent = true;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== 争上游 逻辑自检 ===");
        sb.AppendLine("第 1 局开局手牌 18/18/18 合计 54 √");
        int roundsDone = 0;
        StartGame();
        int guard = 0;
        while (!SilentFinished && guard < 200000)
        {
            guard++;
            if (roundOver)
            {
                sb.AppendLine("ERROR: 局面卡在已结束状态");
                break;
            }
            int seat = current;
            if (hands[seat].Count == 0)
            {
                sb.AppendLine("ERROR: 轮到没有牌的玩家 " + names[seat]);
                break;
            }
            Play choice;
            bool mustH4 = roundFirstPlay && roundNo == 1;
            if (lastPlay == null) choice = ChooseLead(hands[seat], mustH4);
            else choice = ChooseFollow(hands[seat], lastPlay);

            if (choice == null)
            {
                if (lastPlay == null)
                {
                    sb.AppendLine("ERROR: 领出者找不到任何合法牌型");
                    break;
                }
                DoPass(seat);
            }
            else
            {
                ApplyPlay(seat, choice);
            }

            if (roundNo != roundsDone)
            {
                roundsDone = roundNo;
                sb.AppendLine("第 " + roundNo + " 局开始，牌数合计 " + TotalCards());
            }
            if (TotalCards() > 54 || TotalCards() < 0)
            {
                sb.AppendLine("ERROR: 牌数异常 " + TotalCards());
                break;
            }
            int zeros = 0;
            for (int i = 0; i < 3; i++) if (hands[i].Count == 0) zeros++;
            if (!SilentFinished && zeros > 1)
            {
                sb.AppendLine("ERROR: 同时有两个空手玩家");
                break;
            }
        }
        sb.AppendLine("循环结束：finished=" + SilentFinished + "，步数=" + guard + "，局数=" + roundNo);
        sb.AppendLine(Notes.ToString());
        sb.AppendLine("结束时牌数合计 " + TotalCards());
        for (int i = 0; i < roundRanks.Count; i++)
        {
            int[] rk = roundRanks[i];
            string invalid = (rk[0] == rk[1] || rk[1] == rk[2] || rk[0] == rk[2]) ? "  ERROR:名次重复" : "  OK";
            sb.AppendLine("第 " + (i + 1) + " 局：上游 " + names[rk[0]] + " / 中游 " + names[rk[1]] + " / 下游 " + names[rk[2]] + invalid);
        }
        File.WriteAllText("selftest.txt", sb.ToString());
    }

    // ================= 绘制 =================
    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        Ui.Felt(g, PW, PH);

        GraphicsPath tp = new GraphicsPath();
        tp.AddEllipse(new Rectangle(24, 104, 1232, 520));
        using (PathGradientBrush pb = new PathGradientBrush(tp))
        {
            pb.CenterColor = Color.FromArgb(24, 124, 82);
            pb.SurroundColors = new Color[] { Color.FromArgb(7, 48, 32) };
            g.FillPath(pb, tp);
        }
        using (Pen pn = new Pen(Color.FromArgb(70, 214, 180, 96), 2f)) g.DrawPath(pn, tp);
        tp.Dispose();

        DrawHeader(g);
        DrawBotPanel(g, 1);
        DrawBotPanel(g, 2);
        DrawCenterPlay(g);
        DrawLog(g);
        DrawHand(g);
        DrawTip(g);
    }

    void DrawHeader(Graphics g)
    {
        RectangleF pill = new RectangleF(450, 10, 380, 44);
        using (GraphicsPath p = CardPainter.RoundRect(pill, 22f))
        {
            using (LinearGradientBrush b = new LinearGradientBrush(pill, Color.FromArgb(9, 60, 40), Color.FromArgb(5, 40, 26), 90f))
                g.FillPath(b, p);
            using (Pen pn = new Pen(Color.FromArgb(110, 220, 185, 100), 1f)) g.DrawPath(pn, p);
        }
        Font f = Ui.F(12, FontStyle.Bold);
        g.DrawString(Lang.F("head_round", roundNo), f, new SolidBrush(Color.FromArgb(255, 226, 160)), pill, Ui.Center);
        f.Dispose();

        string timerText;
        Color timerColor = Color.FromArgb(196, 225, 208);
        if (roundOver) timerText = Lang.T("round_over");
        else if (current == 0)
        {
            timerText = Lang.F("timer_your_turn", remainSec);
            if (remainSec <= 5) timerColor = Color.FromArgb(255, 150, 120);
        }
        else
        {
            timerText = Lang.F("timer_bot", names[current]);
        }
        Font f2 = Ui.F(10, FontStyle.Regular);
        g.DrawString(timerText, f2, new SolidBrush(timerColor), new RectangleF(450, 56, 380, 24), Ui.Center);
        f2.Dispose();
    }

    string RoleText(int seat)
    {
        if (prevRoles[0] == seat) return Lang.T("role_up");
        if (prevRoles[1] == seat) return Lang.T("role_mid");
        if (prevRoles[2] == seat) return Lang.T("role_down");
        return "";
    }

    void DrawBotPanel(Graphics g, int seat)
    {
        Rectangle box = (seat == 1) ? new Rectangle(44, 48, 252, 110) : new Rectangle(984, 48, 252, 110);
        bool active = (current == seat) && !roundOver;
        using (GraphicsPath p = CardPainter.RoundRect(box, 12f))
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(120, 3, 28, 18))) g.FillPath(b, p);
            using (Pen pn = new Pen(active ? Color.FromArgb(255, 200, 80) : Color.FromArgb(85, 150, 120), active ? 2f : 1f))
                g.DrawPath(pn, p);
        }

        Font f1 = Ui.F(12, FontStyle.Bold);
        g.DrawString(names[seat], f1, new SolidBrush(active ? Color.FromArgb(255, 226, 150) : Color.FromArgb(232, 246, 238)), box.X + 14, box.Y + 8);
        f1.Dispose();

        string role = RoleText(seat);
        if (role != "")
        {
            Font fr = Ui.F(8.5f, FontStyle.Regular);
            g.DrawString(role, fr, new SolidBrush(Color.FromArgb(170, 215, 190)), box.X + 14, box.Y + 30);
            fr.Dispose();
        }

        Font f3 = Ui.F(15, FontStyle.Bold);
        g.DrawString(Lang.F("remain", hands[seat].Count), f3, new SolidBrush(Color.FromArgb(240, 245, 235)), box.X + 14, box.Y + 50);
        f3.Dispose();

        int show = Math.Min(3, hands[seat].Count);
        for (int i = 0; i < show; i++)
        {
            RectangleF r = new RectangleF(box.Right - 76 + i * 17, box.Y + 62, 28f, 40f);
            CardPainter.DrawBack(g, r);
        }

        if (lastAction[seat] != null)
        {
            Font fa = Ui.F(9, FontStyle.Bold);
            Color col = (lastAction[seat] == "pass") ? Color.FromArgb(205, 165, 155)
                     : (lastAction[seat] == "done") ? Color.FromArgb(255, 210, 120)
                     : Color.FromArgb(170, 232, 202);
            g.DrawString((lastAction[seat] == "pass") ? Lang.T("action_pass") : (lastAction[seat] == "done") ? Lang.T("action_done") : Lang.T("action_play"), fa, new SolidBrush(col),
                new RectangleF(box.X + 8, box.Y + 8, box.Width - 20, 22), Ui.Far);
            fa.Dispose();
        }
    }

    void DrawCenterPlay(Graphics g)
    {
        Rectangle area = new Rectangle(380, 278, 520, 200);

        if (centerPlay == null)
        {
            Font fh = Ui.F(12, FontStyle.Regular);
            string hint = roundOver ? "" : (current == 0 ? Lang.T("center_you") : Lang.F("center_bot", names[current]));
            g.DrawString(hint, fh, new SolidBrush(Color.FromArgb(150, 202, 178)), area, Ui.Center);
            fh.Dispose();
            return;
        }

        Font fl = Ui.F(10, FontStyle.Bold);
        g.DrawString(Lang.F("center_play", names[centerSeat], centerPlay.TypeName), fl,
            new SolidBrush(Color.FromArgb(255, 222, 145)), new RectangleF(area.X, area.Y - 6, area.Width, 26), Ui.Center);
        fl.Dispose();

        int n = centerPlay.Cards.Count;
        float cw = 72f, ch = 100f, step = 30f;
        float total = cw + (n - 1) * step;
        float x = area.X + (area.Width - total) / 2f;
        float y = area.Y + 30f;
        foreach (Card c in centerPlay.Cards)
        {
            CardPainter.Draw(g, new RectangleF(x, y, cw, ch), c, false);
            x += step;
        }

        if (lastPlay == null && !roundOver)
        {
            Font fs = Ui.F(9, FontStyle.Regular);
            string who = (current == 0) ? Lang.T("name_you") : names[current];
            g.DrawString(Lang.F("all_pass", who), fs, new SolidBrush(Color.FromArgb(196, 232, 212)),
                new RectangleF(area.X, area.Y + 142, area.Width, 24), Ui.Center);
            fs.Dispose();
        }
    }
    void DrawLog(Graphics g)
    {
        Rectangle box = new Rectangle(1032, 268, 214, 332);
        using (GraphicsPath p = CardPainter.RoundRect(box, 10f))
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(96, 3, 26, 17))) g.FillPath(b, p);
            using (Pen pn = new Pen(Color.FromArgb(60, 140, 190, 160), 1f)) g.DrawPath(pn, p);
        }
        Font ft = Ui.F(9, FontStyle.Bold);
        g.DrawString(Lang.T("log_title"), ft, new SolidBrush(Color.FromArgb(255, 220, 150)), box.X + 12, box.Y + 8);
        ft.Dispose();

        Font f = Ui.F(8f, FontStyle.Regular);
        float y = box.Y + 34f;
        int shown = Math.Min(15, logLines.Count);
        for (int i = logLines.Count - shown; i < logLines.Count; i++)
        {
            Color col = (i == logLines.Count - 1) ? Color.FromArgb(255, 235, 190) : Color.FromArgb(178, 214, 196);
            g.DrawString(logLines[i], f, new SolidBrush(col), new RectangleF(box.X + 10, y, box.Width - 20, 18f), Ui.Ellipsis);
            y += 19f;
        }
        f.Dispose();
    }

    void DrawHand(Graphics g)
    {
        handRects.Clear();
        int n = hands[0].Count;

        Font f = Ui.F(10, FontStyle.Bold);
        string role = RoleText(0);
        g.DrawString(Lang.F("hand_info", Lang.T("name_you") + (role == "" ? "" : "　" + role), n),
            f, new SolidBrush(Color.FromArgb(230, 246, 235)), 30, 630);
        f.Dispose();

        if (n == 0) return;

        float cw = 76f, ch = 106f, step = 90f;
        float avail = 980f;
        if (n > 1 && cw + (n - 1) * 90f > avail) step = (avail - cw) / (n - 1);
        float total = cw + (n - 1) * step;
        float x0 = 30f + (avail - total) / 2f;
        float y0 = 658f;

        if (current == 0 && !roundOver)
        {
            RectangleF glowR = new RectangleF(x0 - 16f, y0 - 20f, total + 32f, ch + 34f);
            using (GraphicsPath p = CardPainter.RoundRect(glowR, 18f))
            {
                using (PathGradientBrush pb = new PathGradientBrush(p))
                {
                    pb.CenterColor = Color.FromArgb(46, 255, 214, 120);
                    pb.SurroundColors = new Color[] { Color.FromArgb(0, 255, 214, 120) };
                    g.FillPath(pb, p);
                }
            }
        }

        for (int i = 0; i < n; i++)
        {
            Card c = hands[0][i];
            bool sel = selected.Contains(c);
            handRects.Add(new RectangleF(x0 + i * step, y0 - (sel ? 18f : 0f), cw, ch));
        }
        // 先画未选中的，再画选中的，避免被右侧牌遮住
        for (int i = 0; i < n; i++)
        {
            Card c = hands[0][i];
            if (!selected.Contains(c)) CardPainter.Draw(g, handRects[i], c, false);
        }
        for (int i = 0; i < n; i++)
        {
            Card c = hands[0][i];
            if (selected.Contains(c)) CardPainter.Draw(g, handRects[i], c, true);
        }
    }

    void DrawTip(Graphics g)
    {
        if (string.IsNullOrEmpty(tipText)) return;
        Font f = Ui.F(11, FontStyle.Bold);
        SizeF sz = g.MeasureString(tipText, f);
        RectangleF r = new RectangleF((PW - (sz.Width + 48f)) / 2f, 566f, sz.Width + 48f, 38f);
        using (GraphicsPath p = CardPainter.RoundRect(r, 19f))
        {
            using (SolidBrush b = new SolidBrush(Color.FromArgb(218, 8, 34, 24))) g.FillPath(b, p);
            using (Pen pn = new Pen(Color.FromArgb(170, 240, 200, 110), 1f)) g.DrawPath(pn, p);
        }
        g.DrawString(tipText, f, new SolidBrush(Color.FromArgb(255, 240, 210)), r, Ui.Center);
        f.Dispose();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (roundOver || current != 0) return;
        for (int i = handRects.Count - 1; i >= 0; i--)
        {
            if (handRects[i].Contains(e.X, e.Y))
            {
                if (i >= hands[0].Count) return;
                Card c = hands[0][i];
                if (selected.Contains(c)) selected.Remove(c);
                else selected.Add(c);
                Invalidate();
                return;
            }
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        secTimer.Stop();
        botTimer.Stop();
        tipTimer.Stop();
        base.OnFormClosing(e);
    }
}
