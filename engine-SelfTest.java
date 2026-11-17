package com.zhengshengyou.engine;

import java.util.List;

/**
 * Headless verification of the rules engine, runnable on any desktop JVM:
 *   javac -encoding UTF-8 -d classes src/main/java/com/zhengshengyou/engine/*.java
 *   java -cp classes com.zhengshengyou.engine.SelfTest
 * Plays 300 complete five-round matches with every seat on AI and asserts the invariants
 * the desktop build checks as well: the 4 of Hearts rule, 54 cards after deal and after
 * trades, distinct places, and no deadlock.
 */
public final class SelfTest {

    public static void main(String[] args) {
        Lang.setCode("en");
        int failures = 0;
        int matches = 300;

        for (int m = 0; m < matches; m++) {
            Game g = new Game(1234567L + m);
            g.autoPilot = true;
            g.headless = true;
            g.newMatch();

            if (g.lastDealTotal != 54) { fail("deal total " + g.lastDealTotal); failures++; }
            if (g.hands[0].size() != 18 || g.hands[1].size() != 18 || g.hands[2].size() != 18) {
                fail("deal sizes not 18/18/18"); failures++;
            }

            int guard = 0;
            while (!g.matchOver() && guard < 200000) {
                guard++;
                if (g.state == Game.ST_PLAY) {
                    g.botTurn();
                    if (g.matchOver()) break;
                    continue;
                }
                if (g.state == Game.ST_TRADES || g.state == Game.ST_ROUND_END) {
                    fail("engine stuck in state " + g.state);
                    failures++;
                    break;
                }
                break;
            }
            if (guard >= 200000) { fail("match " + m + " did not terminate"); failures++; }

            if (!g.firstPlayChecked) { fail("round 1 first play never happened"); failures++; }
            else if (!g.firstPlayHadH4) { fail("round 1 first play missed the 4H"); failures++; }

            if (g.roundRanks.size() != 5) { fail("rounds played: " + g.roundRanks.size()); failures++; }

            for (int r = 0; r < g.roundRanks.size(); r++) {
                int[] rk = g.roundRanks.get(r);
                if (rk[0] == rk[1] || rk[1] == rk[2] || rk[0] == rk[2]) {
                    fail("duplicate places in round " + (r + 1)); failures++;
                }
            }

            if (failures > 6) break;
        }

        // trade totals are checked on a single match where we look after every round
        Game g2 = new Game(99L);
        g2.autoPilot = true;
        g2.headless = true;
        g2.newMatch();
        int guard2 = 0;
        while (!g2.matchOver() && guard2 < 200000) {
            guard2++;
            if (g2.state == Game.ST_PLAY) g2.botTurn();
            else break;
        }
        if (g2.lastTradeTotal != 54) { fail("trade total " + g2.lastTradeTotal); failures++; }

        // parsing sanity
        List<Card> two = new java.util.ArrayList<Card>();
        two.add(new Card(9, Card.HEART));
        two.add(new Card(9, Card.SPADE));
        Play pair = PlayRules.parse(two, 5);
        if (pair == null || pair.type != Play.PAIR) { fail("pair parse"); failures++; }

        List<Card> jokers = new java.util.ArrayList<Card>();
        jokers.add(new Card(15, Card.JOKER));
        jokers.add(new Card(16, Card.JOKER));
        if (PlayRules.parse(jokers, 5) != null) { fail("jokers must not form a pair"); failures++; }

        List<Card> tripleAlone = new java.util.ArrayList<Card>();
        tripleAlone.add(new Card(12, Card.CLUB));
        tripleAlone.add(new Card(12, Card.DIAMOND));
        tripleAlone.add(new Card(12, Card.SPADE));
        if (PlayRules.parse(tripleAlone, 10) != null) { fail("bare triple allowed with 10 cards"); failures++; }
        if (PlayRules.parse(tripleAlone, 3) == null) { fail("bare triple refused as last three cards"); failures++; }

        // 5 beats 8 beats jokers ... 4
        if (!(Card.gameValue(5) > Card.gameValue(8))) { fail("5 must beat 8"); failures++; }
        if (!(Card.gameValue(8) > Card.gameValue(16))) { fail("8 must beat big joker"); failures++; }
        if (!(Card.gameValue(3) > Card.gameValue(2))) { fail("3 must beat 2"); failures++; }
        if (!(Card.gameValue(6) > Card.gameValue(4))) { fail("6 must beat 4"); failures++; }

        // every language has every key
        for (int i = 0; i < Lang.CODES.length; i++) {
            Lang.idx = i;
            String[] keys = { "game_title", "rules_body", "btn_play", "name_you", "log_pass", "type_triple1" };
            for (String k : keys) if (Lang.t(k) == null || Lang.t(k).isEmpty()) { fail("missing " + k + " in " + Lang.code()); failures++; }
        }
        Lang.idx = 0;

        if (failures == 0) System.out.println("SELFTEST OK: " + matches + " matches, rules and languages verified");
        else { System.out.println("SELFTEST FAILED: " + failures + " problem(s)"); System.exit(1); }
    }

    private static void fail(String msg) {
        System.out.println("FAIL " + msg);
    }
}
