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

/// <summary>效果卡:点数 7。Compact。效果同原版 Compact(机器人)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class CompactFragment : EffectCardModelBase
{
    public CompactFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 7;

    public override bool GainsBlock => true;
    protected override CardModel PortraitSourceCard => ModelDb.Card<Compact>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(6m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Block" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        await CreatureCmd.GainBlock(cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars.Block.BaseValue, 1m), base.DynamicVars.Block.Props, cardPlay);
        List<CardModel> list = PileType.Hand.GetPile(cardPlay.Player).Cards.Where((CardModel c) => c != null && c.IsTransformable && c.Type == CardType.Status).ToList();
        List<CardTransformation> list2 = new List<CardTransformation>();
        foreach (CardModel item in list)
        {
            CardModel cardModel = cardPlay.Card.CombatState.CreateCard<Fuel>(cardPlay.Player);
            if (cardPlay.Card.IsUpgraded)
            {
                CardCmd.Upgrade(cardModel);
            }
            list2.Add(new CardTransformation(item, cardModel));
        }
        await CardCmd.Transform(list2, null);
    }

}
