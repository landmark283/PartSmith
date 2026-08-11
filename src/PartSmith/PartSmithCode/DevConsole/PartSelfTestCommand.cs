using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Exceptions;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Unlocks;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Cards.CostCards;
using PartSmith.PartSmithCode.Cards.Splicing;

namespace PartSmith.PartSmithCode.DevConsole;

/// <summary>
/// 自动化验证用的自测命令(配合 bridge 手动驱动 + 简单脚本)。
/// 在**当前活动战斗**里现造一张指定拼卡并塞进手牌,返回渲染后的标题/描述,
/// 让外部驱动能读到真实显示文本并打牌断言效果。
///    parttest room [&lt;encounterId&gt;]   → 直达测试战斗房间(默认 KNIGHTS_ELITE,正好 3 敌)。
///    parttest maxhp                    → 把当前战斗所有敌人血量顶到 999999999。
///    parttest make &lt;effectId&gt;[,&lt;effectId&gt;...]
///        → 宿主=Scrap(0费),按序拼接所有效果,塞进手牌。
///    parttest make &lt;costCardId&gt; &lt;effectId&gt;[,&lt;effectId&gt;...]
///        → 显式指定宿主费用卡。
///    parttest info &lt;handIdx&gt;   → 打印手牌某张卡的信息。
///    parttest encounters        → 列出全部合法遭遇 id(供驱动选战斗)。
///    parttest library [pool]    → 百科大全(卡牌图鉴)自检:镜像 NCardLibraryGrid 的收卡/可见性逻辑,
///        列出各池在图鉴里应显示的卡及可见状态(LOCKED/NOT_SEEN/VISIBLE),默认 effect。
///        排查"某张卡在图鉴里不显示"最直接的手段(等价于图鉴该池筛选后的可见集)。
/// 注意:拼接**绕过点数容量校验**(调试工具专用;HowlFromBeyond 16 点 &gt; 壳容量 15,
/// 正常拼接拼不上,测试需要不受限)。不战斗时(地图等)改加进牌组。
/// </summary>
public class PartSelfTestCommand : AbstractConsoleCmd
{
    public override string CmdName => "parttest";

    public override string Args => "room [<encounterId>] | maxhp | make [<costCardId>] <effectId[,effectId...]> | info <handIdx> | encounters | library [effect|cost|hunter_effect|hunter_cost|all]";

    public override string Description => "PartSmith self-test: make (splice + add to hand, capacity bypass), info <handIdx>, encounters, library (card library self-check)";

    public override bool IsNetworked => false;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (issuingPlayer == null)
        {
            return new CmdResult(false, "No issuing player.");
        }
        if (!RunManager.Instance.IsInProgress)
        {
            return new CmdResult(false, "A run is currently not in progress!");
        }

