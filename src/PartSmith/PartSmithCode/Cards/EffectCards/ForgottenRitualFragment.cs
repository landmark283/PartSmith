#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 7。Forgotten Ritual。效果同原版 ForgottenRitual。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class ForgottenRitualFragment : EffectCardModelBase
{
    public ForgottenRitualFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<ForgottenRitual>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyVar(3),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Energy" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (WasCardExhaustedThisTurn(cardPlay))
        {
            NGroundFireVfx child = NGroundFireVfx.Create(cardPlay.Player.Creature, VfxColor.Purple);
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(child);
            await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
            await PlayerCmd.GainEnergy(UpgradedIntValue(cardPlay, base.DynamicVars.Energy.IntValue, 1), cardPlay.Player);
        }
    }


    private bool WasCardExhaustedThisTurn(CardPlay cardPlay)
        => CombatManager.Instance.History.Entries.OfType<CardExhaustedEntry>().Any(e => e.HappenedThisTurn(cardPlay.Card.CombatState) && e.Actor == cardPlay.Player.Creature);

}
