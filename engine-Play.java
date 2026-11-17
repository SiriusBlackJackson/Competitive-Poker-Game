package com.zhengshengyou.engine;

import java.util.ArrayList;
import java.util.List;

/** A legal combination: single, pair, triple (last three cards only) or triple + 1. */
public class Play {
    public static final int SINGLE = 0, PAIR = 1, TRIPLE = 2, TRIPLE_ONE = 3;

    public final int type;
    public final List<Card> cards;
    public final int key; // rank used for comparison; for triple+1 it is the triple's rank

    public Play(int type, List<Card> cards, int key) {
        this.type = type;
        this.cards = cards;
        this.key = key;
    }

    public String typeName() {
        switch (type) {
            case SINGLE: return Lang.t("type_single");
            case PAIR: return Lang.t("type_pair");
            case TRIPLE: return Lang.t("type_triple");
            default: return Lang.t("type_triple1");
        }
    }

    public String text() {
        List<String> parts = new ArrayList<String>();
        for (Card c : cards) parts.add(c.display());
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < parts.size(); i++) {
            if (i > 0) sb.append(' ');
            sb.append(parts.get(i));
        }
        return sb.toString();
    }
}
