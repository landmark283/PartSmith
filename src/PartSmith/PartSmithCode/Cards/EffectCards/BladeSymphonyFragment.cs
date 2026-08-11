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

/// <summary>效果卡:点数 11。Blade Symphony。效果同原版 BladeSymphony(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class BladeSymphonyFragment : EffectCardModelBase
{
    public BladeSymphonyFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies)
    {
    }

    public override int PointCost => 11;

    protected override CardModel PortraitSourceCard => ModelDb.Card<BladeSymphony>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(2),
    };
    public override int UpgradeEnergyGain => 1;

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        if (cardPlay.Card.CombatState == null)
        {
            return;
        }
        IEnumerable<Creature> enumerable = from c in cardPlay.Card.CombatState.GetTeammatesOf(cardPlay.Player.Creature)
            where c != null && c.IsAlive && c.IsPlayer
            select c;
        foreach (Creature teammate in enumerable)
        {
            for (int i = 0; i < base.DynamicVars.Cards.IntValue; i++)
            {
                await Shiv.CreateInHand(teammate.Player, cardPlay.Card.CombatState);
                await Cmd.Wait(0.1f);
            }
        }
    }

}
