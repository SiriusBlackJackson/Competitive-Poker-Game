package com.zhengshengyou.engine;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashSet;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Random;
import java.util.Set;

/**
 * Rules engine for Zheng Shang You: dealing, turns, AI, places, tribute and requests.
 * It never talks to the UI directly - everything the screen has to show is either a field
 * on this object or a {@link Callbacks} call, so the same engine drives the Android table
 * and the headless self test.
 */
public class Game {

    public static final int ME = 0;
    public static final int N = 3;

    public static final int ST_PLAY = 1, ST_TRADES = 2, ST_ROUND_END = 3, ST_MATCH_END = 4;

    public interface Callbacks {
        void onChanged();
        void onAskRank(int giverSeat, int askerSeat, List<Integer> candidates, int refusals);
        void onAskGive(int askerSeat, int rank, int refusals);
        void onTradesFinished();
        void onRoundEnd(int[] ranks, boolean lastRound);
        void onMatchEnd(List<int[]> allRanks);
    }

    public final List<Card>[] hands;
    public final List<int[]> roundRanks = new ArrayList<int[]>();
    public final List<String> log = new ArrayList<String>();

    public int roundNo;
    public int state = ST_PLAY;
    public int current = 0;
    public Play lastPlay;
    public int lastPlayer = -1;
    public Play centerPlay;
    public int centerSeat = -1;
    public boolean roundFirstPlay = true;
    public final String[] lastAction = new String[N];
    public int[] prevRoles = new int[] { -1, -1, -1 };
    public String tip = "";

    private final int[] finishOrder = new int[N];
    private int finishCount;
    private int passCount;
    private int playLeader;
    private final Random rnd;

    private Callbacks cb;
    public boolean autoPilot;   // used by the self test: the human seat also plays by AI
    public boolean headless;

    // request (second trade step) state
    private int reqPair;
    private int reqAsker, reqGiver, reqForbid, reqRefusals;
    private final Set<Integer> reqAsked = new HashSet<Integer>();
    private boolean awaitingRank, awaitingGive;
    private int pendingWant = -1;
    private Card tributeDownCard, tributeMidCard;

    public boolean firstPlayChecked;
    public boolean firstPlayHadH4;
    public int lastDealTotal;
    public int lastTradeTotal;

    public Game(long seed) {
        rnd = new Random(seed);
        hands = new List[N];
        for (int i = 0; i < N; i++) hands[i] = new ArrayList<Card>();
    }

    public void setCallbacks(Callbacks c) { cb = c; }

    public String name(int seat) {
        return Lang.t(seat == 0 ? "name_you" : (seat == 1 ? "name_bot1" : "name_bot2"));
    }

    public int[] handCounts() {
        return new int[] { hands[0].size(), hands[1].size(), hands[2].size() };
    }

    // ================= match / round flow =================

    public void newMatch() {
        roundNo = 0;
        prevRoles = new int[] { -1, -1, -1 };
        roundRanks.clear();
        log.clear();
        state = ST_PLAY;
        nextRound();
    }

    public void nextRound() {
        roundNo++;
        state = ST_PLAY;
        finishCount = 0;
        lastPlay = null;
        lastPlayer = -1;
        passCount = 0;
        centerPlay = null;
        centerSeat = -1;
        tip = "";
        for (int i = 0; i < N; i++) lastAction[i] = null;

        deal();
        int leader = (roundNo == 1) ? findHeart4() : prevRoles[0];
        if (leader < 0) leader = 0;
        playLeader = leader;
        current = leader;
        roundFirstPlay = true;

        log.clear();
        if (roundNo > 1) {
            addLog(Lang.f("log_trade_header", roundNo));
            beginTrades();
        } else {
            addLog(Lang.f("log_h4_leader", name(leader)));
            notifyChanged();
        }
    }

    private void deal() {
        List<Card> deck = Card.buildDeck();
        Card.shuffle(deck, rnd);
        for (int i = 0; i < N; i++) {
            hands[i] = new ArrayList<Card>();
            for (int k = 0; k < 18; k++) hands[i].add(deck.get(i * 18 + k));
            Collections.sort(hands[i], Card.DESC);
        }
        lastDealTotal = totalCards();
    }

    private int findHeart4() {
        for (int i = 0; i < N; i++)
            for (Card c : hands[i])
                if (c.rank == 4 && c.suit == Card.HEART) return i;
        return 0;
    }

    // ================= step 1: tribute =================

