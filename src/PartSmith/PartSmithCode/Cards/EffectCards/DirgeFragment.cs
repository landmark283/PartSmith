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

/// <summary>效果卡:点数 1。Dirge。效果同原版 Dirge(亡灵契约师,X 费)。</summary>
[Pool(typeof(PartSmithBoneManEffectCardPool))]
public class DirgeFragment : EffectCardModelBase
{
    public DirgeFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 1;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Dirge>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new SummonVar(3m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Summon" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        int xValue = await XCostHelper.ResolveAndSpend(cardPlay);
        for (int i = 0; i < xValue; i++)
        {
            await OstyCmd.Summon(choiceContext, cardPlay.Player, UpgradedValue(cardPlay, base.DynamicVars.Summon.BaseValue, 1m), cardPlay.Card);
        }
        var souls = Soul.Create(cardPlay.Player, xValue, cardPlay.Card.CombatState).ToList();
        if (cardPlay.Card.IsUpgraded)
        {
            foreach (var soul in souls)
            {
                CardCmd.Upgrade(soul);
            }
        }
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(souls, PileType.Draw, cardPlay.Player, CardPilePosition.Random));
    }

}