        string sub = args.Length > 0 ? args[0].ToLowerInvariant() : "";
        return sub switch
        {
            "room" => Room(args),
            "maxhp" => MaxHp(),
            "make" => Make(issuingPlayer, args),
            "info" => Info(issuingPlayer, args),
            "encounters" => Encounters(),
            "library" => Library(args),
            _ => new CmdResult(false, "Unknown subcommand. Usage: " + Args),
        };
    }

    private CmdResult Make(Player player, string[] args)
    {
        if (args.Length < 2)
        {
            return new CmdResult(false, "Usage: parttest make <effectId>[,<effectId>...] | parttest make <costCardId> <effectId>[,<effectId>...]");
        }

        string effectArg = args[args.Length - 1].ToUpperInvariant();
        string? costId = args.Length >= 3 ? args[1].ToUpperInvariant() : null;
        var effectEntries = effectArg.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToList();
        if (effectEntries.Count == 0)
        {
            return new CmdResult(false, "No effect ids given.");
        }

        // 宿主费用卡:显式指定,或默认 Scrap。
        CostCardModelBase? hostModel;
        if (costId != null)
        {
            hostModel = ModelDb.All.OfType<CostCardModelBase>().FirstOrDefault(c => c.Id.Entry == costId);
            if (hostModel == null)
            {
                return new CmdResult(false, $"Cost card '{costId}' not found. Available: {string.Join(", ", ModelDb.All.OfType<CostCardModelBase>().Select(c => c.Id.Entry))}");
            }
        }
        else
        {
            hostModel = ModelDb.All.OfType<Scrap>().FirstOrDefault();
            if (hostModel == null)
            {
                return new CmdResult(false, "Scrap not found in ModelDb.");
            }
        }

        // 效果卡。
        var effects = new List<EffectCardModelBase>();
        foreach (string id in effectEntries)
        {
            var effect = ModelDb.All.OfType<EffectCardModelBase>().FirstOrDefault(c => c.Id.Entry == id);
            if (effect == null)
            {
                return new CmdResult(false, $"Effect card '{id}' not found. Available: {string.Join(", ", ModelDb.All.OfType<EffectCardModelBase>().Select(c => c.Id.Entry))}");
            }
            effects.Add(effect);
        }

        // 在活动作用域创建实例:战斗中 → 战斗副本,否则 → run 状态(加进牌组)。
        bool inCombat = CombatManager.Instance.IsInProgress;
        ICardScope scope = inCombat
            ? (ICardScope)CombatManager.Instance.DebugOnlyGetState()!
            : (ICardScope)RunManager.Instance.DebugOnlyGetState()!;
        var card = scope.CreateCard(hostModel, player);

        foreach (var effect in effects)
        {
            AttachUnchecked(card, effect);
        }

        // 加进手牌(战斗)或牌组(非战斗)。
        if (inCombat)
        {
            var hand = PileType.Hand.GetPile(player);
            if (hand.Cards.Count >= CardPile.MaxCardsInHand)
            {
                return new CmdResult(false, $"The hand is full ({hand.Cards.Count}/{CardPile.MaxCardsInHand}).");
            }
        }
        PileType targetPile = inCombat ? PileType.Hand : PileType.Deck;
        Task task = CardPileCmd.Add(card, targetPile);

        string desc = card.GetDescriptionForPile(targetPile, null) ?? "";
        return new CmdResult(task, true, $"Added spliced '{card.Title}' to '{targetPile}'.\n{desc}");
    }

    /// <summary>
    /// 直达测试战斗房间(替代外部驱动反复调 <c>fight</c>,减少脚本连发导致的竞态/主线程污染)。
    /// 复用 <c>fight</c> 的 <see cref="RunManager.EnterRoomDebug"/> 路径,直接跳入指定遭遇;
    /// 默认 <c>KNIGHTS_ELITE</c>(正好 3 个敌人)。进房后再调 <c>parttest maxhp</c> 把敌人血量顶到极大,
    /// 得到"3 个几乎打不死的敌人"的测试台。
    /// </summary>
    private CmdResult Room(string[] args)
    {
        if (!RunManager.Instance.IsInProgress)
        {
            return new CmdResult(false, "A run is currently not in progress!");
        }
        string encounterId = args.Length > 1 ? args[1].ToUpperInvariant() : "KNIGHTS_ELITE";
        ModelId modelId = new ModelId(ModelId.SlugifyCategory<EncounterModel>(), encounterId);
        EncounterModel encounterModel;
        try
        {
            encounterModel = ModelDb.GetById<EncounterModel>(modelId).ToMutable();
        }
        catch (ModelNotFoundException)
        {
            return new CmdResult(false, $"Encounter '{encounterId}' not found. See: parttest encounters");
        }
        encounterModel.DebugRandomizeRng();
        Task task = RunManager.Instance.EnterRoomDebug(RoomType.Monster, MapPointType.Unassigned, encounterModel);
        return new CmdResult(task, true, $"Entered test room: '{encounterModel.Id.Entry}' ({encounterModel.MonstersWithSlots.Count} enemies).");
    }

    /// <summary>把当前战斗里所有敌人血量顶到 999999999(≈int 上限)。战斗激活后同步调用,主线程安全。</summary>
    private CmdResult MaxHp()
    {
        if (!CombatManager.Instance.IsInProgress || CombatManager.Instance.DebugOnlyGetState() is not { } combat)
        {
            return new CmdResult(false, "No combat in progress.");
        }
        int count = 0;
        foreach (Creature enemy in combat.Enemies)
        {
            enemy.SetMaxHpInternal(999999999m);
            enemy.SetCurrentHpInternal(999999999m);
            count++;
        }
        return new CmdResult(true, $"Set {count} enemies to 999999999 HP.");
    }

    /// <summary>同 <see cref="SpliceController.AttachEffect"/> 但跳过容量校验(自测专用)。</summary>
    private static void AttachUnchecked(CardModel costCard, EffectCardModelBase effectCard)
    {
        int joinIndex = SpliceController.Attachments(costCard).Count;
        var modifier = (EffectAttachmentModifier)CardModifier.Get<EffectAttachmentModifier>().MutableClone();
        modifier.EffectCardId = effectCard.Id.ToString();
        modifier.JoinIndex = joinIndex;
        modifier.Priority = joinIndex;
        CardModifier.AddModifier(costCard, modifier);

        // 效果卡关键词(Exhaust/Innate 等)转移到宿主拼卡(同 AttachEffect)。
        foreach (CardKeyword keyword in effectCard.Keywords)
        {
            costCard.AddKeyword(keyword);
        }
    }

    private CmdResult Info(Player player, string[] args)
    {
        if (args.Length < 2 || !int.TryParse(args[1], out int index))
        {
            return new CmdResult(false, "Usage: parttest info <handIdx>");
        }
        var hand = PileType.Hand.GetPile(player).Cards;
        if (index < 0 || index >= hand.Count)
        {
            return new CmdResult(false, $"Invalid hand index {index}. Valid range: 0-{hand.Count - 1}.");
        }
        var card = hand[index];
        string keywords = string.Join(",", card.Keywords.Select(k => k.ToString()));
        string points = card is CostCardModelBase cost
            ? $"  points {SpliceController.UsedPoints(card)}/{cost.PointCapacity}"
            : "";
        string desc = card.GetDescriptionForPile(PileType.Hand, null) ?? "";
        return new CmdResult(true, $"[{index}] {card.Title}\nType={card.Type} Target={card.TargetType} Keywords=[{keywords}]{points}\n{desc}");
    }

    private CmdResult Encounters()
    {
        var ids = ModelDb.AllEncounters.Select(e => e.Id.Entry).OrderBy(x => x).ToList();
        return new CmdResult(true, "Encounters:\n" + string.Join("\n", ids));
    }

    /// <summary>
    /// 百科大全(卡牌图鉴)自检:镜像 <c>NCardLibraryGrid</c> 的收卡/可见性逻辑,
    /// 列出各池在图鉴里**应该显示**的卡及其可见状态,等价于图鉴点该池筛选按钮后的可见集。
    /// 收卡 = <c>ModelDb.AllCards.Where(ShouldShowInCardLibrary)</c>;
    /// 状态 = 解锁(<c>GetUnlockedCards</c>)→ 已见(<c>DiscoveredCards</c>)→ 完整,否则 LOCKED / NOT_SEEN。
    /// 用法:parttest library [effect|cost|hunter_effect|hunter_cost|all](默认 effect)。
    /// 排查"某张卡在图鉴里不显示"时,先看它是否出现在本列表;出现再看状态是不是 LOCKED/NOT_SEEN。
    /// </summary>
    private CmdResult Library(string[] args)
    {
        string target = args.Length > 1 ? args[1].ToLowerInvariant() : "effect";
        string[] wanted = target switch
        {
            "cost" => new[] { "PartSmithCostCardPool" },
            "hunter_effect" => new[] { "PartSmithHunterEffectCardPool" },
            "hunter_cost" => new[] { "PartSmithHunterCostCardPool" },
            "all" => new[]
            {
                "PartSmithEffectCardPool", "PartSmithCostCardPool",
                "PartSmithHunterEffectCardPool", "PartSmithHunterCostCardPool",
            },
            _ => new[] { "PartSmithEffectCardPool" },
        };

        UnlockState unlockState = SaveManager.Instance.GenerateUnlockStateFromProgress();
        var unlocked = ModelDb.AllCardPools
            .SelectMany(p => p.GetUnlockedCards(unlockState, CardMultiplayerConstraint.None))
            .ToHashSet();
        var seen = SaveManager.Instance.Progress.DiscoveredCards;

        var inLibrary = ModelDb.AllCards.Where(c => c.ShouldShowInCardLibrary).ToList();
        var lines = new List<string>
        {
            $"AllCards={ModelDb.AllCards.Count()} ShouldShowInCardLibrary={inLibrary.Count}",
        };
        foreach (string poolName in wanted)
        {
            var pool = ModelDb.AllCardPools.FirstOrDefault(p => p.GetType().Name == poolName);
            if (pool == null)
            {
                lines.Add($"[{poolName}] POOL NOT FOUND");
                continue;
            }
            bool seenByDefault = pool is CustomCardPoolModel { SeenByDefault: true };
            var matching = inLibrary.Where(c => c.Pool?.GetType().Name == poolName)
                .OrderBy(c => c.Id.Entry).ToList();
            lines.Add($"[{poolName}] matching={matching.Count} SeenByDefault={seenByDefault}");
            foreach (var c in matching)
            {
                bool isUnlocked = unlocked.Contains(c);
                bool isSeen = seen.Contains(c.Id);
                string state = !isUnlocked ? "LOCKED" : (!isSeen ? "NOT_SEEN" : "VISIBLE");
                lines.Add($"  {c.Id.Entry} | {c.Title} | {c.Type}/{c.Rarity} | pool={c.Pool?.GetType().Name} | {state}");
            }
        }
        return new CmdResult(true, string.Join("\n", lines));
    }
}
