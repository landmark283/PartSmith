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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 3。Anticipate。效果同原版 Anticipate(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class AnticipateFragment : EffectCardModelBase
{
    public AnticipateFragment() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCost => 3;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Anticipate>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<DexterityPower>(2m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "DexterityPower" => 2m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        await PowerCmd.Apply<AnticipatePower>(choiceContext, cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars.Dexterity.BaseValue, 2m), cardPlay.Player.Creature, cardPlay.Card);
    }

}
