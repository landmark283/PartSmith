using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Cards.CostCards;
using PartSmith.PartSmithCode.Cards.Splicing;

namespace PartSmith.PartSmithCode.DevConsole;

/// <summary>
/// 拼接调试命令(M1 验证用,不开奖励 UI):
///   partsplice shell              → 往牌组加一张空壳费用卡(TrinketShell,共享#1)
///   partsplice attach &lt;deckIndex&gt; &lt;effectId&gt; → 把效果卡拼到牌组第 deckIndex 张卡上(超容量会报错)
///   partsplice list               → 列出牌组里的费用卡与已拼效果
/// effectId 用 SCREAMING_SNAKE,如 STRIKE_FRAGMENT / DRAW_FRAGMENT。
/// </summary>
public class SpliceTestCommand : AbstractConsoleCmd
{
    public override string CmdName => "partsplice";

    public override string Args => "shell | attach <deckIndex> <effectId> | list";

    public override string Description => "PartSmith splicing debug: shell (add empty cost card), attach <idx> <effectId>, list";

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
            "shell" => AddShell(issuingPlayer),
            "attach" => Attach(issuingPlayer, args),
            "list" => List(issuingPlayer),
            _ => new CmdResult(false, "Unknown subcommand. Usage: " + Args),
        };
    }

    private CmdResult AddShell(Player player)
    {
        // 注意:不能用 ModelDb.AllCards——自定义池不在 AllCardPools 固定数组里,
        // 自定义卡只注册在 ModelDb.All(_contentById)中。
        var shell = ModelDb.All.OfType<TrinketShell>().FirstOrDefault();
        if (shell == null)
        {
            return new CmdResult(false, "TrinketShell not found in ModelDb.");
        }

        var runState = RunManager.Instance.DebugOnlyGetState();
        if (runState == null)
        {
            return new CmdResult(false, "Run state not available.");
        }

        var card = runState.CreateCard(shell, player);
        Task task = CardPileCmd.Add(card, PileType.Deck);
        return new CmdResult(task, true, $"Added TrinketShell to deck.");
    }

    private CmdResult Attach(Player player, string[] args)
    {
        if (args.Length < 3 || !int.TryParse(args[1], out int index))
        {
            return new CmdResult(false, "Usage: partsplice attach <deckIndex> <effectId>");
        }

        var deck = PileType.Deck.GetPile(player).Cards;
        if (index < 0 || index >= deck.Count)
        {
            return new CmdResult(false, $"Deck index {index} out of range (deck has {deck.Count} cards).");
        }

        var target = deck[index];
        if (target is not CostCardModelBase)
        {
            return new CmdResult(false, $"Deck card at index {index} is not a cost card ({target.Id.Entry}).");
        }

        var costCard = (CostCardModelBase)target;

        string effectId = args[2].ToUpperInvariant();
        var effect = ModelDb.All.OfType<EffectCardModelBase>().FirstOrDefault(c => EntryMatches(c.Id.Entry, effectId));
        if (effect == null)
        {
            string available = string.Join(", ", ModelDb.All.OfType<EffectCardModelBase>().Select(c => c.Id.Entry));
            return new CmdResult(false, $"Effect card '{effectId}' not found. Available: {available}");
        }

        var modifier = SpliceController.AttachEffect(target, effect);
        if (modifier == null)
        {
            int used = SpliceController.UsedPoints(target);
            return new CmdResult(false,
                $"Cannot splice '{effect.Id.Entry}' (cost {effect.PointCost}) onto '{target.Id.Entry}' — used {used}/{costCard.PointCapacity} points.");
        }
        return new CmdResult(true, $"Spliced '{effect.Id.Entry}' onto '{target.Id.Entry}' (join {modifier.JoinIndex}).");
    }

    /// <summary>匹配 id:接受带前缀(<c>PARTSMITH-X</c>)或不带前缀(<c>X</c>)两种写法
    /// (自定义卡 Entry 带 BaseLib 命名空间前缀)。</summary>
    private static bool EntryMatches(string entry, string input)
        => string.Equals(entry, input, System.StringComparison.OrdinalIgnoreCase)
           || entry.EndsWith("-" + input, System.StringComparison.OrdinalIgnoreCase);

    private CmdResult List(Player player)
    {
        var deck = PileType.Deck.GetPile(player).Cards;
        var costCards = deck.OfType<CostCardModelBase>().ToList();
        if (costCards.Count == 0)
        {
            return new CmdResult(true, "No cost cards in deck.");
        }

        var lines = costCards.Select(c =>
        {
            int used = SpliceController.UsedPoints(c);
            string effects = string.Join(", ", SpliceController.AttachedEffects(c).Select(e => e.Id.Entry));
            return $"  {c.Id.Entry}  points {used}/{c.PointCapacity}  effects: [{effects}]";
        });
        return new CmdResult(true, "Cost cards:\n" + string.Join("\n", lines));
    }
}
