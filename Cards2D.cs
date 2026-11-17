using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

static class CardPainter
{
    public const float CW = 76f;
    public const float CH = 106f;

    static readonly StringFormat SFC = MakeCenter();
    static StringFormat MakeCenter()
    {
        StringFormat f = new StringFormat();
        f.Alignment = StringAlignment.Center;
        f.LineAlignment = StringAlignment.Center;
        return f;
    }

    public static GraphicsPath RoundRect(RectangleF r, float rad)
    {
        GraphicsPath p = new GraphicsPath();
        float d = rad * 2f;
        if (d > r.Width) d = r.Width;
        if (d > r.Height) d = r.Height;
        p.AddArc(r.X, r.Y, d, d, 180f, 90f);
        p.AddArc(r.Right - d, r.Y, d, d, 270f, 90f);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0f, 90f);
        p.AddArc(r.X, r.Bottom - d, d, d, 90f, 90f);
        p.CloseFigure();
        return p;
    }

    public static void Draw(Graphics g, RectangleF r, Card c, bool selected)
    {
        if (r.Width <= 1f || r.Height <= 1f) return;
        GraphicsState st = g.Save();
        g.TranslateTransform(r.X, r.Y);
        g.ScaleTransform(r.Width / CW, r.Height / CH);
        DrawLocal(g, c, selected);
        g.Restore(st);
    }

    static void DrawLocal(Graphics g, Card c, bool selected)
    {
        RectangleF body = new RectangleF(0f, 0f, CW - 2f, CH - 4f);

        // 阴影
        GraphicsPath shadow = RoundRect(new RectangleF(1.5f, 3f, body.Width, body.Height), 7f);
        using (SolidBrush sb = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
            g.FillPath(sb, shadow);
        shadow.Dispose();

        GraphicsPath path = RoundRect(body, 7f);
        using (LinearGradientBrush bg = new LinearGradientBrush(body, Color.FromArgb(255, 255, 255), Color.FromArgb(242, 242, 236), 90f))
            g.FillPath(bg, path);
        Color border = selected ? Color.FromArgb(255, 196, 60) : Color.FromArgb(150, 150, 150);
        using (Pen pn = new Pen(border, selected ? 2.2f : 1f))
            g.DrawPath(pn, path);
        path.Dispose();

        if (c.IsJoker) DrawJoker(g, c);
        else if (c.Rank >= 11 && c.Rank <= 14) DrawFace(g, c);
        else DrawNumber(g, c);

        if (selected)
        {
            GraphicsPath glow = RoundRect(new RectangleF(-1f, -1f, body.Width + 2f, body.Height + 2f), 8f);
            using (Pen pn = new Pen(Color.FromArgb(120, 255, 210, 90), 1.2f))
                g.DrawPath(pn, glow);
            glow.Dispose();
        }
    }

    static Brush Ink(Card c)
    {
        return new SolidBrush(c.IsRed ? Color.FromArgb(198, 32, 42) : Color.FromArgb(28, 28, 32));
    }

    static void DrawCorner(Graphics g, Card c, Brush ink)
    {
        Font fr = new Font("Times New Roman", 13f, FontStyle.Bold);
        Font fs = new Font("Segoe UI Symbol", 11f, FontStyle.Regular);
        RectangleF rRank = new RectangleF(2f, 1f, 27f, 19f);
        RectangleF rSuit = new RectangleF(2f, 18f, 27f, 16f);
        g.DrawString(c.RankText, fr, ink, rRank, SFC);
        g.DrawString(c.SuitGlyph, fs, ink, rSuit, SFC);

        GraphicsState st = g.Save();
        g.TranslateTransform(CW - 2f, CH - 4f);
        g.RotateTransform(180f);
        g.DrawString(c.RankText, fr, ink, rRank, SFC);
        g.DrawString(c.SuitGlyph, fs, ink, rSuit, SFC);
        g.Restore(st);

        fr.Dispose(); fs.Dispose();
    }

    static float[][] PipLayout(int rank)
    {
        float c = 0.5f, l = 0.36f, r = 0.64f;
        float y1 = 0.30f, y2 = 0.41f, y3 = 0.50f, y4 = 0.59f, y5 = 0.70f;
        switch (rank)
        {
            case 2: return new float[][] { new float[] { c, y1 }, new float[] { c, y5 } };
            case 3: return new float[][] { new float[] { c, y1 }, new float[] { c, y3 }, new float[] { c, y5 } };
            case 4: return new float[][] { new float[] { l, y1 }, new float[] { r, y1 }, new float[] { l, y5 }, new float[] { r, y5 } };
            case 5:
                return new float[][] { new float[] { l, y1 }, new float[] { r, y1 }, new float[] { c, y3 }, new float[] { l, y5 }, new float[] { r, y5 } };
            case 6:
                return new float[][] { new float[] { l, y1 }, new float[] { r, y1 }, new float[] { l, y3 }, new float[] { r, y3 }, new float[] { l, y5 }, new float[] { r, y5 } };
            case 7:
                return new float[][] { new float[] { l, y1 }, new float[] { r, y1 }, new float[] { c, y2 }, new float[] { l, y3 }, new float[] { r, y3 }, new float[] { l, y5 }, new float[] { r, y5 } };
            case 8:
                return new float[][] { new float[] { l, y1 }, new float[] { r, y1 }, new float[] { c, y2 }, new float[] { l, y3 }, new float[] { r, y3 }, new float[] { c, y4 }, new float[] { l, y5 }, new float[] { r, y5 } };
            case 9:
                return new float[][] { new float[] { l, y1 }, new float[] { r, y1 }, new float[] { l, 0.43f }, new float[] { r, 0.43f }, new float[] { c, y3 }, new float[] { l, 0.57f }, new float[] { r, 0.57f }, new float[] { l, y5 }, new float[] { r, y5 } };
            default: // 10
                return new float[][] { new float[] { l, y1 }, new float[] { r, y1 }, new float[] { l, 0.43f }, new float[] { r, 0.43f }, new float[] { l, 0.57f }, new float[] { r, 0.57f }, new float[] { l, y5 }, new float[] { r, y5 }, new float[] { c, y2 }, new float[] { c, y4 } };
        }
    }

    static void DrawNumber(Graphics g, Card c)
    {
        Brush ink = Ink(c);
        DrawCorner(g, c, ink);

        float[][] pos = PipLayout(c.Rank);
        Font pip = new Font("Segoe UI Symbol", 12f, FontStyle.Regular);
        foreach (float[] p in pos)
        {
            float x = p[0] * CW - 8f;
            float y = p[1] * CH - 8f;
            g.DrawString(c.SuitGlyph, pip, ink, new RectangleF(x, y, 16f, 16f), SFC);
        }
        pip.Dispose();
        ink.Dispose();
    }

    static void DrawFigure(Graphics g, RectangleF box, Card c, Color accent)
    {
        float w = box.Width, h = box.Height;
        float cx = box.X + w / 2f;
        float headR = w * 0.20f;
        float headCY = box.Y + h * 0.40f;

        Color light = c.IsRed ? Color.FromArgb(252, 226, 214) : Color.FromArgb(222, 230, 246);

        // 身体
        RectangleF bodyR = new RectangleF(cx - w * 0.35f, headCY + headR * 0.55f, w * 0.70f, h * 0.62f);
        using (GraphicsPath bp = new GraphicsPath())
        {
            bp.AddEllipse(bodyR);
            using (SolidBrush b = new SolidBrush(light)) g.FillPath(b, bp);
            using (Pen pn = new Pen(Color.FromArgb(150, accent), 0.9f)) g.DrawPath(pn, bp);
        }

        // 头发
        using (GraphicsPath hp = new GraphicsPath())
        {
            hp.AddEllipse(cx - headR * 1.10f, headCY - headR * 1.15f, headR * 2.20f, headR * 2.20f);
            using (SolidBrush b = new SolidBrush(Color.FromArgb(235, accent))) g.FillPath(b, hp);
            using (Pen pn = new Pen(Color.FromArgb(170, accent), 0.9f)) g.DrawPath(pn, hp);
        }

        // 脸
        RectangleF face = new RectangleF(cx - headR, headCY - headR * 0.86f, headR * 2f, headR * 2f);
        using (SolidBrush b = new SolidBrush(Color.FromArgb(250, 232, 208))) g.FillEllipse(b, face);

        // 眼睛
        using (SolidBrush b = new SolidBrush(Color.FromArgb(120, accent)))
        {
            g.FillEllipse(b, cx - headR * 0.55f, headCY - headR * 0.20f, headR * 0.30f, headR * 0.22f);
            g.FillEllipse(b, cx + headR * 0.25f, headCY - headR * 0.20f, headR * 0.30f, headR * 0.22f);
        }

        // 冠 / 帽
        using (Pen pn = new Pen(Color.FromArgb(200, accent), 0.9f))
        using (SolidBrush b = new SolidBrush(Color.FromArgb(180, accent)))
        {
            if (c.Rank == 13) // K 王冠
            {
                PointF[] pts = new PointF[]
                {
                    new PointF(cx - headR * 1.02f, headCY - headR * 0.86f),
                    new PointF(cx - headR * 0.92f, headCY - headR * 1.48f),
                    new PointF(cx - headR * 0.46f, headCY - headR * 1.05f),
                    new PointF(cx, headCY - headR * 1.62f),
                    new PointF(cx + headR * 0.46f, headCY - headR * 1.05f),
                    new PointF(cx + headR * 0.92f, headCY - headR * 1.48f),
                    new PointF(cx + headR * 1.02f, headCY - headR * 0.86f)
                };
                g.FillPolygon(b, pts);
                g.DrawPolygon(pn, pts);
            }
            else if (c.Rank == 12) // Q 头饰
            {
                g.FillEllipse(b, cx - headR * 1.05f, headCY - headR * 1.42f, headR * 2.1f, headR * 0.72f);
                g.DrawEllipse(pn, cx - headR * 1.05f, headCY - headR * 1.42f, headR * 2.1f, headR * 0.72f);
            }
            else // J 帽 + 羽
            {
                g.FillEllipse(b, cx - headR * 1.05f, headCY - headR * 1.30f, headR * 2.1f, headR * 0.62f);
                g.DrawEllipse(pn, cx - headR * 1.05f, headCY - headR * 1.30f, headR * 2.1f, headR * 0.62f);
                g.DrawLine(pn, cx + headR * 0.85f, headCY - headR * 1.22f, cx + headR * 1.35f, headCY - headR * 1.70f);
            }
        }

        // 衣领
        using (Pen pn = new Pen(Color.FromArgb(170, accent), 0.8f))
            g.DrawLine(pn, cx - headR * 0.55f, headCY + headR * 0.75f, cx + headR * 0.55f, headCY + headR * 0.75f);
    }

    static void DrawFace(Graphics g, Card c)
    {
        Color accent = c.IsRed ? Color.FromArgb(190, 45, 55) : Color.FromArgb(42, 52, 96);

        RectangleF inner = new RectangleF(3f, 2f, CW - 8f, CH - 8f);
        using (GraphicsPath p = RoundRect(inner, 4f))
        {
            using (LinearGradientBrush b = new LinearGradientBrush(inner, Color.FromArgb(255, 253, 246), Color.FromArgb(246, 235, 212), 45f))
                g.FillPath(b, p);
            using (Pen pn = new Pen(Color.FromArgb(196, 172, 132), 1f))
                g.DrawPath(pn, p);
        }

        RectangleF panel = new RectangleF(inner.X + 2f, inner.Y + 2f, inner.Width - 4f, inner.Height - 4f);
        using (Pen pn = new Pen(Color.FromArgb(120, accent), 0.9f))
            g.DrawRectangle(pn, panel.X, panel.Y, panel.Width, panel.Height);

        GraphicsPath split = new GraphicsPath();
        split.AddLine(panel.X, panel.Y + panel.Height / 2f, panel.Right, panel.Y + panel.Height / 2f);
        using (Pen pn = new Pen(Color.FromArgb(90, accent), 0.8f))
            g.DrawPath(pn, split);
        split.Dispose();

        float hh = panel.Height / 2f + 3f;
        DrawFigure(g, new RectangleF(panel.X, panel.Y, panel.Width, hh), c, accent);

        GraphicsState st = g.Save();
        g.TranslateTransform(panel.X + panel.Width / 2f, panel.Y + panel.Height / 2f);
        g.RotateTransform(180f);
        DrawFigure(g, new RectangleF(-panel.Width / 2f, -panel.Height / 2f, panel.Width, hh), c, accent);
        g.Restore(st);

        Brush ink = Ink(c);
        DrawCorner(g, c, ink);
        ink.Dispose();
    }

    static void DrawJoker(Graphics g, Card c)
    {
        Color accent = c.IsRed ? Color.FromArgb(190, 40, 50) : Color.FromArgb(40, 44, 56);

        RectangleF inner = new RectangleF(3f, 2f, CW - 8f, CH - 8f);
        using (GraphicsPath p = RoundRect(inner, 5f))
        {
            using (LinearGradientBrush b = new LinearGradientBrush(inner, Color.FromArgb(255, 252, 238), Color.FromArgb(245, 232, 202), 60f))
                g.FillPath(b, p);
            using (Pen pn = new Pen(Color.FromArgb(210, 178, 120), 1f))
                g.DrawPath(pn, p);
        }

        Brush ink = new SolidBrush(accent);

        // 竖排 JOKER
        Font fj = new Font("Times New Roman", 8.5f, FontStyle.Bold);
        string jk = "JOKER";
        for (int i = 0; i < jk.Length; i++)
            g.DrawString(jk.Substring(i, 1), fj, ink, new RectangleF(3f, 20f + i * 12.5f, 9f, 12f), SFC);
        fj.Dispose();

        // 星
        Font fst = new Font("Segoe UI Symbol", 11f, FontStyle.Regular);
        g.DrawString("★", fst, ink, new RectangleF(10f, 1f, 16f, 16f), SFC);
        fst.Dispose();

        // 小丑头像
        float cx = 46f, cy = 46f;
        using (SolidBrush b = new SolidBrush(Color.FromArgb(250, 232, 208)))
            g.FillEllipse(b, cx - 12f, cy - 12f, 24f, 24f);
        using (Pen pn = new Pen(Color.FromArgb(150, accent), 1f))
            g.DrawEllipse(pn, cx - 12f, cy - 12f, 24f, 24f);
        using (SolidBrush b = new SolidBrush(Color.FromArgb(170, accent)))
        {
            g.FillEllipse(b, cx - 8f, cy - 3f, 4.5f, 4.5f);
            g.FillEllipse(b, cx + 3.5f, cy - 3f, 4.5f, 4.5f);
            g.FillPie(b, cx - 6f, cy + 2f, 12f, 8f, 0f, 180f);
            g.FillPolygon(b, new PointF[] { new PointF(cx - 13f, cy - 13f), new PointF(cx, cy - 21f), new PointF(cx + 13f, cy - 13f) });
        }

        // 竖排 大王 / 小王（跟随当前语言，任意长度都不裁字）
        Font fc = new Font(Lang.FontFamily, 8.5f, FontStyle.Bold);
        string t = c.RankText;
        GraphicsState stj = g.Save();
        g.TranslateTransform(34f, 132f);
        g.RotateTransform(-90f);
        g.DrawString(t, fc, ink, new RectangleF(0f, 0f, 46f, 12f), SFC);
        g.Restore(stj);
        fc.Dispose();
        ink.Dispose();
    }

    // 牌背
    public static void DrawBack(Graphics g, RectangleF r)
    {
        if (r.Width <= 1f) return;
        GraphicsState st = g.Save();
        g.TranslateTransform(r.X, r.Y);
        g.ScaleTransform(r.Width / CW, r.Height / CH);

        RectangleF body = new RectangleF(0f, 0f, CW - 2f, CH - 4f);
        GraphicsPath shadow = RoundRect(new RectangleF(1.5f, 3f, body.Width, body.Height), 7f);
        using (SolidBrush sb = new SolidBrush(Color.FromArgb(70, 0, 0, 0))) g.FillPath(sb, shadow);
        shadow.Dispose();

        GraphicsPath path = RoundRect(body, 7f);
        using (LinearGradientBrush bg = new LinearGradientBrush(body, Color.FromArgb(122, 26, 34), Color.FromArgb(66, 12, 20), 90f))
            g.FillPath(bg, path);
        using (Pen pn = new Pen(Color.FromArgb(120, 220, 190, 130), 1f))
            g.DrawPath(pn, path);

        RectangleF inner = new RectangleF(5f, 4f, body.Width - 10f, body.Height - 10f);
        GraphicsPath ip = RoundRect(inner, 4f);
        using (Pen pn = new Pen(Color.FromArgb(90, 230, 200, 145), 1f)) g.DrawPath(pn, ip);

        // 斜纹
        g.SetClip(ip);
        using (Pen pn = new Pen(Color.FromArgb(45, 240, 215, 160), 1f))
        {
            for (int x = -20; x < 90; x += 6)
            {
                g.DrawLine(pn, inner.X + x, inner.Y, inner.X + x + 40, inner.Bottom);
                g.DrawLine(pn, inner.X + x, inner.Bottom, inner.X + x + 40, inner.Y);
            }
        }
        g.ResetClip();

        // 中心徽记
        using (SolidBrush b = new SolidBrush(Color.FromArgb(35, 255, 250, 240)))
            g.FillEllipse(b, 20f, 36f, 36f, 36f);
        using (Pen pn = new Pen(Color.FromArgb(110, 230, 205, 150), 1f))
            g.DrawEllipse(pn, 20f, 36f, 36f, 36f);
        Font fc = new Font("Times New Roman", 14f, FontStyle.Bold);
        g.DrawString("Z", fc, new SolidBrush(Color.FromArgb(150, 245, 225, 180)), new RectangleF(20f, 36f, 36f, 36f), SFC);
        fc.Dispose();
        ip.Dispose();
        path.Dispose();
        g.Restore(st);
    }

    // 供自检：输出一张样式预览图
    public static void RenderPreview()
    {
        Bitmap bmp = new Bitmap(1180, 760);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            using (LinearGradientBrush bg = new LinearGradientBrush(new Rectangle(0, 0, 1180, 760), Color.FromArgb(18, 96, 62), Color.FromArgb(4, 34, 22), 90f))
                g.FillRectangle(bg, 0, 0, 1180, 760);

            List<Card> sample = new List<Card>();
            sample.Add(new Card(5, 1));
            sample.Add(new Card(8, 0));
            sample.Add(new Card(16, 4));
            sample.Add(new Card(15, 4));
            sample.Add(new Card(13, 1));
            sample.Add(new Card(12, 2));
            sample.Add(new Card(11, 3));
            sample.Add(new Card(10, 0));
            sample.Add(new Card(9, 1));
            sample.Add(new Card(4, 1));
            sample.Add(new Card(3, 2));
            sample.Add(new Card(2, 3));
            sample.Add(new Card(7, 0));
            sample.Add(new Card(6, 1));

            for (int i = 0; i < sample.Count; i++)
            {
                int col = i % 7, row = i / 7;
                RectangleF r = new RectangleF(30 + col * 96, 30 + row * 140, CW, CH);
                Draw(g, r, sample[i], false);
            }
            // 牌背
            DrawBack(g, new RectangleF(30 + 6 * 96, 170, CW, CH));

            // 大尺寸检查
            Draw(g, new RectangleF(420, 380, 150, 210), new Card(5, 1), true);
            Draw(g, new RectangleF(600, 380, 150, 210), new Card(16, 4), false);
            Draw(g, new RectangleF(780, 380, 150, 210), new Card(13, 0), false);
            Draw(g, new RectangleF(960, 380, 150, 210), new Card(10, 3), false);
        }
        bmp.Save("preview.png", System.Drawing.Imaging.ImageFormat.Png);
        bmp.Dispose();
    }
}
