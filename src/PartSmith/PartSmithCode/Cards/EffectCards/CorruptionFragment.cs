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
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 18。Corruption。效果同原版 Corruption。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class CorruptionFragment : EffectCardModelBase
{
    public CorruptionFragment() : base(0, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override int PointCost => 18;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Corruption>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Power", 1m),
    };
    public override int UpgradeEnergyGain => 1;

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        NPowerUpVfx.CreateNormal(cardPlay.Player.Creature);
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "PowerUp", cardPlay.Player.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<CorruptionPower>(choiceContext, cardPlay.Player.Creature, base.DynamicVars["Power"].BaseValue, cardPlay.Player.Creature, cardPlay.Card);
    }

}
