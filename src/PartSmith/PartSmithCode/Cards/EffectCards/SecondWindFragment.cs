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

/// <summary>效果卡:点数 7。Second Wind。效果同原版 SecondWind。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class SecondWindFragment : EffectCardModelBase
{
    public SecondWindFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 7;

    public override bool GainsBlock => true;
    protected override CardModel PortraitSourceCard => ModelDb.Card<SecondWind>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(5m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Block" => 2m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        foreach (CardModel item in GetCards(cardPlay).ToList())
        {
            await CardCmd.Exhaust(choiceContext, item);
            await CreatureCmd.GainBlock(cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars.Block.BaseValue, 2m), base.DynamicVars.Block.Props, cardPlay);
        }
    }


    private static IEnumerable<CardModel> GetCards(CardPlay cardPlay)
        => PileType.Hand.GetPile(cardPlay.Player).Cards.Where(c => c.Type != CardType.Attack);

}
