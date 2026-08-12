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

/// <summary>效果卡:点数 11。Bulk Up。效果同原版 BulkUp(机器人)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class BulkUpFragment : EffectCardModelBase
{
    public BulkUpFragment() : base(0, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 11;

    protected override CardModel PortraitSourceCard => ModelDb.Card<BulkUp>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("OrbSlots", 1m),
        new PowerVar<StrengthPower>(2m),
        new PowerVar<DexterityPower>(2m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "DexterityPower" => 1m, "StrengthPower" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "PowerUp", cardPlay.Player.Character.PowerUpAnimDelay);
        OrbCmd.RemoveSlots(cardPlay.Player, base.DynamicVars["OrbSlots"].IntValue);
        await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars.Strength.BaseValue, 1m), cardPlay.Player.Creature, cardPlay.Card);
        await PowerCmd.Apply<DexterityPower>(choiceContext, cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars.Dexterity.BaseValue, 1m), cardPlay.Player.Creature, cardPlay.Card);
    }

}
