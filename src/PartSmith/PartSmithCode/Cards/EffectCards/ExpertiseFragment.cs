#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 7。Expertise。效果同原版 Expertise(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class ExpertiseFragment : EffectCardModelBase
{
    public ExpertiseFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Expertise>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(2),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Cards" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (CardModel item in await CardPileCmd.Draw(choiceContext, UpgradedIntValue(cardPlay, base.DynamicVars.Cards.IntValue, 1), cardPlay.Player))
        {
            CardCmd.ApplySingleTurnRetain(item);
        }
    }

}
