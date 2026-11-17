package com.zhengshengyou.app;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.RectF;
import android.graphics.Typeface;
import android.os.Handler;
import android.os.Looper;
import android.text.TextUtils;
import android.view.MotionEvent;
import android.view.View;

import com.zhengshengyou.engine.Card;
import com.zhengshengyou.engine.Game;
import com.zhengshengyou.engine.Lang;
import com.zhengshengyou.engine.Play;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

/**
 * Touch table for the Android edition. The rules live in {@link Game}; this class only
 * draws and routes taps, and mirrors the desktop layout: opponents on top, played cards
 * in the centre, your hand along the bottom.
 */
public class TableView extends View implements Game.Callbacks {

    private final MainActivity activity;
    private Game game;
    private final Set<Card> selected = new HashSet<Card>();
    private final Handler ui = new Handler(Looper.getMainLooper());

    private float w, h, dp;
    private float topBarH, handTop, cardW, cardH, step, cardTop;

    private final Paint pText = new Paint(Paint.ANTI_ALIAS_FLAG);
    private final Paint pSmall = new Paint(Paint.ANTI_ALIAS_FLAG);
    private final Paint pBold = new Paint(Paint.ANTI_ALIAS_FLAG);
    private final Paint pTitle = new Paint(Paint.ANTI_ALIAS_FLAG);
    private final Paint pBody = new Paint(Paint.ANTI_ALIAS_FLAG);
    private final Paint pBorder = new Paint(Paint.ANTI_ALIAS_FLAG);
    private final Paint pFill = new Paint(Paint.ANTI_ALIAS_FLAG);
    private final Paint pShadow = new Paint(Paint.ANTI_ALIAS_FLAG);
    private final Paint pCardRank = new Paint(Paint.ANTI_ALIAS_FLAG);
    private final Paint pCardSuit = new Paint(Paint.ANTI_ALIAS_FLAG);

    private final RectF rLeftPanel = new RectF();
    private final RectF rRightPanel = new RectF();
    private final RectF rCenter = new RectF();
    private final RectF rLog = new RectF();
    private final RectF btnPlay = new RectF();
    private final RectF btnPass = new RectF();
    private final RectF btnMenu = new RectF();
    private final RectF btnLang = new RectF();

    private final List<RectF> handRects = new ArrayList<RectF>();
    private final List<Card> handCards = new ArrayList<Card>();

    private boolean dialogOpen;
    private String tip = "";
    private int remainSec = 30;
    private int timerSeat = -1;

    public TableView(MainActivity a) {
        super(a);
        activity = a;
        dp = getResources().getDisplayMetrics().density;
        setBackgroundColor(Color.rgb(6, 45, 30));

        Typeface tf = Typeface.DEFAULT;
        Typeface tfb = Typeface.create(Typeface.DEFAULT, Typeface.BOLD);
        pText.setTypeface(tfb);
        pText.setTextSize(dp * 14);
        pText.setColor(Color.rgb(240, 245, 235));

        pSmall.setTypeface(tf);
        pSmall.setTextSize(dp * 11);
        pSmall.setColor(Color.rgb(170, 205, 185));

        pBold.setTypeface(tfb);
        pBold.setTextSize(dp * 15);
        pBold.setColor(Color.rgb(255, 226, 160));

        pTitle.setTypeface(tfb);
        pTitle.setTextSize(dp * 20);
        pTitle.setColor(Color.rgb(255, 230, 170));
        pTitle.setTextAlign(Paint.Align.CENTER);

        pBody.setTypeface(tfb);
        pBody.setTextSize(dp * 13);
        pBody.setColor(Color.rgb(255, 236, 190));
        pBody.setTextAlign(Paint.Align.CENTER);

        pCardRank.setTypeface(tfb);
        pCardSuit.setTypeface(tf);
    }

    // ================= lifecycle / match =================

    public void startNewMatch() {
        game = new Game(System.nanoTime());
        game.setCallbacks(this);
        game.newMatch();
        selected.clear();
        invalidate();
        if (!introShown) {
            introShown = true;
            showIntro();
            return;
        }
        newTurn();
    }

