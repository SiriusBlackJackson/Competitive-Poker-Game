package com.zhengshengyou.engine;

/** One card of the 54-card deck. rank 2..14 = 2..A, 15 = small joker, 16 = big joker. */
public class Card {
    public static final int SPADE = 0, HEART = 1, CLUB = 2, DIAMOND = 3, JOKER = 4;

    public final int rank;
    public final int suit;

    public Card(int rank, int suit) {
        this.rank = rank;
        this.suit = suit;
    }

    public boolean isJoker() { return suit == JOKER; }

    public boolean isRed() {
        return suit == HEART || suit == DIAMOND || (suit == JOKER && rank == 16);
    }

    public int value() { return gameValue(rank); }

    /** 5 > 8 > big joker > small joker > 3 > 2 > A > K > Q > J > 10 > 9 > 7 > 6 > 4 */
    public static int gameValue(int rank) {
        switch (rank) {
            case 5: return 15;
            case 8: return 14;
            case 16: return 13;
            case 15: return 12;
            case 3: return 11;
            case 2: return 10;
            case 14: return 9;
            case 13: return 8;
            case 12: return 7;
            case 11: return 6;
            case 10: return 5;
            case 9: return 4;
            case 7: return 3;
            case 6: return 2;
            default: return 1;
        }
    }

    public String rankText() {
        if (rank == 16) return Lang.t("rank_big_joker");
        if (rank == 15) return Lang.t("rank_small_joker");
        return rankTextOf(rank);
    }

    public static String rankTextOf(int rank) {
        if (rank == 16) return Lang.t("rank_big_joker");
        if (rank == 15) return Lang.t("rank_small_joker");
        if (rank == 14) return "A";
        if (rank == 13) return "K";
        if (rank == 12) return "Q";
        if (rank == 11) return "J";
        return Integer.toString(rank);
    }

    public String suitGlyph() {
        switch (suit) {
            case SPADE: return "♠";
            case HEART: return "♥";
            case CLUB: return "♣";
            case DIAMOND: return "♦";
            default: return "";
        }
    }

    public String display() {
        return isJoker() ? rankText() : rankText() + suitGlyph();
    }

    @Override
    public String toString() { return display(); }

    public static final java.util.Comparator<Card> DESC = new java.util.Comparator<Card>() {
        @Override public int compare(Card a, Card b) {
            if (a.value() != b.value()) return Integer.compare(b.value(), a.value());
            return Integer.compare(a.suit, b.suit);
        }
    };

    public static java.util.List<Card> buildDeck() {
        java.util.List<Card> deck = new java.util.ArrayList<Card>();
        for (int s = 0; s < 4; s++)
            for (int r = 2; r <= 14; r++)
                deck.add(new Card(r, s));
        deck.add(new Card(15, JOKER));
        deck.add(new Card(16, JOKER));
        return deck;
    }

    public static void shuffle(java.util.List<Card> deck, java.util.Random rnd) {
        for (int i = deck.size() - 1; i > 0; i--) {
            int j = rnd.nextInt(i + 1);
            Card t = deck.get(i);
            deck.set(i, deck.get(j));
            deck.set(j, t);
        }
    }
}
