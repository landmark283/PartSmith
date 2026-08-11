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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 8。Shadow Step。效果同原版 ShadowStep(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class ShadowStepFragment : EffectCardModelBase
{
    public ShadowStepFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCost => 8;

    protected override CardModel PortraitSourceCard => ModelDb.Card<ShadowStep>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(3),
    };
    public override int UpgradeEnergyGain => 1;

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardCmd.Discard(choiceContext, PileType.Hand.GetPile(cardPlay.Player).Cards);
        await PowerCmd.Apply<ShadowStepPower>(choiceContext, cardPlay.Player.Creature, 1m, cardPlay.Player.Creature, cardPlay.Card);
    }

}