    private void showIntro() {
        dialogOpen = true;
        AlertDialog.Builder b = new AlertDialog.Builder(activity);
        b.setTitle(Lang.t("game_title"));
        b.setMessage(Lang.t("start_msg_body"));
        b.setPositiveButton(Lang.t("btn_start_play"), new DialogInterface.OnClickListener() {
            @Override public void onClick(DialogInterface d, int which) {
                dialogOpen = false;
                newTurn();
                invalidate();
            }
        });
        b.setNeutralButton(Lang.t("btn_language"), new DialogInterface.OnClickListener() {
            @Override public void onClick(DialogInterface d, int which) {
                dialogOpen = false;
                showLanguage(true);
            }
        });
        b.setCancelable(false);
        showDialog(b);
    }

    private void showDialog(AlertDialog.Builder b) {
        AlertDialog d = b.show();
        d.getWindow().getDecorView().setLayoutDirection(
                Lang.isRtl() ? View.LAYOUT_DIRECTION_RTL : View.LAYOUT_DIRECTION_LTR);
    }

    private boolean introShown;

    public void stopTimers() {
        ui.removeCallbacksAndMessages(null);
    }

    private final Runnable botRunnable = new Runnable() {
        @Override public void run() {
            if (game == null || dialogOpen) return;
            if (game.state == Game.ST_PLAY && game.current != Game.ME) {
                game.botTurn();
                invalidate();
                schedule();
            }
        }
    };

    private final Runnable tickRunnable = new Runnable() {
        @Override public void run() {
            if (game == null || dialogOpen) return;
            if (game.state != Game.ST_PLAY) return;
            if (game.current != Game.ME) return;
            remainSec--;
            if (remainSec <= 0) {
                remainSec = 0;
                showTip(Lang.t("tip_timeout"));
                game.autoHuman();
                selected.clear();
                invalidate();
                schedule();
                return;
            }
            invalidate();
            ui.postDelayed(this, 1000);
        }
    };

    private void schedule() {
        ui.removeCallbacks(botRunnable);
        ui.removeCallbacks(tickRunnable);
        if (game == null || dialogOpen) return;
        if (game.state != Game.ST_PLAY) { timerSeat = -1; return; }
        if (game.current == Game.ME) {
            if (timerSeat != Game.ME) { timerSeat = Game.ME; remainSec = 30; }
            ui.postDelayed(tickRunnable, 1000);
        } else {
            timerSeat = game.current;
            ui.postDelayed(botRunnable, 950);
        }
    }

    private void newTurn() {
        timerSeat = -1;
        remainSec = 30;
        schedule();
    }

    private void showTip(String t) {
        tip = t;
        invalidate();
        ui.postDelayed(new Runnable() {
            @Override public void run() { tip = ""; invalidate(); }
        }, 1800);
    }

    // ================= callbacks from the engine =================

    @Override public void onChanged() {
        invalidate();
    }

    @Override public void onAskRank(final int giverSeat, final int askerSeat, final List<Integer> cands, final int refusals) {
        dialogOpen = true;
        final String[] labels = new String[cands.size()];
        for (int i = 0; i < cands.size(); i++) {
            int rank = cands.get(i);
            int have = 0;
            for (Card c : game.hands[askerSeat]) if (c.rank == rank) have++;
            String hint = Lang.t(have == 1 ? "ask_pair" : "ask_triple");
            labels[i] = Card.rankTextOf(rank) + "  ·  " + hint;
        }
        AlertDialog.Builder b = new AlertDialog.Builder(activity);
        b.setTitle(Lang.f("ask_head", game.name(giverSeat)));
        b.setItems(labels, new DialogInterface.OnClickListener() {
            @Override public void onClick(DialogInterface d, int which) {
                d.dismiss();
                dialogOpen = false;
                game.chooseRank(cands.get(which));
                invalidate();
                schedule();
            }
        });
        b.setNegativeButton(Lang.t("ask_cancel"), new DialogInterface.OnClickListener() {
            @Override public void onClick(DialogInterface d, int which) {
                dialogOpen = false;
                game.giveUpRequest();
                invalidate();
                schedule();
            }
        });
        b.setOnCancelListener(new DialogInterface.OnCancelListener() {
            @Override public void onCancel(DialogInterface d) {
                dialogOpen = false;
                game.giveUpRequest();
                invalidate();
                schedule();
            }
        });
        showDialog(b);
    }

