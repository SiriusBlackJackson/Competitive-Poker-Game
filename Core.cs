using System;
using System.Collections.Generic;
using System.Linq;

// ============ 牌 ============
class Card
{
    public int Rank;   // 2..14 = 2..A, 15 = 小王, 16 = 大王
    public int Suit;   // 0 黑桃 1 红桃 2 梅花 3 方块 4 王
    public Card(int r, int s) { Rank = r; Suit = s; }

    public bool IsJoker { get { return Suit == 4; } }
    public bool IsRed { get { return Suit == 1 || Suit == 3 || (Suit == 4 && Rank == 16); } }
    public int Value { get { return GameValue(Rank); } }

    // 牌力：5 > 8 > 大王 > 小王 > 3 > 2 > A > K > Q > J > 10 > 9 > 7 > 6 > 4
    public static int GameValue(int rank)
    {
        switch (rank)
        {
            case 5: return 15;
            case 8: return 14;
            case 16: return 13;  // 大王
            case 15: return 12;  // 小王
            case 3: return 11;
            case 2: return 10;
            case 14: return 9;   // A
            case 13: return 8;   // K
            case 12: return 7;   // Q
            case 11: return 6;   // J
            case 10: return 5;
            case 9: return 4;
            case 7: return 3;
            case 6: return 2;
            default: return 1;   // 4
        }
    }

    public string RankText
    {
        get
        {
            if (Rank == 16) return "大王";
            if (Rank == 15) return "小王";
            if (Rank == 14) return "A";
            if (Rank == 13) return "K";
            if (Rank == 12) return "Q";
            if (Rank == 11) return "J";
            return Rank.ToString();
        }
    }

    public string SuitGlyph
    {
        get
        {
            switch (Suit)
            {
                case 0: return "♠";
                case 1: return "♥";
                case 2: return "♣";
                case 3: return "♦";
                default: return "";
            }
        }
    }

    public string Display { get { return IsJoker ? RankText : RankText + SuitGlyph; } }

    public static string RankTextOf(int rank)
    {
        if (rank == 16) return Lang.T("rank_big_joker");
        if (rank == 15) return Lang.T("rank_small_joker");
        if (rank == 14) return "A";
        if (rank == 13) return "K";
        if (rank == 12) return "Q";
        if (rank == 11) return "J";
        return rank.ToString();
    }

    public static List<Card> BuildDeck()
    {
        List<Card> deck = new List<Card>();
        for (int s = 0; s < 4; s++)
            for (int r = 2; r <= 14; r++)
                deck.Add(new Card(r, s));
        deck.Add(new Card(15, 4));
        deck.Add(new Card(16, 4));
        return deck;
    }

    public static void Shuffle(List<Card> deck, Random rnd)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            Card t = deck[i]; deck[i] = deck[j]; deck[j] = t;
        }
    }

    public static int CompareDesc(Card a, Card b)
    {
        if (a.Value != b.Value) return b.Value.CompareTo(a.Value);
        return a.Suit.CompareTo(b.Suit);
    }
}

// ============ 牌型 ============
enum PlayType { Single = 0, Pair = 1, Triple = 2, TripleOne = 3 }

class Play
{
    public PlayType Type;
    public List<Card> Cards;
    public int Key;      // 比较用的点数（三带一取三张的点数）

    public Play(PlayType t, List<Card> cards, int key)
    {
        Type = t; Cards = cards; Key = key;
    }

    public string TypeName
    {
        get
        {
            switch (Type)
            {
                case PlayType.Single: return Lang.T("type_single");
                case PlayType.Pair: return Lang.T("type_pair");
                case PlayType.Triple: return Lang.T("type_triple");
                default: return Lang.T("type_triple1");
            }
        }
    }

    public string Text
    {
        get
        {
            List<string> parts = new List<string>();
            foreach (Card c in Cards) parts.Add(c.Display);
            return string.Join(" ", parts.ToArray());
        }
    }
}

static class PlayRules
{
    // 解析一手牌，非法返回 null
    public static Play Parse(List<Card> sel, int handCount)
    {
        if (sel == null || sel.Count == 0) return null;
        List<IGrouping<int, Card>> groups = sel.GroupBy(c => c.Rank).ToList();

        if (sel.Count == 1)
            return new Play(PlayType.Single, new List<Card>(sel), sel[0].Rank);

        if (sel.Count == 2)
        {
            if (groups.Count == 1 && !sel[0].IsJoker)
                return new Play(PlayType.Pair, new List<Card>(sel), sel[0].Rank);
            return null;
        }

        if (sel.Count == 3)
        {
            // 只剩最后三张同点数时才允许三张不带
            if (groups.Count == 1 && !sel[0].IsJoker && handCount == 3)
                return new Play(PlayType.Triple, new List<Card>(sel), sel[0].Rank);
            return null;
        }

        if (sel.Count == 4)
        {
            List<IGrouping<int, Card>> big = groups.Where(g => g.Count() == 3).ToList();
            if (big.Count == 1 && groups.Count == 2)
                return new Play(PlayType.TripleOne, new List<Card>(sel), big[0].Key);
            return null;
        }

        return null;
    }

    public static bool Beats(Play a, Play b)
    {
        if (a == null) return false;
        if (b == null) return true;
        if (a.Type != b.Type) return false;
        if (a.Cards.Count != b.Cards.Count) return false;
        return Card.GameValue(a.Key) > Card.GameValue(b.Key);
    }

    public static List<Play> AllPlays(List<Card> hand)
    {
        List<Play> res = new List<Play>();
        if (hand == null || hand.Count == 0) return res;

        List<IGrouping<int, Card>> groups = hand.GroupBy(c => c.Rank).ToList();
        foreach (IGrouping<int, Card> g in groups)
        {
            List<Card> cards = g.ToList();
            int key = g.Key;

            res.Add(new Play(PlayType.Single, new List<Card>(new Card[] { cards[0] }), key));

            if (cards.Count >= 2 && !cards[0].IsJoker)
                res.Add(new Play(PlayType.Pair, new List<Card>(new Card[] { cards[0], cards[1] }), key));

            if (cards.Count >= 3 && !cards[0].IsJoker)
            {
                List<Card> kickers = hand.Where(c => c.Rank != key).ToList();
                kickers.Sort((a, b) => a.Value.CompareTo(b.Value));
                int n = Math.Min(2, kickers.Count);
                for (int i = 0; i < n; i++)
                {
                    List<Card> lst = new List<Card>(new Card[] { cards[0], cards[1], cards[2], kickers[i] });
                    res.Add(new Play(PlayType.TripleOne, lst, key));
                }
            }
        }

        if (hand.Count == 3 && groups.Count == 1 && !hand[0].IsJoker)
            res.Add(new Play(PlayType.Triple, new List<Card>(hand), hand[0].Rank));

        return res;
    }
}