    private void beginTrades() {
        state = ST_TRADES;
        int up = prevRoles[0], mid = prevRoles[1], down = prevRoles[2];
        tributeDownCard = null;
        tributeMidCard = null;

        if (up >= 0 && mid >= 0 && down >= 0) {
            // both cards are taken from the hands as they were before any tribute moved
            tributeDownCard = biggest(hands[down]);
            tributeMidCard = biggest(hands[mid]);
            if (tributeDownCard != null) {
                move(hands[down], hands[mid], tributeDownCard);
                addLog(Lang.f("log_tribute1", name(down), tributeDownCard.display(), name(mid)));
            }
            if (tributeMidCard != null) {
                move(hands[mid], hands[up], tributeMidCard);
                addLog(Lang.f("log_tribute2", name(mid), tributeMidCard.display(), name(up)));
            }
        }

        for (int i = 0; i < N; i++) Collections.sort(hands[i], Card.DESC);
        notifyChanged();

        reqPair = 0;
        startRequestPair();
    }

    public int forbidRankOf(int pair) {
        if (pair == 0) return tributeMidCard != null ? tributeMidCard.rank : -1;
        return tributeDownCard != null ? tributeDownCard.rank : -1;
    }

    private void startRequestPair() {
        int up = prevRoles[0], mid = prevRoles[1], down = prevRoles[2];
        if (reqPair == 0) {
            reqAsker = mid; reqGiver = up; reqForbid = forbidRankOf(0);
        } else if (reqPair == 1) {
            reqAsker = down; reqGiver = mid; reqForbid = forbidRankOf(1);
        } else {
            tradesDone();
            return;
        }
        reqRefusals = 0;
        reqAsked.clear();
        pendingWant = -1;
        if (reqAsker < 0 || reqGiver < 0) { reqPair++; startRequestPair(); return; }
        addLog(Lang.f("log_ask", name(reqAsker), name(reqGiver)));
        requestLoop();
    }

    private void nextPair() {
        reqPair++;
        startRequestPair();
    }

    /** Runs the request loop until it needs the human, or both pairs are done. */
    private void requestLoop() {
        while (true) {
            List<Integer> cands = requestCandidates(hands[reqAsker], hands[reqGiver], reqForbid);
            cands.removeAll(reqAsked);
            if (cands.isEmpty()) {
                addLog(Lang.f("log_no_idea", name(reqAsker)));
                nextPair();
                return;
            }
            int want;
            if (reqAsker == ME && !autoPilot) {
                awaitingRank = true;
                notifyChanged();
                cb.onAskRank(reqGiver, reqAsker, cands, reqRefusals);
                return;
            }
            want = cands.get(0);
            reqAsked.add(want);
            resolveWant(want);
            return;
        }
    }

    private void resolveWant(int want) {
        if (countRank(hands[reqGiver], want) == 0) {
            addLog(Lang.f("log_want_absent", name(reqAsker), Card.rankTextOf(want), name(reqGiver)));
            requestLoop();
            return;
        }
        if (reqGiver == ME && !autoPilot) {
            pendingWant = want;
            awaitingGive = true;
            notifyChanged();
            cb.onAskGive(reqAsker, want, reqRefusals);
            return;
        }
        applyGiveDecision(want, botDecideGive(hands[reqGiver], want));
    }

    private void applyGiveDecision(int want, boolean give) {
        if (give) {
            Card card = firstOfRank(hands[reqGiver], want);
            if (card != null) {
                move(hands[reqGiver], hands[reqAsker], card);
                addLog(Lang.f("log_gave", name(reqGiver), name(reqAsker), card.display()));
            }
            nextPair();
            return;
        }
        reqRefusals++;
        addLog(Lang.f("log_refuse", name(reqGiver), reqRefusals));
        if (reqRefusals >= 3) {
            if (!hands[reqGiver].isEmpty()) {
                Card rc = hands[reqGiver].get(rnd.nextInt(hands[reqGiver].size()));
                move(hands[reqGiver], hands[reqAsker], rc);
                addLog(Lang.f("log_force_give", name(reqGiver), name(reqAsker), rc.display()));
            }
            nextPair();
            return;
        }
        requestLoop();
    }

    // --- UI entry points for the request step ---

    public boolean awaitingRankChoice() { return awaitingRank; }
    public boolean awaitingGive() { return awaitingGive; }
    public int pendingRank() { return pendingWant; }