    @Override public void onAskGive(final int askerSeat, final int rank, final int refusals) {
        dialogOpen = true;
        AlertDialog.Builder b = new AlertDialog.Builder(activity);
        b.setTitle(Lang.t("give_title"));
        b.setMessage(Lang.f("give_head", game.name(askerSeat), Card.rankTextOf(rank)) + "\n\n"
                + Lang.f("give_sub", refusals));
        b.setPositiveButton(Lang.t("give_yes"), new DialogInterface.OnClickListener() {
            @Override public void onClick(DialogInterface d, int which) {
                dialogOpen = false;
                game.decideGive(true);
                invalidate();
                schedule();
            }
        });
        b.setNegativeButton(Lang.t("give_no"), new DialogInterface.OnClickListener() {
            @Override public void onClick(DialogInterface d, int which) {
                dialogOpen = false;
                game.decideGive(false);
                invalidate();
                schedule();
            }
        });
        b.setOnCancelListener(new DialogInterface.OnCancelListener() {
            @Override public void onCancel(DialogInterface d) {
                dialogOpen = false;
                game.decideGive(false);
                invalidate();
                schedule();
            }
        });
        showDialog(b);
    }

    @Override public void onTradesFinished() {
        dialogOpen = true;
        StringBuilder sb = new StringBuilder();
        int from = Math.max(0, game.log.size() - 12);
        for (int i = from; i < game.log.size(); i++) sb.append(game.log.get(i)).append('\n');
        sb.append('\n').append(Lang.f("trade_hands", game.name(0), game.hands[0].size(),
                game.name(1), game.hands[1].size(), game.name(2), game.hands[2].size()));

        AlertDialog.Builder b = new AlertDialog.Builder(activity);
        b.setTitle(Lang.f("notice_trade", game.roundNo));
        b.setMessage(sb.toString());
        b.setPositiveButton(Lang.t("btn_start_play"), new DialogInterface.OnClickListener() {
            @Override public void onClick(DialogInterface d, int which) {
                dialogOpen = false;
                game.startPlay();
                newTurn();
                invalidate();
            }
        });
        b.setCancelable(false);
        showDialog(b);
    }

    @Override public void onRoundEnd(final int[] ranks, boolean lastRound) {
        if (lastRound) return; // the scoreboard dialog follows immediately
        dialogOpen = true;
        String msg = Lang.f("notice_round_rank", game.roundNo) + "\n\n"
                + Lang.f("lbl_up", game.name(ranks[0])) + "\n"
                + Lang.f("lbl_mid", game.name(ranks[1])) + "\n"
                + Lang.f("lbl_down", game.name(ranks[2])) + "\n\n"
                + Lang.t("msg_next_hint");
        AlertDialog.Builder b = new AlertDialog.Builder(activity);
        b.setTitle(Lang.f("notice_round_end", game.roundNo));
        b.setMessage(msg);
        b.setPositiveButton(Lang.t("btn_next"), new DialogInterface.OnClickListener() {
            @Override public void onClick(DialogInterface d, int which) {
                dialogOpen = false;
                game.nextRound();
                selected.clear();
                newTurn();
                invalidate();
            }
        });
        b.setCancelable(false);
        showDialog(b);
    }

    @Override public void onMatchEnd(List<int[]> allRanks) {
        dialogOpen = true;
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < allRanks.size(); i++) {
            int[] rk = allRanks.get(i);
            sb.append(Lang.f("total_line", i + 1, game.name(rk[0]), game.name(rk[1]), game.name(rk[2]) + "\n"));
        }
        int[] cnt = new int[3];
        for (int[] rk : allRanks) cnt[rk[0]]++;
        sb.append('\n').append(Lang.f("total_wins", game.name(0), cnt[0], game.name(1), cnt[1], game.name(2), cnt[2]));

