using System;
using System.Collections.Generic;
using System.Globalization;

// 多语言字符串表：key + 8 种语言（简中 / 繁中 / 英 / 法 / 俄 / 日 / 西 / 阿）
static class Lang
{
    public static readonly string[] Codes = new string[] { "zh-CN", "zh-TW", "en", "fr", "ru", "ja", "es", "ar" };
    public static readonly string[] Names = new string[] { "简体中文", "繁體中文", "English", "Français", "Русский", "日本語", "Español", "العربية" };

    public static int Idx = 0;

    public static string Code { get { return Codes[Idx]; } }
    public static bool IsRtl { get { return Codes[Idx] == "ar"; } }
    public static string FontFamily { get { return IsRtl ? "Segoe UI" : "Microsoft YaHei"; } }

    public static void SetCode(string code)
    {
        for (int i = 0; i < Codes.Length; i++)
            if (Codes[i] == code) { Idx = i; return; }
        Idx = 0;
    }

    static readonly string[,] TAB = new string[,]
    {
        // key, zh-CN, zh-TW, en, fr, ru, ja, es, ar
        { "game_title",
          "争 上 游", "爭 上 遊", "Zheng Shang You", "Zheng Shang You", "Zheng Shang You",
          "争上游", "Zheng Shang You", "تشنغ شانغ يو" },

        { "subtitle",
          "五 局 · 三 人 · 真 实 牌 面", "五 局 · 三 人 · 真 實 牌 面",
          "5 Rounds · 3 Players · Real Cards", "5 manches · 3 joueurs · vraies cartes",
          "5 раундов · 3 игрока · настоящие карты", "5ラウンド · 3人 · 本物のカード",
          "5 rondas · 3 jugadores · cartas reales", "٥ جولات · ٣ لاعبين · بطاقات حقيقية" },

        { "rank_line",
          "5 > 8 > 大王 > 小王 > 3 > 2 > A > K > Q > J > 10 > 9 > 7 > 6 > 4",
          "5 > 8 > 大王 > 小王 > 3 > 2 > A > K > Q > J > 10 > 9 > 7 > 6 > 4",
          "5 > 8 > Big Joker > Small Joker > 3 > 2 > A > K > Q > J > 10 > 9 > 7 > 6 > 4",
          "5 > 8 > Grand Joker > Petit Joker > 3 > 2 > A > K > Q > J > 10 > 9 > 7 > 6 > 4",
          "5 > 8 > Большой Джокер > Малый Джокер > 3 > 2 > A > K > Q > J > 10 > 9 > 7 > 6 > 4",
          "5 > 8 > 大ジョーカー > 小ジョーカー > 3 > 2 > A > K > Q > J > 10 > 9 > 7 > 6 > 4",
          "5 > 8 > Comodín grande > Comodín pequeño > 3 > 2 > A > K > Q > J > 10 > 9 > 7 > 6 > 4",
          "من الأقوى إلى الأضعف: الخمسة، ثم الثمانية، ثم الجوكر الكبير، ثم الجوكر الصغير، ثم الثلاثة، ثم الاثنان، ثم الآس، ثم الملك، ثم الوزير، ثم الجاك، ثم العشرة، ثم التسعة، ثم السبعة، ثم الستة، ثم الأربعة" },

        { "btn_start",
          "开 始 游 戏", "開 始 遊 戲", "Start Game", "Démarrer", "Начать игру",
          "ゲーム開始", "Iniciar partida", "ابدأ اللعب" },

        { "btn_rules",
          "游 戏 规 则", "遊 戲 規 則", "Rules", "Règles", "Правила",
          "ルール", "Reglas", "القواعد" },

        { "btn_exit",
          "退 出 游 戏", "退 出 遊 戲", "Exit", "Quitter", "Выход",
          "終了", "Salir", "خروج" },

        { "start_msg_title",
          "准备开始", "準備開始", "Ready", "Prêt", "Готово",
          "準備", "Preparado", "الاستعداد" },

        { "start_msg_body",
          "本游戏共 5 轮（5 局），采用 3 人模式：\r\n\r\n你 与 电脑甲、电脑乙 同桌对战。\r\n\r\n每轮先出完的是上游，第二是中游，最后是下游。\r\n第 2 局起有上贡与要牌环节。\r\n\r\n每手思考时间 30 秒，超时自动处理。",
          "本遊戲共 5 輪（5 局），採用 3 人模式：\r\n\r\n你 與 電腦甲、電腦乙 同桌對戰。\r\n\r\n每輪先出完的是上游，第二是中游，最後是下游。\r\n第 2 局起有上貢與要牌環節。\r\n\r\n每手思考時間 30 秒，超時自動處理。",
          "This game has 5 rounds in 3-player mode.\r\n\r\nYou play against CPU A and CPU B.\r\n\r\nFirst to empty the hand is 1st, then 2nd, last is 3rd.\r\nTribute and request phases start from round 2.\r\n\r\n30 seconds per turn; the game acts for you when time is up.",
          "5 manches en mode 3 joueurs.\r\n\r\nVous affrontez IA A et IA B.\r\n\r\nLe premier à finir est 1er, puis 2e, le dernier est 3e.\r\nLes échanges commencent à la manche 2.\r\n\r\n30 secondes par tour, action automatique en cas de dépassement.",
          "5 раундов, режим на 3 игрока.\r\n\r\nВы играете против Бота A и Бота B.\r\n\r\nПервый вышедший — 1-й, затем 2-й, последний — 3-й.\r\nС 2-го раунда начинаются взносы и запросы.\r\n\r\n30 секунд на ход, иначе ход выполняется автоматически.",
          "全5ラウンド・3人モードです。\r\n\r\nあなたは CPU甲・CPU乙 と対戦します。\r\n\r\n最初に上がった人が1位、次が2位、最後が3位です。\r\n2ラウンド目から上納と要求を行います。\r\n\r\n1手30秒、超過すると自動処理されます。",
          "5 rondas en modo de 3 jugadores.\r\n\r\nJuegas contra CPU A y CPU B.\r\n\r\nEl primero en quedarse sin cartas es 1º, luego 2º, el último es 3º.\r\nLos intercambios empiezan en la ronda 2.\r\n\r\n30 segundos por turno; si se agota, se actúa automáticamente.",
          "٥ جولات بوضع ٣ لاعبين.\r\n\r\nأنت تلعب ضد الكمبيوتر أ والكمبيوتر ب.\r\n\r\nالأول الذي ينهي أوراقه هو الأول، ثم الثاني، والأخير هو الثالث.\r\nتبدأ مراحل الإهداء والطلب من الجولة الثانية.\r\n\r\n٣٠ ثانية لكل دور، وبعدها يُنفَّذ تلقائياً." },

        { "rules_title",
          "游戏规则", "遊戲規則", "Game Rules", "Règles du jeu", "Правила игры",
          "ゲームのルール", "Reglas del juego", "قواعد اللعبة" },

        { "rules_body",
          "【一、牌力大小】\r\n从小到大：4 < 6 < 7 < 9 < 10 < J < Q < K < A < 2 < 3 < 小王 < 大王 < 8 < 5\r\n也就是：5 最大，8 次之，然后是 大王、小王、3、2、A、K、Q、J、10、9、7、6，4 最小。\r\n\r\n【二、允许的牌型】\r\n1. 单张：一张牌。\r\n2. 对子：两张点数相同的牌（大小王不算对子）。\r\n3. 三带一：三张点数相同的牌 + 任意一张单牌，共 4 张。\r\n　比较大小时只看三张的点数，附带的那张不参与比较。\r\n　（若最后手里只剩三张同点牌，可以一次打出，算“三张”。）\r\n本游戏没有：三带二、炸弹、飞机、连对、顺子、同花等牌型。\r\n只有牌型相同、张数相同才能比较大小，点数大的压点数小的；压不过就只能“不要”。\r\n\r\n【三、用牌与座次】\r\n一副牌 54 张，3 人各 18 张，没有地主，三人各自为战。\r\n出牌顺序：你 → 电脑甲 → 电脑乙 → 你，循环进行。\r\n其他两人都不要时，由最后出牌的人重新领出（可以出任意合法牌型）。\r\n\r\n【四、第一局：红桃 4 定先手】\r\n第 1 局由拿到“红桃 4”的人先出牌，并且他打出的第一手牌必须包含红桃 4\r\n（单张红桃4、对子里的红桃4、三带一里的红桃4 都可以）。\r\n从第 2 局开始，由上一局的上游先出牌。\r\n\r\n【五、名次】\r\n每一局中：最先出完牌的 = 上游；第二个出完的 = 中游；最后剩下的 = 下游。\r\n上游出完后，其余两人继续出牌，直到分出中游和下游。\r\n\r\n【六、第 2 局起的换牌（上贡与要牌）】\r\n1. 上贡：上一局的下游，必须把手上最大的一张牌交给中游。\r\n2. 上贡：上一局的中游，必须把手上最大的一张牌交给上游。\r\n3. 要牌：中游可以向上游讨要一张自己需要的牌（用来凑对子或三带一）。\r\n　前提是上游手里确实有这张牌，且上游愿意给；不愿意就换一张再要，最多被拒绝 3 次，\r\n　拒绝满 3 次后由上游随机给一张。\r\n4. 要牌：下游同理向中游讨要一张自己需要的牌，规则相同。\r\n　不能要对方手里最大的那张牌，也不能把刚上贡的牌要回来。\r\n（每一局流程：先重新发牌，然后换牌，再开始出牌。）\r\n\r\n【七、时间】\r\n每手牌思考时间 30 秒。超过 30 秒自动“不要”；\r\n如果轮到你领出，超时则自动打出最小的一手合法牌。\r\n\r\n【八、赛制】\r\n共 5 局，打满后统计每局的上游 / 中游 / 下游。",
          "【一、牌力大小】\r\n從小到大：4 < 6 < 7 < 9 < 10 < J < Q < K < A < 2 < 3 < 小王 < 大王 < 8 < 5\r\n也就是：5 最大，8 次之，然後是 大王、小王、3、2、A、K、Q、J、10、9、7、6，4 最小。\r\n\r\n【二、允許的牌型】\r\n1. 單張：一張牌。\r\n2. 對子：兩張點數相同的牌（大小王不算對子）。\r\n3. 三帶一：三張點數相同的牌 + 任意一張單牌，共 4 張。\r\n　比較大小時只看三張的點數，附帶的那張不參與比較。\r\n　（若最後手裡只剩三張同點牌，可以一次打出，算「三張」。）\r\n本遊戲沒有：三帶二、炸彈、飛機、連對、順子、同花等牌型。\r\n只有牌型相同、張數相同才能比較大小，點數大的壓點數小的；壓不過就只能「不要」。\r\n\r\n【三、用牌與座次】\r\n一副牌 54 張，3 人各 18 張，沒有地主，三人各自為戰。\r\n出牌順序：你 → 電腦甲 → 電腦乙 → 你，循環進行。\r\n其他兩人都不要時，由最後出牌的人重新領出（可以出任意合法牌型）。\r\n\r\n【四、第一局：紅桃 4 定先手】\r\n第 1 局由拿到「紅桃 4」的人先出牌，並且他打出的第一手牌必須包含紅桃 4\r\n（單張紅桃4、對子裡的紅桃4、三帶一裡的紅桃4 都可以）。\r\n從第 2 局開始，由上一局的上游先出牌。\r\n\r\n【五、名次】\r\n每一局中：最先出完牌的 = 上游；第二個出完的 = 中游；最後剩下的 = 下游。\r\n上游出完後，其餘兩人繼續出牌，直到分出中游和下游。\r\n\r\n【六、第 2 局起的換牌（上貢與要牌）】\r\n1. 上貢：上一局的下游，必須把手裡最大的一張牌交給中游。\r\n2. 上貢：上一局的中游，必須把手裡最大的一張牌交給上游。\r\n3. 要牌：中游可以向上游討要一張自己需要的牌（用來湊對子或三帶一）。\r\n　前提是上游手裡確實有這張牌，且上游願意給；不願意就換一張再要，最多被拒絕 3 次，\r\n　拒絕滿 3 次後由上游隨機給一張。\r\n4. 要牌：下游同理向中游討要一張自己需要的牌，規則相同。\r\n　不能要對方手裡最大的那張牌，也不能把剛上貢的牌要回來。\r\n（每一局流程：先重新發牌，然後換牌，再開始出牌。）\r\n\r\n【七、時間】\r\n每手牌思考時間 30 秒。超過 30 秒自動「不要」；\r\n如果輪到你領出，超時則自動打出最小的一手合法牌。\r\n\r\n【八、賽制】\r\n共 5 局，打滿後統計每局的上游 / 中游 / 下游。",
          "[1] Card strength\r\nLow to high: 4 < 6 < 7 < 9 < 10 < J < Q < K < A < 2 < 3 < Small Joker < Big Joker < 8 < 5\r\nSo: 5 is the strongest, then 8, Big Joker, Small Joker, 3, 2, A, K, Q, J, 10, 9, 7, 6, and 4 is the weakest.\r\n\r\n[2] Allowed plays\r\n1. Single: one card.\r\n2. Pair: two cards of the same rank (jokers cannot form a pair).\r\n3. Triple + 1: three cards of the same rank plus any single card, 4 cards total.\r\n   Only the triple's rank counts when comparing; the kicker does not.\r\n   (If your last 3 cards share a rank, you may play them at once as a Triple.)\r\nNo triple+2, no bomb, no airplane, no consecutive pairs, no straights, no flush.\r\nA play can beat another only with the same type and same card count and a higher rank; otherwise you must Pass.\r\n\r\n[3] Deck and order\r\n54 cards, 3 players, 18 each, free-for-all.\r\nOrder: You → CPU A → CPU B → You, repeating.\r\nWhen both others pass, the last player to play leads again with any legal play.\r\n\r\n[4] Round 1: the 4 of Hearts leads\r\nThe player holding the 4 of Hearts starts round 1, and that first play must contain the 4 of Hearts\r\n(a single 4 of Hearts, a pair containing it, or a triple+1 containing it are all fine).\r\nFrom round 2 on, the previous 1st place leads.\r\n\r\n[5] Ranking\r\nFirst to empty the hand = 1st; second = 2nd; the remaining player = 3rd.\r\nAfter 1st finishes, the other two keep playing until 2nd and 3rd are decided.\r\n\r\n[6] Trades from round 2 (tribute, then request)\r\n1. Tribute: last round's 3rd place must give their highest card to the 2nd place.\r\n2. Tribute: last round's 2nd place must give their highest card to the 1st place.\r\n3. Request: the 2nd place may ask the 1st place for a card they need (to complete a pair or triple+1).\r\nThe 1st place must actually hold that rank and may refuse; each refusal forces a new request, up to 3 refusals, then a random card is given.\r\n4. Request: the 3rd place asks the 2nd place in the same way, with the same rules.\r\nYou may not request the giver's highest card, nor ask back the card just tributed.\r\n(Each round: redeal, then trades, then play.)\r\n\r\n[7] Time\r\n30 seconds per turn. Past 30 seconds you automatically Pass.\r\nIf you are leading, a timeout plays your smallest legal lead.\r\n\r\n[8] Format\r\n5 rounds; 1st / 2nd / 3rd are recorded for each round.",
          "[1] Force des cartes\r\nDu plus faible au plus fort : 4 < 6 < 7 < 9 < 10 < J < Q < K < A < 2 < 3 < Petit Joker < Grand Joker < 8 < 5\r\nDonc 5 est la plus forte, puis 8, Grand Joker, Petit Joker, 3, 2, A, K, Q, J, 10, 9, 7, 6, et 4 la plus faible.\r\n\r\n[2] Combinaisons autorisées\r\n1. Simple : une carte.\r\n2. Paire : deux cartes de même rang (les jokers ne forment pas de paire).\r\n3. Triple + 1 : trois cartes de même rang plus une carte seule, 4 cartes au total.\r\n   Seul le rang du triple compte dans la comparaison.\r\n   (S'il ne reste que 3 cartes de même rang, on peut les jouer d'un coup en « Triple ».)\r\nPas de triple+2, pas de bombe, pas d'avion, pas de paires suivies, pas de suite, pas de couleur.\r\nUne combinaison ne bat qu'une combinaison de même type, même nombre de cartes et rang supérieur ; sinon il faut Passer.\r\n\r\n[3] Jeu et ordre\r\n54 cartes, 3 joueurs, 18 chacun, chacun pour soi.\r\nOrdre : Vous → IA A → IA B → Vous.\r\nSi les deux autres passent, le dernier à avoir joué relance librement.\r\n\r\n[4] Manche 1 : le 4 de cœur ouvre\r\nLe joueur qui a le 4 de cœur commence la manche 1 et son premier coup doit contenir ce 4 de cœur.\r\nDès la manche 2, le 1er de la manche précédente ouvre.\r\n\r\n[5] Classement\r\nPremier à finir = 1er ; deuxième = 2e ; le dernier = 3e.\r\nQuand le 1er a fini, les deux autres continuent jusqu'au classement complet.\r\n\r\n[6] Échanges dès la manche 2 (tribut, puis demande)\r\n1. Tribut : le 3e de la manche précédente donne sa carte la plus forte au 2e.\r\n2. Tribut : le 2e de la manche précédente donne sa carte la plus forte au 1er.\r\n3. Demande : le 2e peut demander au 1er une carte dont il a besoin (pour une paire ou un triple+1).\r\nLe 1er doit posséder ce rang et peut refuser ; chaque refus impose une nouvelle demande, 3 refus maximum, puis une carte est donnée au hasard.\r\n4. Demande : le 3e demande au 2e de la même façon, mêmes règles.\r\nInterdit de demander la carte la plus forte du donneur ou celle qui vient d'être donnée en tribut.\r\n(Chaque manche : redistribution, puis échanges, puis jeu.)\r\n\r\n[7] Temps\r\n30 secondes par tour. Au-delà, vous Passez automatiquement.\r\nSi c'est vous qui ouvrez, le délai joue votre plus petite relance légale.\r\n\r\n[8] Format\r\n5 manches ; le classement de chaque manche est enregistré.",
          "[1] Сила карт\r\nОт слабых к сильным: 4 < 6 < 7 < 9 < 10 < J < Q < K < A < 2 < 3 < Малый Джокер < Большой Джокер < 8 < 5\r\nТо есть 5 — самая сильная, затем 8, Большой Джокер, Малый Джокер, 3, 2, A, K, Q, J, 10, 9, 7, 6, а 4 — самая слабая.\r\n\r\n[2] Разрешённые комбинации\r\n1. Одиночная: одна карта.\r\n2. Пара: две карты одного достоинства (джокеры пару не образуют).\r\n3. Тройка + 1: три карты одного достоинства плюс любая одиночная, всего 4 карты.\r\n   При сравнении учитывается только достоинство тройки.\r\n   (Если остались ровно 3 карты одного достоинства, их можно сыграть сразу как «Тройку».)\r\nНет тройки+2, бомб, «самолётов», пар подряд, стритов и мастей.\r\nКомбинацию можно побить только такой же по типу и числу карт с более высоким достоинством; иначе — Пас.\r\n\r\n[3] Колода и порядок\r\n54 карты, 3 игрока по 18, каждый сам за себя.\r\nПорядок: Вы → Бот A → Бот B → Вы.\r\nЕсли оба спасовали, последний игравший начинает снова любой законной комбинацией.\r\n\r\n[4] Раунд 1: ведёт 4 червей\r\nРаунд 1 начинает игрок с 4 червей, и его первый ход обязан содержать 4 червей\r\n(одиночная 4 червей, пара с ней или тройка+1 с ней — всё допустимо).\r\nС раунда 2 начинает победитель прошлого раунда.\r\n\r\n[5] Места\r\nПервый без карт = 1-е место; второй = 2-е; оставшийся = 3-е.\r\nПосле выхода первого двое продолжают до распределения мест.\r\n\r\n[6] Обмен с раунда 2 (сначала взнос, затем запрос)\r\n1. Взнос: 3-е место прошлого раунда отдаёт свою старшую карту 2-му месту.\r\n2. Взнос: 2-е место прошлого раунда отдаёт свою старшую карту 1-му месту.\r\n3. Запрос: 2-е место может попросить у 1-го нужную карту (для пары или тройки+1).\r\nУ 1-го должна быть эта карта, и он вправе отказать; каждый отказ требует нового запроса, максимум 3 отказа, затем карта выдаётся случайно.\r\n4. Запрос: 3-е место просит у 2-го по тем же правилам.\r\nНельзя просить старшую карту отдающего и нельзя требовать обратно только что отданную карту.\r\n(Каждый раунд: раздача, обмен, игра.)\r\n\r\n[7] Время\r\n30 секунд на ход. После 30 секунд — автоматический Пас.\r\nЕсли ход ваш и вы начинаете, по таймауту играется минимальная законная комбинация.\r\n\r\n[8] Формат\r\n5 раундов; места каждого раунда записываются.",
          "【1】カードの強さ\r\n弱い順：4 < 6 < 7 < 9 < 10 < J < Q < K < A < 2 < 3 < 小ジョーカー < 大ジョーカー < 8 < 5\r\nつまり 5 が最強、次に 8、大ジョーカー、小ジョーカー、3、2、A、K、Q、J、10、9、7、6、4 が最弱です。\r\n\r\n【2】出せる役\r\n1. 単発：1枚。\r\n2. ペア：同じ数字の2枚（ジョーカーはペアになりません）。\r\n3. スリー＋1：同じ数字3枚＋任意の1枚、計4枚。\r\n　比較は3枚の数字だけで行い、添える1枚は比べません。\r\n　（最後に同じ数字3枚だけが残った場合、まとめて「スリー」として出せます。）\r\nスリー＋2、ボム、飛行機、連続ペア、ストレート、フラッシュはありません。\r\n同じ役・同じ枚数で数字が大きいものだけが勝ち、それ以外はパスしかありません。\r\n\r\n【3】カードと順番\r\n54枚、3人で18枚ずつ、各自の戦い。\r\n順番：あなた → CPU甲 → CPU乙 → あなた。\r\n他の2人がパスしたら、最後に出した人が自由に切り出します。\r\n\r\n【4】第1局：ハートの4が先手\r\n第1局はハートの4を持つ人が先手で、最初の一手にハートの4を含める必要があります\r\n（単発・ペア・スリー＋1 のいずれでも可）。\r\n第2局からは前局の1位が先手です。\r\n\r\n【5】順位\r\n最初に上がった人＝1位、次＝2位、残った人＝3位。\r\n1位が上がった後も、2位と3位が決まるまで続けます。\r\n\r\n【6】第2局からの交換（まず上納、次に要求）\r\n1. 上納：前局3位は自分の最大の1枚を2位に渡します。\r\n2. 上納：前局2位は自分の最大の1枚を1位に渡します。\r\n3. 要求：2位は1位に必要な1枚（ペアやスリー＋1を作るため）を求められます。\r\n1位がその数字を持ち、かつ渡す意思がある場合のみ。断られたら別の数字を要求し、最大3回まで。3回断られたらランダムに1枚渡します。\r\n4. 要求：3位も同じ規則で2位に要求します。\r\n相手の最大の1枚は要求できず、上納したばかりのカードも要求できません。\r\n（各局：配り直し → 交換 → プレイ）\r\n\r\n【7】時間\r\n1手30秒。超えると自動でパス。\r\n自分が切り出す番で時間切れの場合、最小の合法手を自動で出します。\r\n\r\n【8】形式\r\n全5局、各局の1位・2位・3位を記録します。",
          "[1] Fuerza de las cartas\r\nDe menor a mayor: 4 < 6 < 7 < 9 < 10 < J < Q < K < A < 2 < 3 < Comodín pequeño < Comodín grande < 8 < 5\r\nEs decir: 5 es la más fuerte, luego 8, Comodín grande, Comodín pequeño, 3, 2, A, K, Q, J, 10, 9, 7, 6 y 4 la más débil.\r\n\r\n[2] Jugadas permitidas\r\n1. Individual: una carta.\r\n2. Pareja: dos cartas del mismo rango (los comodines no forman pareja).\r\n3. Trío + 1: tres cartas del mismo rango más una carta suelta, 4 en total.\r\n   Solo cuenta el rango del trío al comparar.\r\n   (Si quedan exactamente 3 cartas del mismo rango, se pueden jugar de golpe como «Trío».)\r\nNo hay trío+2, bomba, avión, parejas seguidas, escaleras ni color.\r\nSolo se supera otra jugada del mismo tipo, mismo número de cartas y rango mayor; si no, hay que Pasar.\r\n\r\n[3] Baraja y orden\r\n54 cartas, 3 jugadores, 18 cada uno, todos contra todos.\r\nOrden: Tú → CPU A → CPU B → Tú.\r\nSi los otros dos pasan, quien jugó al final vuelve a abrir con cualquier jugada legal.\r\n\r\n[4] Ronda 1: abre el 4 de corazones\r\nQuien tenga el 4 de corazones abre la ronda 1 y su primera jugada debe contener ese 4 de corazones.\r\nDesde la ronda 2 abre el 1º de la ronda anterior.\r\n\r\n[5] Clasificación\r\nPrimero en quedarse sin cartas = 1º; segundo = 2º; el restante = 3º.\r\nTras terminar el 1º, los otros dos siguen hasta decidir 2º y 3º.\r\n\r\n[6] Intercambios desde la ronda 2 (primero tributo, luego petición)\r\n1. Tributo: el 3º de la ronda anterior entrega su carta más alta al 2º.\r\n2. Tributo: el 2º de la ronda anterior entrega su carta más alta al 1º.\r\n3. Petición: el 2º puede pedir al 1º una carta que necesite (para pareja o trío+1).\r\nEl 1º debe tener ese rango y puede negarse; cada negativa obliga a pedir otra, hasta 3 negativas, luego se entrega una carta al azar.\r\n4. Petición: el 3º pide al 2º igual, con las mismas reglas.\r\nNo se puede pedir la carta más alta del que entrega ni recuperar la carta recién tributada.\r\n(Cada ronda: reparto, intercambios y juego.)\r\n\r\n[7] Tiempo\r\n30 segundos por turno. Al pasarse, se Pasa automáticamente.\r\nSi te toca abrir, el tiempo agotado juega la jugada legal más baja.\r\n\r\n[8] Formato\r\n5 rondas; se registra el puesto de cada ronda.",
          "[١] قوة البطاقات\r\nمن الأضعف إلى الأقوى: الأربعة، ثم الستة، ثم السبعة، ثم التسعة، ثم العشرة، ثم الجاك، ثم الوزير، ثم الملك، ثم الآس، ثم الاثنان، ثم الثلاثة، ثم الجوكر الصغير، ثم الجوكر الكبير، ثم الثمانية، ثم الخمسة\r\nأي أن الخمسة هي الأقوى، ثم الثمانية، ثم الجوكر الكبير، ثم الجوكر الصغير، ثم الثلاثة، ثم الاثنان، ثم الآس، ثم الملك، ثم الوزير، ثم الجاك، ثم العشرة، ثم التسعة، ثم السبعة، ثم الستة، ثم الأربعة هي الأضعف.\r\n\r\n[٢] الأشكال المسموحة\r\n1. مفرد: بطاقة واحدة.\r\n2. زوج: بطاقتان بالرتبة نفسها (الجوكر لا يكوّن زوجاً).\r\n3. ثلاثية + 1: ثلاث بطاقات بالرتبة نفسها مع بطاقة واحدة، أي 4 بطاقات.\r\n   تُقارن رتبة الثلاثية فقط، ولا تُحتسب البطاقة المرفقة.\r\n   (إذا بقيت 3 بطاقات بالرتبة نفسها، يمكن لعبها دفعة واحدة كـ«ثلاثية».)\r\nلا يوجد ثلاثية + 2، ولا قنبلة، ولا طائرة، ولا أزواج متتابعة، ولا سلاسل ولا لون.\r\nلا يُهزم الشكل إلا بشكل مماثل بنفس النوع وعدد البطاقات وبرتبة أعلى، وإلا وجب التمرير.\r\n\r\n[٣] البطاقات والترتيب\r\n54 بطاقة، 3 لاعبين، 18 لكل لاعب، كلٌّ يلعب لنفسه.\r\nالترتيب: أنت ← الكمبيوتر أ ← الكمبيوتر ب ← أنت.\r\nوإذا مرّر الآخران، يبدأ آخر من لعب من جديد بأي شكل صحيح.\r\n\r\n[٤] الجولة الأولى: 4 القلوب يبدأ\r\nمن يملك 4 القلوب يبدأ الجولة الأولى، ويجب أن تحتوي أول بطاقة يلعبها على 4 القلوب.\r\nمن الجولة الثانية يبدأ صاحب المركز الأول في الجولة السابقة.\r\n\r\n[٥] المراكز\r\nأول من ينهي بطاقاته = الأول، ثم الثاني، والباقي = الثالث.\r\nبعد انتهاء الأول يستمر الآخران حتى يتحدد الثاني والثالث.\r\n\r\n[٦] التبادل من الجولة الثانية (الإهداء أولاً ثم الطلب)\r\n1. الإهداء: صاحب المركز الثالث في الجولة السابقة يسلّم أعلى بطاقة لديه لصاحب المركز الثاني.\r\n2. الإهداء: صاحب المركز الثاني يسلّم أعلى بطاقة لديه لصاحب المركز الأول.\r\n3. الطلب: يمكن للثاني أن يطلب من الأول بطاقة يحتاجها (لإكمال زوج أو ثلاثية + 1).\r\nبشرط أن تكون البطاقة لدى الأول وأن يقبل؛ وكل رفض يعني طلب بطاقة أخرى، بحد أقصى 3 رفضات، ثم تُعطى بطاقة عشوائية.\r\n4. الطلب: الثالث يطلب من الثاني بالطريقة نفسها وبنفس القواعد.\r\nلا يجوز طلب أعلى بطاقة لدى الطرف الآخر، ولا استرجاع البطاقة التي أُهديت للتو.\r\n(كل جولة: توزيع ثم تبادل ثم لعب.)\r\n\r\n[٧] الوقت\r\n30 ثانية لكل دور. بعدها يُمرَّر تلقائياً.\r\nوإذا كان دورك للبدء، يلعب الوقت المنتهي أقل بطاقة صحيحة.\r\n\r\n[٨] النظام\r\n5 جولات، وتُسجَّل مراكز كل جولة." },

        { "btn_close",
          "关 闭", "關 閉", "Close", "Fermer", "Закрыть",
          "閉じる", "Cerrar", "إغلاق" },

        { "name_you",
          "你", "你", "You", "Vous", "Вы",
          "あなた", "Tú", "أنت" },
        { "name_bot1",
          "电脑甲", "電腦甲", "CPU A", "IA A", "Бот A",
          "CPU甲", "CPU A", "الكمبيوتر أ" },
        { "name_bot2",
          "电脑乙", "電腦乙", "CPU B", "IA B", "Бот B",
          "CPU乙", "CPU B", "الكمبيوتر ب" },

        { "role_up",
          "上局·上游", "上局·上游", "Last: 1st", "Préc.: 1er", "Прошлый: 1-й",
          "前回・1位", "Ant.: 1º", "السابق: الأول" },
        { "role_mid",
          "上局·中游", "上局·中游", "Last: 2nd", "Préc.: 2e", "Прошлый: 2-й",
          "前回・2位", "Ant.: 2º", "السابق: الثاني" },
        { "role_down",
          "上局·下游", "上局·下游", "Last: 3rd", "Préc.: 3e", "Прошлый: 3-й",
          "前回・3位", "Ant.: 3º", "السابق: الثالث" },

        { "type_single",
          "单张", "單張", "Single", "Simple", "Одиночная",
          "単発", "Individual", "مفرد" },
        { "type_pair",
          "对子", "對子", "Pair", "Paire", "Пара",
          "ペア", "Pareja", "زوج" },
        { "type_triple",
          "三张", "三張", "Triple", "Triple", "Тройка",
          "スリー", "Trío", "ثلاثية" },
        { "type_triple1",
          "三带一", "三帶一", "Triple+1", "Triple+1", "Тройка+1",
          "スリー+1", "Trío+1", "ثلاثية+1" },

        { "rank_big_joker",
          "大王", "大王", "Big Joker", "Grand Joker", "Большой Джокер",
          "大ジョーカー", "Comodín grande", "جوكر كبير" },
        { "rank_small_joker",
          "小王", "小王", "Small Joker", "Petit Joker", "Малый Джокер",
          "小ジョーカー", "Comodín pequeño", "جوكر صغير" },

        { "action_play",
          "出牌", "出牌", "Play", "Jouer", "Ход",
          "出した", "Jugó", "لعب" },
        { "action_pass",
          "不要", "不要", "Pass", "Passe", "Пас",
          "パス", "Pasa", "تمرير" },
        { "action_done",
          "已出完", "已出完", "Out", "Terminé", "Вышел",
          "上がり", "Terminó", "انتهى" },

        { "win_title",
          "争上游 · 三人对战 · 共 5 局", "爭上游 · 三人對戰 · 共 5 局",
          "Zheng Shang You · 3 Players · 5 Rounds", "Zheng Shang You · 3 joueurs · 5 manches",
          "Zheng Shang You · 3 игрока · 5 раундов", "争上游 · 3人対戦 · 全5局",
          "Zheng Shang You · 3 jugadores · 5 rondas", "تشنغ شانغ يو · 3 لاعبين · 5 جولات" },

        { "head_round",
          "第 {0} / 5 局　·　3 人模式", "第 {0} / 5 局　·　3 人模式",
          "Round {0} / 5 · 3 Players", "Manche {0} / 5 · 3 joueurs",
          "Раунд {0} / 5 · 3 игрока", "第 {0} / 5 局　·　3人",
          "Ronda {0} / 5 · 3 jugadores", "الجولة {0} / 5 · 3 لاعبين" },

        { "timer_your_turn",
          "轮到你出牌　倒计时 {0} 秒", "輪到你出牌　倒數 {0} 秒",
          "Your turn · {0}s left", "À vous de jouer · {0} s",
          "Ваш ход · осталось {0} с", "あなたの番 · 残り {0} 秒",
          "Tu turno · quedan {0} s", "دورك · {0} ثانية" },

        { "timer_bot",
          "{0} 正在思考…", "{0} 正在思考…",
          "{0} is thinking…", "{0} réfléchit…", "{0} думает…",
          "{0} が考え中…", "{0} está pensando…", "{0} يفكر…" },

        { "round_over",
          "本局已结束", "本局已結束", "Round over", "Manche terminée", "Раунд окончен",
          "この局は終了", "Ronda terminada", "انتهت الجولة" },

        { "remain",
          "剩 {0} 张", "剩 {0} 張", "{0} left", "{0} en main", "Осталось {0}",
          "残り {0} 枚", "Quedan {0}", "متبقٍ {0}" },

        { "hand_info",
          "{0}　剩 {1} 张", "{0}　剩 {1} 張", "{0}　{1} left", "{0}　{1} en main",
          "{0}　осталось {1}", "{0}　残り {1} 枚", "{0}　quedan {1}", "{0}　متبقٍ {1}" },

        { "center_play",
          "{0}　出　{1}", "{0}　出　{1}", "{0} played {1}", "{0} joue {1}",
          "{0}: {1}", "{0} が出した {1}", "{0} juega {1}", "{0} لعب {1}" },

        { "center_you",
          "轮到你出牌", "輪到你出牌", "Your turn", "À vous", "Ваш ход",
          "あなたの番", "Tu turno", "دورك" },

        { "center_bot",
          "{0} 出牌中…", "{0} 出牌中…", "{0} is playing…", "{0} joue…", "{0} играет…",
          "{0} がプレイ中…", "{0} está jugando…", "{0} يلعب…" },

        { "all_pass",
          "三家都不要，由 {0} 重新领出", "三家都不要，由 {0} 重新領出",
          "All passed — {0} leads again", "Tout le monde passe — {0} relance",
          "Все спасовали — {0} начинает снова", "全員パス — {0} が切り出し",
          "Todos pasan — {0} vuelve a abrir", "مرّر الجميع — {0} يبدأ من جديد" },

        { "log_title",
          "对局记录", "對局記錄", "Game log", "Journal", "Журнал партии",
          "対局ログ", "Registro", "سجل الجولة" },

        { "btn_play",
          "出 牌", "出 牌", "Play", "Jouer", "Играть",
          "出す", "Jugar", "العب" },
        { "btn_pass",
          "不 要", "不 要", "Pass", "Passer", "Пас",
          "パス", "Pasar", "مرّر" },
        { "btn_quit_round",
          "退出本局", "退出本局", "Leave", "Quitter", "Выйти",
          "退出", "Salir", "خروج" },

        { "tip_h4_first",
          "首局：第一手必须打出包含红桃 4 的牌", "首局：第一手必須打出包含紅桃 4 的牌",
          "Round 1: your first play must contain the 4 of Hearts",
          "Manche 1 : le premier coup doit contenir le 4 de cœur",
          "Раунд 1: первый ход должен содержать 4 червей",
          "初回：最初の一手にハートの4を含める必要があります",
          "Ronda 1: la primera jugada debe contener el 4 de corazones",
          "الجولة الأولى: يجب أن تحتوي أول ورقة على 4 القلوب" },

        { "tip_timeout",
          "超过 30 秒，自动处理", "超過 30 秒，自動處理", "Timeout — action taken automatically",
          "Délai dépassé — action automatique", "Время вышло — ход выполнен автоматически",
          "30秒超過 — 自動処理", "Tiempo agotado — acción automática",
          "انتهى الوقت — نُفِّذ تلقائياً" },

        { "tip_select",
          "请先点击选中要出的牌", "請先點擊選中要出的牌", "Select the cards you want to play first",
          "Sélectionnez d'abord les cartes", "Сначала выберите карты",
          "先に出したいカードを選んでください", "Primero selecciona las cartas",
          "اختر البطاقات أولاً" },

        { "tip_bad_type",
          "牌型不合法：只能是单张、对子、三带一", "牌型不合法：只能是單張、對子、三帶一",
          "Invalid play: only single, pair, or triple+1",
          "Coup invalide : simple, paire ou triple+1 uniquement",
          "Неверная комбинация: только одиночная, пара или тройка+1",
          "無効な役：単発・ペア・スリー＋1 のみ",
          "Jugada inválida: solo individual, pareja o trío+1",
          "لعب غير صحيح: مفرد أو زوج أو ثلاثية+1 فقط" },

        { "tip_must_beat",
          "必须出同类型且更大的牌，或选择「不要」", "必須出同類型且更大的牌，或選擇「不要」",
          "Play a bigger card of the same type, or Pass",
          "Jouez plus fort du même type, ou Passez",
          "Нужна карта того же типа и старше, иначе Пас",
          "同じ役でより強い札を出すか、パスしてください",
          "Juega una jugada mayor del mismo tipo o Pasa",
          "العب شكلاً أعلى من النوع نفسه أو مرّر" },

        { "tip_need_h4",
          "首局第一手必须包含红桃 4", "首局第一手必須包含紅桃 4",
          "The first play of round 1 must contain the 4 of Hearts",
          "Le premier coup de la manche 1 doit contenir le 4 de cœur",
          "Первый ход раунда 1 должен содержать 4 червей",
          "初回の最初の一手にハートの4が必要です",
          "La primera jugada de la ronda 1 debe contener el 4 de corazones",
          "يجب أن تحتوي أول ورقة في الجولة الأولى على 4 القلوب" },

        { "tip_cant_pass",
          "这一手由你先出，不能不要", "這一手由你先出，不能不要",
          "You are leading this trick — you cannot Pass",
          "C'est vous qui ouvrez — impossible de passer",
          "Вы начинаете — Пас невозможен",
          "あなたが切り出し役なのでパスできません",
          "Abres esta baza — no puedes Pasar",
          "أنت تبدأ هذه الجولة — لا يمكنك التمرير" },

        { "quit_confirm_title",
          "退出", "退出", "Exit", "Quitter", "Выход",
          "終了", "Salir", "خروج" },

        { "quit_confirm_msg",
          "确定要退出本局并返回主菜单吗？", "確定要退出本局並返回主選單嗎？",
          "Leave this round and return to the main menu?",
          "Quitter cette manche et revenir au menu principal ?",
          "Выйти из раунда и вернуться в меню?",
          "この局を終了してメインメニューに戻りますか？",
          "¿Salir de esta ronda y volver al menú principal?",
          "هل تريد الخروج من هذه الجولة والعودة للقائمة الرئيسية؟" },

        { "log_trade_header",
          "—— 第 {0} 局 换牌 ——", "—— 第 {0} 局 換牌 ——",
          "— Round {0}: trades —", "— Manche {0} : échanges —",
          "— Раунд {0}: обмен —", "— 第 {0} 局 交換 —",
          "— Ronda {0}: intercambios —", "— الجولة {0}: التبادل —" },

        { "log_h4_leader",
          "第 1 局：红桃 4 在 {0} 手上，{0} 先出牌", "第 1 局：紅桃 4 在 {0} 手上，{0} 先出牌",
          "Round 1: {0} holds the 4 of Hearts and leads", "Manche 1 : {0} a le 4 de cœur et ouvre",
          "Раунд 1: 4 червей у {0}, он начинает", "第1局：{0} がハートの4を持ち、先手",
          "Ronda 1: {0} tiene el 4 de corazones y abre", "الجولة 1: {0} يملك 4 القلوب ويبدأ" },

        { "btn_new_match",
          "重新开始", "重新開始", "New match", "Nouvelle partie",
          "Новый матч", "新しい対局", "Nueva partida", "مباراة جديدة" },

        { "btn_language",
          "语言", "語言", "Language", "Langue",
          "Язык", "言語", "Idioma", "اللغة" },

        { "menu",
          "菜单", "選單", "Menu", "Menu",
          "Меню", "メニュー", "Menú", "القائمة" },

        { "log_play",
          "{0}：{1}　{2}", "{0}：{1}　{2}", "{0}: {1}  {2}", "{0} : {1}  {2}",
          "{0}: {1}  {2}", "{0}：{1}　{2}", "{0}: {1}  {2}", "{0}: {1}  {2}" },

        { "log_pass",
          "{0}　不要", "{0}　不要", "{0} passes", "{0} passe", "{0} пасует",
          "{0}　パス", "{0} pasa", "{0} يمرّر" },

        { "notice_round_rank",
          "第 {0} 局名次", "第 {0} 局名次", "Round {0} ranking", "Classement manche {0}",
          "Итоги раунда {0}", "第 {0} 局の順位", "Clasificación ronda {0}",
          "ترتيب الجولة {0}" },

        { "msg_all_done",
          "五局已打完。", "五局已打完。", "All 5 rounds are finished.", "Les 5 manches sont terminées.",
          "Все 5 раундов завершены.", "5局すべて終了しました。", "Las 5 rondas han terminado.",
          "انتهت الجولات الخمس." },

        { "msg_next_hint",
          "下一局开始前：下游、中游上贡，中游和下游可以要牌。",
          "下一局開始前：下游、中游上貢，中游和下游可以要牌。",
          "Before the next round: tribute, then requests.",
          "Avant la manche suivante : tribut, puis demandes.",
          "Перед следующим раундом: взносы, затем запросы.",
          "次の局の前に：上納、その後で要求。",
          "Antes de la siguiente ronda: tributo y luego peticiones.",
          "قبل الجولة التالية: الإهداء ثم الطلبات." },

        { "log_finish",
          "{0} 出完，{1}", "{0} 出完，{1}", "{0} is out — {1}", "{0} a fini — {1}",
          "{0} вышел — {1}", "{0} が上がり — {1}", "{0} terminó — {1}", "{0} أنهى أوراقه — {1}" },

        { "finish_up",
          "拿下上游", "拿下上游", "1st place", "1re place", "1-е место",
          "1位", "1º puesto", "المركز الأول" },
        { "finish_mid",
          "拿下中游", "拿下中游", "2nd place", "2e place", "2-е место",
          "2位", "2º puesto", "المركز الثاني" },

        { "log_tribute1",
          "上贡①　{0} 最大的 {1} → {2}", "上貢①　{0} 最大的 {1} → {2}",
          "Tribute 1: {0}'s highest {1} → {2}", "Tribut 1 : {1} (la plus forte de {0}) → {2}",
          "Взнос 1: старшая {1} от {0} → {2}", "上納①　{0} の最大 {1} → {2}",
          "Tributo 1: {1} (la más alta de {0}) → {2}", "إهداء ١: أعلى {1} من {0} → {2}" },

        { "log_tribute2",
          "上贡②　{0} 最大的 {1} → {2}", "上貢②　{0} 最大的 {1} → {2}",
          "Tribute 2: {0}'s highest {1} → {2}", "Tribut 2 : {1} (la plus forte de {0}) → {2}",
          "Взнос 2: старшая {1} от {0} → {2}", "上納②　{0} の最大 {1} → {2}",
          "Tributo 2: {1} (la más alta de {0}) → {2}", "إهداء ٢: أعلى {1} من {0} → {2}" },

        { "log_ask",
          "{0} 向 {1} 要牌", "{0} 向 {1} 要牌", "{0} asks {1} for a card", "{0} demande une carte à {1}",
          "{0} просит карту у {1}", "{0} が {1} にカードを要求", "{0} pide una carta a {1}",
          "{0} يطلب بطاقة من {1}" },

        { "log_step2",
          "—— 第二步：要牌 ——", "—— 第二步：要牌 ——", "— Step 2: request —",
          "— Étape 2 : demande —", "— Шаг 2: запрос —", "— 第2段階：要求 —",
          "— Paso 2: petición —", "— الخطوة ٢: الطلب —" },

        { "log_no_idea",
          "{0} 没有更好的想法，放弃要牌", "{0} 沒有更好的想法，放棄要牌",
          "{0} has no better idea and skips the request",
          "{0} n'a pas d'autre idée et renonce",
          "{0} не нашёл варианта и отказался от запроса",
          "{0} に良い案がなく要求を断念", "{0} no tiene mejor idea y renuncia",
          "{0} لم يجد فكرة أفضل فتخلى عن الطلب" },

        { "log_you_gaveup",
          "你放弃了要牌", "你放棄了要牌", "You skipped the request", "Vous renoncez à la demande",
          "Вы отказались от запроса", "要求を断念しました", "Renunciaste a la petición",
          "تخليت عن الطلب" },

        { "log_want_absent",
          "{0} 想要「{1}」，{2} 手上没有，换一张", "{0} 想要「{1}」，{2} 手上沒有，換一張",
          "{0} wants a {1}, but {2} has none — try another",
          "{0} veut un {1}, mais {2} n'en a pas — autre choix",
          "{0} хочет {1}, но у {2} её нет — другой вариант",
          "{0} は {1} を希望、{2} は持っていない — 別の札へ",
          "{0} quiere un {1}, pero {2} no lo tiene — otra opción",
          "{0} يريد {1} لكن {2} لا يملكها — جرّب غيرها" },

        { "log_gave",
          "{0} 给了 {1} 一张 {2}", "{0} 給了 {1} 一張 {2}", "{0} gave {1} a {2}",
          "{0} donne un {2} à {1}", "{0} отдал {2} игроку {1}", "{0} が {1} に {2} を渡した",
          "{0} le dio a {1} un {2}", "{0} أعطى {1} بطاقة {2}" },

        { "log_refuse",
          "{0} 拒绝了（第 {1} 次）", "{0} 拒絕了（第 {1} 次）", "{0} refused (attempt {1})",
          "{0} refuse (tentative {1})", "{0} отказал (попытка {1})",
          "{0} が拒否（{1}回目）", "{0} se negó (intento {1})",
          "{0} رفض (المحاولة {1})" },

        { "log_force_give",
          "拒绝满 3 次，{0} 随机给了 {1} 一张 {2}", "拒絕滿 3 次，{0} 隨機給了 {1} 一張 {2}",
          "3 refusals — {0} gives {1} a random {2}",
          "3 refus pour {0} — il donne {2} au hasard à {1}",
          "3 отказа — {0} отдаёт {1} случайную {2}",
          "3回拒否 — {0} が {1} に {2} をランダムで渡す",
          "3 negativas — {0} entrega a {1} un {2} al azar",
          "3 رفضات — {0} يعطي {1} بطاقة {2} عشوائية" },

        { "log_round_result",
          "第 {0} 局：{1} 上游 / {2} 中游 / {3} 下游", "第 {0} 局：{1} 上游 / {2} 中游 / {3} 下游",
          "Round {0}: 1st {1} / 2nd {2} / 3rd {3}",
          "Manche {0} : 1er {1} / 2e {2} / 3e {3}",
          "Раунд {0}: 1-й {1} / 2-й {2} / 3-й {3}",
          "第 {0} 局：1位 {1} / 2位 {2} / 3位 {3}",
          "Ronda {0}: 1º {1} / 2º {2} / 3º {3}",
          "الجولة {0}: الأول {1} / الثاني {2} / الثالث {3}" },

        { "notice_round_end",
          "第 {0} 局结束", "第 {0} 局結束", "Round {0} finished", "Fin de la manche {0}",
          "Раунд {0} окончен", "第 {0} 局 終了", "Fin de la ronda {0}",
          "انتهت الجولة {0}" },

        { "lbl_up",
          "上游：{0}", "上游：{0}", "1st: {0}", "1er : {0}", "1-й: {0}",
          "1位：{0}", "1º: {0}", "الأول: {0}" },

        { "lbl_mid",
          "中游：{0}", "中游：{0}", "2nd: {0}", "2e : {0}", "2-й: {0}",
          "2位：{0}", "2º: {0}", "الثاني: {0}" },

        { "lbl_down",
          "下游：{0}", "下游：{0}", "3rd: {0}", "3e : {0}", "3-й: {0}",
          "3位：{0}", "3º: {0}", "الثالث: {0}" },

        { "btn_next",
          "继续下一局", "繼續下一局", "Next round", "Manche suivante", "Следующий раунд",
          "次の局へ", "Siguiente ronda", "الجولة التالية" },

        { "btn_total",
          "查看总成绩", "查看總成績", "Final results", "Résultats finaux", "Итоги",
          "最終結果", "Resultados finales", "النتائج النهائية" },

        { "btn_menu",
          "返回主菜单", "返回主選單", "Main menu", "Menu principal", "В главное меню",
          "メインメニューへ", "Menú principal", "القائمة الرئيسية" },

        { "total_title",
          "五局总成绩", "五局總成績", "Final results", "Résultats des 5 manches", "Итоги 5 раундов",
          "5局の結果", "Resultados de las 5 rondas", "نتائج الجولات الخمس" },

        { "total_line",
          "第 {0} 局：上游 {1}　中游 {2}　下游 {3}", "第 {0} 局：上游 {1}　中游 {2}　下游 {3}",
          "Round {0}: 1st {1}  2nd {2}  3rd {3}", "Manche {0} : 1er {1}  2e {2}  3e {3}",
          "Раунд {0}: 1-й {1}  2-й {2}  3-й {3}", "第 {0} 局：1位 {1}　2位 {2}　3位 {3}",
          "Ronda {0}: 1º {1}  2º {2}  3º {3}", "الجولة {0}: الأول {1}  الثاني {2}  الثالث {3}" },

        { "total_wins",
          "上游次数　{0} {1}　{2} {3}　{4} {5}", "上游次數　{0} {1}　{2} {3}　{4} {5}",
          "1st places: {0} {1} / {2} {3} / {4} {5}",
          "1res places : {0} {1} / {2} {3} / {4} {5}",
          "1-е места: {0} {1} / {2} {3} / {4} {5}",
          "1位の回数：{0} {1}／{2} {3}／{4} {5}",
          "1ºs puestos: {0} {1} / {2} {3} / {4} {5}",
          "مرات المركز الأول: {0} {1} / {2} {3} / {4} {5}" },

        { "notice_tribute",
          "第 {0} 局 · 上贡完成", "第 {0} 局 · 上貢完成", "Round {0} · Tribute done",
          "Manche {0} · Tribut terminé", "Раунд {0} · Взнос выполнен",
          "第 {0} 局 · 上納完了", "Ronda {0} · Tributo hecho",
          "الجولة {0} · تم الإهداء" },

        { "btn_enter_ask",
          "进入要牌", "進入要牌", "Go to requests", "Passer aux demandes", "К запросам",
          "要求へ進む", "Ir a peticiones", "إلى الطلبات" },

        { "tb_step1",
          "第一步 · 上贡（已交牌）", "第一步 · 上貢（已交牌）", "Step 1 · Tribute (done)",
          "Étape 1 · Tribut (fait)", "Шаг 1 · Взнос (выполнен)",
          "第1段階 · 上納（完了）", "Paso 1 · Tributo (hecho)",
          "الخطوة ١ · الإهداء (تم)" },

        { "tb_give1",
          "{0}（下游）最大的 {1} 交给 {2}（中游）", "{0}（下游）最大的 {1} 交給 {2}（中游）",
          "{0} (3rd) gives the highest {1} to {2} (2nd)",
          "{0} (3e) donne son {1} la plus forte à {2} (2e)",
          "{0} (3-е) отдаёт старшую {1} игроку {2} (2-е)",
          "{0}（3位）の最大 {1} を {2}（2位）へ",
          "{0} (3º) entrega su {1} más alta a {2} (2º)",
          "{0} (الثالث) يسلّم أعلى {1} إلى {2} (الثاني)" },

        { "tb_give2",
          "{0}（中游）最大的 {1} 交给 {2}（上游）", "{0}（中游）最大的 {1} 交給 {2}（上游）",
          "{0} (2nd) gives the highest {1} to {2} (1st)",
          "{0} (2e) donne son {1} la plus forte à {2} (1er)",
          "{0} (2-е) отдаёт старшую {1} игроку {2} (1-е)",
          "{0}（2位）の最大 {1} を {2}（1位）へ",
          "{0} (2º) entrega su {1} más alta a {2} (1º)",
          "{0} (الثاني) يسلّم أعلى {1} إلى {2} (الأول)" },

        { "tb_step2",
          "第二步 · 要牌", "第二步 · 要牌", "Step 2 · Requests", "Étape 2 · Demandes",
          "Шаг 2 · Запросы", "第2段階 · 要求", "Paso 2 · Peticiones",
          "الخطوة ٢ · الطلبات" },

        { "tb_ask1",
          "{0} 可向 {1} 要一张牌，", "{0} 可向 {1} 要一張牌，", "{0} may ask {1} for one card,",
          "{0} peut demander une carte à {1},", "{0} может попросить карту у {1},",
          "{0} は {1} に1枚要求できます。", "{0} puede pedir una carta a {1},",
          "{0} يمكنه طلب بطاقة من {1}،" },

        { "tb_ask2",
          "{0} 可向 {1} 要一张牌，用来凑对子 / 三带一。", "{0} 可向 {1} 要一張牌，用來湊對子 / 三帶一。",
          "{0} may ask {1} for one card to complete a pair / triple+1.",
          "{0} peut demander une carte à {1} pour compléter une paire / un triple+1.",
          "{0} может попросить у {1} карту для пары / тройки+1.",
          "{0} は {1} に1枚要求してペア／スリー＋1を完成できます。",
          "{0} puede pedir a {1} una carta para completar pareja / trío+1.",
          "{0} يمكنه طلب بطاقة من {1} لإكمال زوج أو ثلاثية+1." },

        { "tb_note",
          "注意：不能要对方手里最大的那张牌，也不能把刚上贡的牌要回去。",
          "注意：不能要對方手裡最大的那張牌，也不能把剛上貢的牌要回來。",
          "Note: you cannot request the giver's highest card, nor ask back the card just tributed.",
          "Note : interdit de demander la carte la plus forte du donneur ou celle qui vient d'être tributaire.",
          "Внимание: нельзя просить старшую карту отдающего и нельзя требовать обратно только что отданную.",
          "注意：相手の最大の札と、上納したばかりの札は要求できません。",
          "Nota: no se puede pedir la carta más alta del que entrega ni recuperar la recién tributada.",
          "ملاحظة: لا يمكن طلب أعلى بطاقة لدى الطرف الآخر ولا استرجاع البطاقة المهداة للتو." },

        { "notice_trade",
          "第 {0} 局 换牌结果", "第 {0} 局 換牌結果", "Round {0} · Trade results",
          "Manche {0} · Résultat des échanges", "Раунд {0} · Итог обмена",
          "第 {0} 局 交換結果", "Ronda {0} · Resultado del intercambio",
          "الجولة {0} · نتيجة التبادل" },

        { "btn_start_play",
          "开始出牌", "開始出牌", "Start playing", "Jouer", "Начать игру",
          "プレイ開始", "Empezar a jugar", "ابدأ اللعب" },

        { "trade_hands",
          "手牌：{0} {1} 张　{2} {3} 张　{4} {5} 张", "手牌：{0} {1} 張　{2} {3} 張　{4} {5} 張",
          "Hands: {0} {1} · {2} {3} · {4} {5}", "Mains : {0} {1} · {2} {3} · {4} {5}",
          "Руки: {0} {1} · {2} {3} · {4} {5}", "手札：{0} {1} 枚　{2} {3} 枚　{4} {5} 枚",
          "Manos: {0} {1} · {2} {3} · {4} {5}", "الأيدي: {0} {1} · {2} {3} · {4} {5}" },

        { "ask_title",
          "要牌", "要牌", "Request a card", "Demander une carte", "Запрос карты",
          "カード要求", "Pedir una carta", "طلب بطاقة" },

        { "ask_head",
          "向 {0} 要一张牌\r\n（用来凑对子或三带一）", "向 {0} 要一張牌\r\n（用來湊對子或三帶一）",
          "Ask {0} for a card\r\n(to complete a pair or triple+1)",
          "Demander une carte à {0}\r\n(pour une paire ou un triple+1)",
          "Попросить карту у {0}\r\n(для пары или тройки+1)",
          "{0} にカードを要求\r\n（ペア／スリー＋1のため）",
          "Pedir una carta a {0}\r\n(para pareja o trío+1)",
          "اطلب بطاقة من {0}\r\n(لإكمال زوج أو ثلاثية+1)" },

        { "ask_refusals",
          "已被拒绝 {0} 次（满 3 次对方随机给牌）", "已被拒絕 {0} 次（滿 3 次對方隨機給牌）",
          "Refused {0} time(s); after 3 a random card is given",
          "Refusé {0} fois ; au 3e, une carte est donnée au hasard",
          "Отказов: {0}; после 3 выдаётся случайная карта",
          "拒否 {0} 回（3回でランダムに1枚渡されます）",
          "Negado {0} vez/veces; tras 3 se entrega una carta al azar",
          "رُفض {0} مرة؛ بعد ٣ يُعطى بطاقة عشوائية" },

        { "ask_pair",
          "凑对子", "湊對子", "make a pair", "faire une paire", "для пары",
          "ペア作り", "hacer pareja", "لتكوين زوج" },

        { "ask_triple",
          "凑三张", "湊三張", "make a triple", "faire un triple", "для тройки",
          "スリー作り", "hacer trío", "لتكوين ثلاثية" },

        { "ask_cancel",
          "放弃要牌", "放棄要牌", "Skip request", "Renoncer", "Отказаться",
          "要求しない", "Renunciar", "تخلَّ عن الطلب" },

        { "give_title",
          "对方要牌", "對方要牌", "Card request", "Demande d'une carte", "Запрос карты",
          "カード要求", "Petición de carta", "طلب بطاقة" },

        { "give_head",
          "{0} 想要你给一张「{1}」", "{0} 想要你給一張「{1}」", "{0} wants a {1} from you",
          "{0} veut un {1} de votre part", "{0} просит у вас {1}",
          "{0} が「{1}」を1枚求めています", "{0} quiere un {1} de ti",
          "{0} يطلب منك بطاقة {1}" },

        { "give_sub",
          "已拒绝 {0} 次，拒绝满 3 次后系统会随机给他一张", "已拒絕 {0} 次，拒絕滿 3 次後系統會隨機給他一張",
          "Refused {0} time(s); after 3 the game gives a random card",
          "Refusé {0} fois ; au 3e, une carte est donnée au hasard",
          "Отказов: {0}; после 3 выдаётся случайная карта",
          "拒否 {0} 回。3回でランダムに1枚渡されます。",
          "Negado {0} vez/veces; tras 3 se entrega una carta al azar",
          "رُفض {0} مرة؛ بعد ٣ تُعطى بطاقة عشوائية" },

        { "give_yes",
          "给 他", "給 他", "Give", "Donner", "Отдать",
          "渡す", "Dar", "أعطِ" },

        { "give_no",
          "拒 绝", "拒 絕", "Refuse", "Refuser", "Отказать",
          "断る", "Negar", "ارفض" },

        { "lang_title",
          "语言 / Language", "語言 / Language", "Language", "Langue", "Язык",
          "言語", "Idioma", "اللغة" }
    };

    public static string T(string key)
    {
        for (int i = 0; i < TAB.GetLength(0); i++)
        {
            if (TAB[i, 0] == key)
            {
                string s = TAB[i, Idx + 1];
                if (s == null || s.Length == 0) return TAB[i, 1];
                return s;
            }
        }
        return key;
    }

    public static string F(string key, params object[] args)
    {
        try { return string.Format(T(key), args); }
        catch { return T(key); }
    }
}