    public void chooseRank(int rank) {
        if (!awaitingRank) return;
        awaitingRank = false;
        reqAsked.add(rank);
        resolveWant(rank);
    }

    public void giveUpRequest() {
        if (!awaitingRank) return;
        awaitingRank = false;
        addLog(Lang.t("log_you_gaveup"));
        nextPair();
    }

    public void decideGive(boolean give) {
        if (!awaitingGive) return;
        awaitingGive = false;
        applyGiveDecision(pendingWant, give);
    }

    private void tradesDone() {
        for (int i = 0; i < N; i++) Collections.sort(hands[i], Card.DESC);
        lastTradeTotal = totalCards();
        notifyChanged();
        if (headless) {
            startPlay();
            return;
        }
        cb.onTradesFinished();
    }

    /** Called by the UI after the trade summary has been dismissed. */
    public void startPlay() {
        state = ST_PLAY;
        current = playLeader;
        roundFirstPlay = true;
        notifyChanged();
    }

    // ================= play =================

    /** @return null when the play was accepted, otherwise the key of the message to show. */
    public String humanPlay(List<Card> sel) {
        if (state != ST_PLAY || current != ME) return "tip_bad_type";
        if (sel == null || sel.isEmpty()) return "tip_select";

        Play p = PlayRules.parse(sel, hands[ME].size());
        if (p == null) return "tip_bad_type";
        if (lastPlay != null && lastPlayer != ME && !PlayRules.beats(p, lastPlay)) return "tip_must_beat";
        if (roundFirstPlay && roundNo == 1 && !containsHeart4(sel)) return "tip_need_h4";

        applyPlay(ME, p);
        return null;
    }

    public String humanPass() {
        if (state != ST_PLAY || current != ME) return "tip_cant_pass";
        if (lastPlay == null) return "tip_cant_pass";
        doPass(ME);
        return null;
    }

    /** Timeout: pass when following, play the smallest legal lead when leading. */
    public void autoHuman() {
        if (state != ST_PLAY || current != ME) return;
        if (lastPlay == null) {
            Play p = chooseLead(hands[ME], roundFirstPlay && roundNo == 1);
            if (p != null) applyPlay(ME, p);
            return;
        }
        doPass(ME);
    }

    public void botTurn() {
        if (state != ST_PLAY) return;
        if (current == ME && !autoPilot) return;
        int seat = current;
        Play choice = (lastPlay == null)
                ? chooseLead(hands[seat], roundFirstPlay && roundNo == 1)
                : chooseFollow(hands[seat], lastPlay);
        if (choice == null) doPass(seat);
        else applyPlay(seat, choice);
    }

    public void applyPlay(int seat, Play p) {
        if (p == null) return;
        if (roundFirstPlay && roundNo == 1) {
            firstPlayChecked = true;
            firstPlayHadH4 = containsHeart4(p.cards);
        }
        for (Card c : p.cards) hands[seat].remove(c);
        centerPlay = p;
        centerSeat = seat;
        lastAction[seat] = "play";
        lastPlay = p;
        lastPlayer = seat;
        passCount = 0;
        roundFirstPlay = false;
        addLog(Lang.f("log_play", name(seat), p.typeName(), p.text()));

        int next = nextActive(seat);
        if (hands[seat].isEmpty()) {
            finishOrder[finishCount++] = seat;
            lastAction[seat] = "done";
            lastPlay = null;
            lastPlayer = -1;
            passCount = 0;
            addLog(Lang.f("log_finish", name(seat),
                    finishCount == 1 ? Lang.t("finish_up") : Lang.t("finish_mid")));
            if (finishCount == 2) {
                endRound();
                return;
            }
        }
        current = next;
        notifyChanged();
    }

    private void doPass(int seat) {
        passCount++;
        lastAction[seat] = "pass";
        addLog(Lang.f("log_pass", name(seat)));
        int active = activeCount();
        if (passCount >= active - 1) {
            lastPlay = null;
            lastPlayer = -1;
            passCount = 0;
        }
        current = nextActive(seat);
        notifyChanged();
    }

    public Play chooseLead(List<Card> hand, boolean mustH4) {
        List<Play> plays = PlayRules.allPlays(hand);
        if (mustH4) {
            List<Play> keep = new ArrayList<Play>();
            for (Play p : plays) if (containsHeart4(p.cards)) keep.add(p);
            plays = keep;
        }
        if (plays.isEmpty()) return null;
        Collections.sort(plays, PLAY_LOW_FIRST);
        return plays.get(0);
    }