        AlertDialog.Builder b = new AlertDialog.Builder(activity);
        b.setTitle(Lang.t("total_title"));
        b.setMessage(sb.toString());
        b.setPositiveButton(Lang.t("btn_new_match"), new DialogInterface.OnClickListener() {
            @Override public void onClick(DialogInterface d, int which) {
                dialogOpen = false;
                startNewMatch();
            }
        });
        b.setNegativeButton(Lang.t("btn_menu"), new DialogInterface.OnClickListener() {
            @Override public void onClick(DialogInterface d, int which) {
                dialogOpen = false;
                showMenu();
            }
        });
        b.setCancelable(false);
        showDialog(b);
    }

    // ================= menus =================

    public void showMenu() {
        String[] items = { Lang.t("btn_rules"), Lang.t("btn_language"), Lang.t("btn_new_match"), Lang.t("btn_exit") };
        AlertDialog.Builder b = new AlertDialog.Builder(activity);
        b.setTitle(Lang.t("menu"));
        b.setItems(items, new DialogInterface.OnClickListener() {
            @Override public void onClick(DialogInterface d, int which) {
                if (which == 0) showRules();
                else if (which == 1) showLanguage(false);
                else if (which == 2) confirmNewMatch();
                else activity.finish();
            }
        });
        showDialog(b);
    }

    private void showRules() {
        dialogOpen = true;
        android.widget.TextView tv = new android.widget.TextView(activity);
        tv.setText(Lang.t("rules_body"));
        tv.setTextSize(15);
        tv.setTextColor(Color.rgb(232, 244, 236));
        tv.setPadding((int) (dp * 16), (int) (dp * 12), (int) (dp * 16), (int) (dp * 12));
        android.widget.ScrollView sv = new android.widget.ScrollView(activity);
        sv.addView(tv);
        AlertDialog.Builder b = new AlertDialog.Builder(activity);
        b.setTitle(Lang.t("rules_title"));
        b.setView(sv);
        b.setPositiveButton(Lang.t("btn_close"), new DialogInterface.OnClickListener() {
            @Override public void onClick(DialogInterface d, int which) { dialogOpen = false; invalidate(); schedule(); }
        });
        b.setCancelable(false);
        showDialog(b);
    }

    private void showLanguage(final boolean fromIntro) {
        dialogOpen = true;
        AlertDialog.Builder b = new AlertDialog.Builder(activity);
        b.setTitle(Lang.t("lang_title"));
        b.setSingleChoiceItems(Lang.NAMES, Lang.idx, new DialogInterface.OnClickListener() {
            @Override public void onClick(DialogInterface d, int which) {
                activity.saveLanguage(Lang.CODES[which]);
                d.dismiss();
                dialogOpen = false;
                requestLayout();
                invalidate();
                if (fromIntro) showIntro();
                else schedule();
            }
        });
        b.setOnCancelListener(new DialogInterface.OnCancelListener() {
            @Override public void onCancel(DialogInterface d) {
                dialogOpen = false;
                invalidate();
                schedule();
            }
        });
        showDialog(b);
    }

    private void confirmNewMatch() {
        AlertDialog.Builder b = new AlertDialog.Builder(activity);
        b.setTitle(Lang.t("btn_new_match"));
        b.setMessage(Lang.t("start_msg_body"));
        b.setPositiveButton(Lang.t("btn_new_match"), new DialogInterface.OnClickListener() {
            @Override public void onClick(DialogInterface d, int which) { startNewMatch(); }
        });
        b.setNegativeButton(Lang.t("btn_close"), null);
        showDialog(b);
    }

    public void confirmExit() {
        AlertDialog.Builder b = new AlertDialog.Builder(activity);
        b.setTitle(Lang.t("quit_confirm_title"));
        b.setMessage(Lang.t("quit_confirm_msg"));
        b.setPositiveButton(Lang.t("btn_exit"), new DialogInterface.OnClickListener() {
            @Override public void onClick(DialogInterface d, int which) { activity.finish(); }
        });
        b.setNegativeButton(Lang.t("btn_close"), null);
        showDialog(b);
    }

    // ================= layout =================

    @Override
    protected void onSizeChanged(int ww, int hh, int oldw, int oldh) {
        super.onSizeChanged(ww, hh, oldw, oldh);
        w = ww;
        h = hh;
        dp = getResources().getDisplayMetrics().density;

        topBarH = dp * 52;
        float panelW = Math.min(w * 0.26f, dp * 200);
        float panelH = dp * 58;
        float panelY = topBarH + dp * 4;
        rLeftPanel.set(dp * 10, panelY, dp * 10 + panelW, panelY + panelH);
        rRightPanel.set(w - dp * 10 - panelW, panelY, w - dp * 10, panelY + panelH);

        cardH = Math.min(h * 0.30f, dp * 150);
        cardW = cardH * 0.70f;
        cardTop = h - cardH - dp * 16;
        handTop = cardTop;

        rCenter.set(w * 0.30f, topBarH + dp * 8, w * 0.70f, cardTop - dp * 62);
        rLog.set(dp * 10, panelY + panelH + dp * 8, w * 0.30f - dp * 6, cardTop - dp * 62);

        float bh = dp * 42;
        float bw = Math.min(dp * 104, w * 0.16f);
        float by = cardTop - dp * 54;

        // language pill in the top bar, mirrored for RTL
        float lw = Math.min(dp * 96, w * 0.17f);
        float lh = dp * 30;
        float ly = dp * 4;
        if (Lang.isRtl()) btnLang.set(dp * 10, ly, dp * 10 + lw, ly + lh);
        else btnLang.set(w - dp * 10 - lw, ly, w - dp * 10, ly + lh);
        if (Lang.isRtl()) {
            btnMenu.set(w - dp * 10 - bw, by, w - dp * 10, by + bh);
            btnPass.set(dp * 10, by, dp * 10 + bw, by + bh);
            btnPlay.set(dp * 10 + bw + dp * 10, by, dp * 10 + bw * 2 + dp * 10, by + bh);
        } else {
            btnMenu.set(dp * 10, by, dp * 10 + bw, by + bh);
            btnPass.set(w - dp * 10 - bw, by, w - dp * 10, by + bh);
            btnPlay.set(w - dp * 20 - bw * 2, by, w - dp * 20 - bw, by + bh);
        }
    }

    private void layoutHand() {
        handCards.clear();
        handRects.clear();
        if (game == null) return;
        List<Card> hand = game.hands[Game.ME];
        int n = hand.size();
        if (n == 0) return;

        float maxRow = w - dp * 20;
        float cw = cardW;
        float ch = cardH;
        // keep every rank visible: never let the exposed sliver fall below 30% of a card
        if (n > 1) {
            float fit = maxRow / (1f + 0.30f * (n - 1));
            if (fit < cw) {
                cw = Math.max(fit, dp * 30);
                ch = cw / 0.70f;
                if (ch > cardH) ch = cardH;
            }
        }
        float st = (n > 1) ? (maxRow - cw) / (n - 1) : 0;
        if (st > cw * 0.66f) st = cw * 0.66f;
        float startX = (w - (cw + st * (n - 1))) / 2f;
        float topY = h - dp * 16 - ch;
        handTop = topY;
        for (int i = 0; i < n; i++) {
            float left = startX + st * i;
            handCards.add(hand.get(i));
            handRects.add(new RectF(left, topY, left + cw, topY + ch));
        }
    }

    // ================= drawing =================

    @Override
    protected void onDraw(Canvas c) {
        if (game == null) return;
        layoutHand();
        drawTable(c);
        drawTopBar(c);
        drawPanels(c);
        drawCenter(c);
        drawLog(c);
        drawButtons(c);
        drawHand(c);
        drawTip(c);
    }

    private void drawTable(Canvas c) {
        pFill.setColor(Color.rgb(9, 74, 51));
        c.drawRect(0, 0, w, h, pFill);
        pFill.setColor(Color.rgb(13, 96, 66));
        c.drawOval(new RectF(w * 0.03f, topBarH - dp * 10, w * 0.97f, h * 0.92f), pFill);
        pBorder.setColor(Color.argb(70, 214, 180, 96));
        pBorder.setStrokeWidth(dp * 1.5f);
        c.drawOval(new RectF(w * 0.03f, topBarH - dp * 10, w * 0.97f, h * 0.92f), pBorder);
    }

    private void drawTopBar(Canvas c) {
        String left = (game.roundNo >= 1 && game.roundNo <= 5)
                ? Lang.f("head_round", game.roundNo)
                : Lang.t("game_title");
        pTitle.setTextSize(dp * 17);
        pTitle.setTextAlign(Paint.Align.CENTER);
        c.drawText(left, w / 2f, dp * 24, pTitle);

        String sub;
        if (game.state == Game.ST_PLAY) {
            sub = game.current == Game.ME
                    ? Lang.f("timer_your_turn", remainSec)
                    : Lang.f("timer_bot", game.name(game.current));
        } else {
            sub = Lang.t("round_over");
        }
        pSmall.setTextAlign(Paint.Align.CENTER);
        c.drawText(sub, w / 2f, dp * 44, pSmall);

        // language pill
        pFill.setColor(Color.argb(190, 8, 62, 44));
        c.drawRoundRect(btnLang, dp * 15, dp * 15, pFill);
        pBorder.setColor(Color.argb(170, 140, 210, 170));
        pBorder.setStrokeWidth(dp);
        c.drawRoundRect(btnLang, dp * 15, dp * 15, pBorder);
        pBold.setTextAlign(Paint.Align.CENTER);
        pBold.setTextSize(dp * 10.5f);
        pBold.setColor(Color.rgb(255, 226, 160));
        c.drawText(ellipsize(pBold, Lang.t("btn_language"), btnLang.width() - dp * 10),
                btnLang.centerX(), btnLang.centerY() + dp * 3.5f, pBold);
    }

    private void drawPanels(Canvas c) {
        for (int seat = 1; seat <= 2; seat++) {
            RectF r = (seat == 1) ? rLeftPanel : rRightPanel;
            panel(c, r, seat);
        }
    }

    private void panel(Canvas c, RectF r, int seat) {
        pFill.setColor(Color.argb(150, 4, 40, 27));
        c.drawRoundRect(r, dp * 8, dp * 8, pFill);
        pBorder.setColor(Color.argb(110, 120, 200, 150));
        pBorder.setStrokeWidth(dp);
        c.drawRoundRect(r, dp * 8, dp * 8, pBorder);

        boolean rtl = Lang.isRtl();
        float left = rtl ? r.right - dp * 10 : r.left + dp * 10;
        pText.setTextAlign(rtl ? Paint.Align.RIGHT : Paint.Align.LEFT);
        pText.setTextSize(dp * 14);
        c.drawText(game.name(seat), left, r.top + dp * 20, pText);

        pSmall.setTextAlign(rtl ? Paint.Align.RIGHT : Paint.Align.LEFT);
        String act = actionText(game.lastAction[seat]);
        if (act.length() > 0) c.drawText(act, left, r.top + dp * 38, pSmall);

        pBold.setTextAlign(rtl ? Paint.Align.RIGHT : Paint.Align.LEFT);
        pBold.setTextSize(dp * 16);
        c.drawText(Lang.f("remain", game.hands[seat].size()), left, r.bottom - dp * 8, pBold);

        // card back emblem on the opposite side of the panel
        RectF back = new RectF(rtl ? r.left + dp * 8 : r.right - dp * 34,
                r.top + dp * 14, rtl ? r.left + dp * 42 : r.right - dp * 8, r.bottom - dp * 10);
        drawCardBack(c, back);
    }

    private String actionText(String code) {
        if (code == null) return "";
        if (code.equals("pass")) return Lang.t("action_pass");
        if (code.equals("done")) return Lang.t("action_done");
        return Lang.t("action_play");
    }

    private void drawCenter(Canvas c) {
        if (game.state != Game.ST_PLAY) return;
        float cx = w / 2f;
        String label;
        if (game.lastPlay == null) {
            label = Lang.f("all_pass", game.name(game.current));
            pBody.setTextSize(dp * 13);
            pBody.setTextAlign(Paint.Align.CENTER);
            c.drawText(ellipsize(pBody, label, w * 0.55f), cx, rCenter.centerY(), pBody);
            return;
        }
        label = Lang.f("center_play", game.name(game.lastPlayer), game.lastPlay.typeName());
        pBody.setTextSize(dp * 13);
        pBody.setTextAlign(Paint.Align.CENTER);
        c.drawText(ellipsize(pBody, label, w * 0.55f), cx, rCenter.top + dp * 18, pBody);

        List<Card> cards = game.lastPlay.cards;
        float ch = Math.min(dp * 92, rCenter.height() - dp * 32);
        float cw = ch * 0.70f;
        float gap = dp * 8;
        float total = cw * cards.size() + gap * (cards.size() - 1);
        float x = cx - total / 2f;
        float y = rCenter.top + dp * 28;
        for (Card card : cards) {
            drawCard(c, new RectF(x, y, x + cw, y + ch), card, false);
            x += cw + gap;
        }
    }

    private void drawLog(Canvas c) {
        int from = Math.max(0, game.log.size() - 6);
        float y = rLog.top + dp * 14;
        pSmall.setTextSize(dp * 10.5f);
        pSmall.setTextAlign(Lang.isRtl() ? Paint.Align.RIGHT : Paint.Align.LEFT);
        float x = Lang.isRtl() ? rLog.right : rLog.left;
        for (int i = from; i < game.log.size(); i++) {
            c.drawText(ellipsize(pSmall, game.log.get(i), rLog.width()), x, y, pSmall);
            y += dp * 15;
            if (y > rLog.bottom) break;
        }
    }

    private void drawButtons(Canvas c) {
        boolean canPlay = game.state == Game.ST_PLAY && game.current == Game.ME && game.hands[Game.ME].size() > 0;
        boolean canPass = game.state == Game.ST_PLAY && game.current == Game.ME && game.lastPlay != null && game.lastPlayer != Game.ME;
        button(c, btnPlay, Lang.t("btn_play"), canPlay);
        button(c, btnPass, Lang.t("btn_pass"), canPass);
        button(c, btnMenu, Lang.t("menu"), true);
    }

    private void button(Canvas c, RectF r, String text, boolean enabled) {
        pFill.setColor(enabled ? Color.argb(235, 16, 92, 62) : Color.argb(150, 30, 60, 46));
        c.drawRoundRect(r, dp * 7, dp * 7, pFill);
        pBorder.setColor(enabled ? Color.argb(200, 130, 214, 160) : Color.argb(90, 120, 160, 140));
        pBorder.setStrokeWidth(dp);
        c.drawRoundRect(r, dp * 7, dp * 7, pBorder);
        pBold.setTextAlign(Paint.Align.CENTER);
        pBold.setTextSize(dp * 13);
        pBold.setColor(enabled ? Color.rgb(255, 226, 160) : Color.rgb(150, 175, 160));
        c.drawText(ellipsize(pBold, text, r.width() - dp * 12), r.centerX(),
                r.centerY() + dp * 4.5f, pBold);
        pBold.setColor(Color.rgb(255, 226, 160));
    }

    private void drawHand(Canvas c) {
        if (handCards.isEmpty()) return;
        for (int i = 0; i < handCards.size(); i++) {
            Card card = handCards.get(i);
            boolean sel = selected.contains(card);
            RectF r = handRects.get(i);
            RectF draw = sel ? new RectF(r.left, r.top - dp * 16, r.right, r.bottom - dp * 16) : r;
            drawCard(c, draw, card, sel);
        }
    }

    private void drawTip(Canvas c) {
        if (tip.length() == 0) return;
        pFill.setColor(Color.argb(210, 4, 40, 27));
        RectF box = new RectF(w * 0.2f, cardTop - dp * 104, w * 0.8f, cardTop - dp * 72);
        c.drawRoundRect(box, dp * 10, dp * 10, pFill);
        pBody.setTextSize(dp * 12);
        pBody.setTextAlign(Paint.Align.CENTER);
        c.drawText(ellipsize(pBody, tip, box.width() - dp * 12), box.centerX(), box.centerY() + dp * 4, pBody);
    }

    // ================= card art =================

    private void drawCardBack(Canvas c, RectF r) {
        float rad = r.height() * 0.10f;
        pShadow.setColor(Color.argb(60, 0, 0, 0));
        c.drawRoundRect(new RectF(r.left + 2, r.top + 3, r.right + 2, r.bottom + 3), rad, rad, pShadow);
        pFill.setColor(Color.rgb(122, 31, 38));
        c.drawRoundRect(r, rad, rad, pFill);
        pBorder.setColor(Color.argb(160, 235, 210, 150));
        pBorder.setStrokeWidth(dp * 1.2f);
        c.drawRoundRect(new RectF(r.left + dp * 3, r.top + dp * 3, r.right - dp * 3, r.bottom - dp * 3), rad * 0.8f, rad * 0.8f, pBorder);
        pTitle.setTextAlign(Paint.Align.CENTER);
        pTitle.setTextSize(r.height() * 0.30f);
        c.drawText("Z", r.centerX(), r.centerY() + r.height() * 0.12f, pTitle);
        pTitle.setTextAlign(Paint.Align.CENTER);
        pTitle.setTextSize(dp * 17);
    }

    private void drawCard(Canvas c, RectF r, Card card, boolean sel) {
        float rad = r.height() * 0.09f;
        pShadow.setColor(Color.argb(70, 0, 0, 0));
        c.drawRoundRect(new RectF(r.left + dp * 1.5f, r.top + dp * 3, r.right + dp * 1.5f, r.bottom + dp * 3), rad, rad, pShadow);

        pFill.setColor(Color.rgb(253, 249, 238));
        c.drawRoundRect(r, rad, rad, pFill);
        pBorder.setColor(sel ? Color.rgb(255, 200, 80) : Color.rgb(190, 176, 152));
        pBorder.setStrokeWidth(sel ? dp * 2.2f : dp * 0.8f);
        c.drawRoundRect(r, rad, rad, pBorder);

        int ink = card.isRed() ? Color.rgb(198, 40, 40) : Color.rgb(32, 32, 32);
        pCardRank.setColor(ink);
        pCardSuit.setColor(ink);

        float small = r.height() * 0.20f;
        pCardRank.setTextSize(small);
        pCardRank.setTextAlign(Paint.Align.LEFT);
        c.drawText(card.rankText(), r.left + r.width() * 0.08f, r.top + small * 0.95f, pCardRank);

        pCardSuit.setTextSize(r.height() * 0.20f);
        pCardSuit.setTextAlign(Paint.Align.LEFT);
        String glyph = card.isJoker() ? "★" : card.suitGlyph();
        c.drawText(glyph, r.left + r.width() * 0.08f, r.top + small * 0.95f + r.height() * 0.20f, pCardSuit);

        pCardSuit.setTextSize(r.height() * 0.42f);
        pCardSuit.setTextAlign(Paint.Align.CENTER);
        String big = card.isJoker() ? "★" : card.suitGlyph();
        c.drawText(big, r.centerX(), r.centerY() + r.height() * 0.16f, pCardSuit);

        if (card.isJoker()) {
            pCardRank.setTextSize(r.height() * 0.14f);
            pCardRank.setTextAlign(Paint.Align.CENTER);
            c.drawText(card.rankText(), r.centerX(), r.bottom - r.height() * 0.08f, pCardRank);
        }
    }

    private String ellipsize(Paint p, String text, float maxW) {
        if (text == null) return "";
        if (p.measureText(text) <= maxW) return text;
        return TextUtils.ellipsize(text, new android.text.TextPaint(p), maxW, TextUtils.TruncateAt.END).toString();
    }

    // ================= touch =================

    @Override
    public boolean onTouchEvent(MotionEvent e) {
        if (e.getAction() != MotionEvent.ACTION_DOWN) return true;
        float x = e.getX(), y = e.getY();

        if (btnMenu.contains(x, y)) { showMenu(); return true; }
        if (btnLang.contains(x, y)) { showLanguage(false); return true; }

        if (game.state == Game.ST_PLAY && game.current == Game.ME) {
            if (btnPass.contains(x, y) && game.lastPlay != null && game.lastPlayer != Game.ME) {
                game.humanPass();
                selected.clear();
                invalidate();
                schedule();
                return true;
            }
            if (btnPlay.contains(x, y)) {
                doPlay();
                return true;
            }
        }

        // top-most card first: later cards are drawn over earlier ones
        for (int i = handRects.size() - 1; i >= 0; i--) {
            if (handRects.get(i).contains(x, y)) {
                Card card = handCards.get(i);
                if (selected.contains(card)) selected.remove(card);
                else selected.add(card);
                invalidate();
                return true;
            }
        }
        return true;
    }

    private void doPlay() {
        String err = game.humanPlay(new ArrayList<Card>(selected));
        if (err != null) {
            showTip(Lang.t(err));
            return;
        }
        selected.clear();
        newTurn();
        invalidate();
    }
}
