package com.zhengshengyou.engine;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

/** Validation and comparison rules. Mirrors the desktop engine one-to-one. */
public final class PlayRules {

    private PlayRules() { }

    /** Parses a selection; returns null when it is not a legal combination. */
    public static Play parse(List<Card> sel, int handCount) {
        if (sel == null || sel.isEmpty()) return null;

        Map<Integer, List<Card>> groups = group(sel);
        int n = sel.size();

        if (n == 1) return new Play(Play.SINGLE, new ArrayList<Card>(sel), sel.get(0).rank);

        if (n == 2) {
            if (groups.size() == 1 && !sel.get(0).isJoker())
                return new Play(Play.PAIR, new ArrayList<Card>(sel), sel.get(0).rank);
            return null;
        }

        if (n == 3) {
            // a bare triple is only allowed when it is the whole remaining hand
            if (groups.size() == 1 && !sel.get(0).isJoker() && handCount == 3)
                return new Play(Play.TRIPLE, new ArrayList<Card>(sel), sel.get(0).rank);
            return null;
        }

        if (n == 4) {
            if (groups.size() == 2) {
                for (Map.Entry<Integer, List<Card>> e : groups.entrySet()) {
                    if (e.getValue().size() == 3)
                        return new Play(Play.TRIPLE_ONE, new ArrayList<Card>(sel), e.getKey());
                }
            }
            return null;
        }

        return null;
    }

    public static boolean beats(Play a, Play b) {
        if (a == null) return false;
        if (b == null) return true;
        if (a.type != b.type) return false;
        if (a.cards.size() != b.cards.size()) return false;
        return Card.gameValue(a.key) > Card.gameValue(b.key);
    }

    public static List<Play> allPlays(List<Card> hand) {
        List<Play> res = new ArrayList<Play>();
        if (hand == null || hand.isEmpty()) return res;

        Map<Integer, List<Card>> groups = group(hand);
        for (Map.Entry<Integer, List<Card>> e : groups.entrySet()) {
            List<Card> cards = e.getValue();
            int key = e.getKey();

            List<Card> one = new ArrayList<Card>();
            one.add(cards.get(0));
            res.add(new Play(Play.SINGLE, one, key));

            if (cards.size() >= 2 && !cards.get(0).isJoker()) {
                List<Card> two = new ArrayList<Card>();
                two.add(cards.get(0));
                two.add(cards.get(1));
                res.add(new Play(Play.PAIR, two, key));
            }

            if (cards.size() >= 3 && !cards.get(0).isJoker()) {
                List<Card> kickers = new ArrayList<Card>();
                for (Card c : hand) if (c.rank != key) kickers.add(c);
                Collections.sort(kickers, new Comparator<Card>() {
                    @Override public int compare(Card a, Card b) { return Integer.compare(a.value(), b.value()); }
                });
                int k = Math.min(2, kickers.size());
                for (int i = 0; i < k; i++) {
                    List<Card> four = new ArrayList<Card>();
                    four.add(cards.get(0));
                    four.add(cards.get(1));
                    four.add(cards.get(2));
                    four.add(kickers.get(i));
                    res.add(new Play(Play.TRIPLE_ONE, four, key));
                }
            }
        }

        if (hand.size() == 3 && groups.size() == 1 && !hand.get(0).isJoker())
            res.add(new Play(Play.TRIPLE, new ArrayList<Card>(hand), hand.get(0).rank));

        return res;
    }

    public static Map<Integer, List<Card>> group(List<Card> cards) {
        Map<Integer, List<Card>> map = new LinkedHashMap<Integer, List<Card>>();
        for (Card c : cards) {
            List<Card> list = map.get(c.rank);
            if (list == null) {
                list = new ArrayList<Card>();
                map.put(c.rank, list);
            }
            list.add(c);
        }
        return map;
    }
}