    public Play chooseFollow(List<Card> hand, Play last) {
        List<Play> cands = new ArrayList<Play>();
        for (Play p : PlayRules.allPlays(hand))
            if (p.type == last.type && PlayRules.beats(p, last)) cands.add(p);
        if (cands.isEmpty()) return null;
        Collections.sort(cands, PLAY_LOW_FIRST);
        return cands.get(0);
    }

    private static final Comparator<Play> PLAY_LOW_FIRST = new Comparator<Play>() {
        @Override public int compare(Play a, Play b) {
            int cmp = Integer.compare(Card.gameValue(a.key), Card.gameValue(b.key));
            if (cmp != 0) return cmp;
            return Integer.compare(b.cards.size(), a.cards.size());
        }
    };

    private int nextActive(int from) {
        for (int k = 1; k <= N; k++) {
            int s = (from + k) % N;
            if (!hands[s].isEmpty()) return s;
        }
        return from;
    }

    private int activeCount() {
        int n = 0;
        for (int i = 0; i < N; i++) if (!hands[i].isEmpty()) n++;
        return n;
    }

    // ================= round end =================

    private void endRound() {
        state = ST_ROUND_END;
        int remaining = -1;
        for (int i = 0; i < N; i++) if (!hands[i].isEmpty()) remaining = i;
        int[] ranks = new int[] { finishOrder[0], finishOrder[1], remaining };
        prevRoles = new int[] { ranks[0], ranks[1], ranks[2] };
        roundRanks.add(ranks);
        addLog(Lang.f("log_round_result", roundNo, name(ranks[0]), name(ranks[1]), name(ranks[2])));
        notifyChanged();

        boolean last = roundNo >= 5;
        if (last) state = ST_MATCH_END;
        if (headless) {
            if (!last) nextRound();
            return;
        }
        cb.onRoundEnd(ranks, last);
        if (last) cb.onMatchEnd(roundRanks);
    }

    public boolean matchOver() { return state == ST_MATCH_END; }

    public int totalCards() {
        return hands[0].size() + hands[1].size() + hands[2].size();
    }

    // ================= helpers =================

    public List<Integer> requestCandidates(List<Card> askerHand, List<Card> giverHand, int forbidRank) {
        List<Integer> res = new ArrayList<Integer>();
        if (giverHand.isEmpty()) return res;

        Card top = biggest(giverHand);
        int topRank = top != null ? top.rank : -1;

        Map<Integer, Integer> mine = new LinkedHashMap<Integer, Integer>();
        for (Card c : askerHand) {
            Integer n = mine.get(c.rank);
            mine.put(c.rank, n == null ? 1 : n + 1);
        }
        for (Card c : giverHand) {
            if (c.isJoker()) continue;
            if (c.rank == forbidRank) continue;
            if (c.rank == topRank) continue;
            Integer cntObj = mine.get(c.rank);
            int cnt = cntObj == null ? 0 : cntObj;
            if ((cnt == 1 || cnt == 2) && !res.contains(c.rank)) res.add(c.rank);
        }
        Collections.sort(res, new Comparator<Integer>() {
            @Override public int compare(Integer a, Integer b) { return Integer.compare(Card.gameValue(b), Card.gameValue(a)); }
        });
        return res;
    }

    public boolean botDecideGive(List<Card> hand, int rank) {
        if (countRank(hand, rank) >= 2) return true;
        return Card.gameValue(rank) <= 8;
    }

    private int countRank(List<Card> hand, int rank) {
        int n = 0;
        for (Card c : hand) if (c.rank == rank) n++;
        return n;
    }

    private Card firstOfRank(List<Card> hand, int rank) {
        for (Card c : hand) if (c.rank == rank) return c;
        return null;
    }

    public Card biggest(List<Card> hand) {
        Card best = null;
        for (Card c : hand)
            if (best == null || c.value() > best.value()) best = c;
        return best;
    }

    private void move(List<Card> from, List<Card> to, Card c) {
        from.remove(c);
        to.add(c);
        Collections.sort(to, Card.DESC);
    }

    public static boolean containsHeart4(List<Card> cards) {
        for (Card c : cards) if (c.rank == 4 && c.suit == Card.HEART) return true;
        return false;
    }

    private void addLog(String s) {
        log.add(s);
        while (log.size() > 40) log.remove(0);
    }

    private void notifyChanged() {
        if (cb != null && !headless) cb.onChanged();
    }
}
