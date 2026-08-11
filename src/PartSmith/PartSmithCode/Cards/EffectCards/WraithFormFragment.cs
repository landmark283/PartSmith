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

/// <summary>效果卡:点数 18。Wraith Form。效果同原版 WraithForm(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class WraithFormFragment : EffectCardModelBase
{
    public WraithFormFragment() : base(0, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override int PointCost => 18;

    protected override CardModel PortraitSourceCard => ModelDb.Card<WraithForm>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<IntangiblePower>(2m),
        new PowerVar<WraithFormPower>(1m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "IntangiblePower" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "PowerUp", cardPlay.Player.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<IntangiblePower>(choiceContext, cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars["IntangiblePower"].BaseValue, 1m), cardPlay.Player.Creature, cardPlay.Card);
        await PowerCmd.Apply<WraithFormPower>(choiceContext, cardPlay.Player.Creature, base.DynamicVars["WraithFormPower"].BaseValue, cardPlay.Player.Creature, cardPlay.Card);
    }

}
