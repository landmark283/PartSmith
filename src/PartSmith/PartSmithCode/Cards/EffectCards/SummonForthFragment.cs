#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 7。Summon Forth。效果同原版 SummonForth(储君)。</summary>
[Pool(typeof(PartSmithWangEffectCardPool))]
public class SummonForthFragment : EffectCardModelBase
{
    public SummonForthFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<SummonForth>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new ForgeVar(8),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Forge" => 3m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        IEnumerable<SovereignBlade> cards = cardPlay.Player.PlayerCombatState.AllCards.OfType<SovereignBlade>().Where(delegate(SovereignBlade c)
        {
            CardPile? pile = c.Pile;
            return pile == null || pile.Type != PileType.Hand;
        });
        await CardPileCmd.Add(cards, PileType.Hand);
        await ForgeCmd.Forge(UpgradedIntValue(cardPlay, base.DynamicVars.Forge.IntValue, 3), cardPlay.Player, cardPlay.Card);
    }

}
