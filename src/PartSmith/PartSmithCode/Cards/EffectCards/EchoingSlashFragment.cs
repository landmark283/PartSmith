#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
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
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 7。Echoing Slash。效果同原版 EchoingSlash(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class EchoingSlashFragment : EffectCardModelBase
{
    public EchoingSlashFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<EchoingSlash>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(10m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 3m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await using AttackContext attackContext = await AttackCommand.CreateContextAsync(cardPlay.Card.CombatState, choiceContext, cardPlay);
        int attackCount = 1;
        while (attackCount > 0)
        {
            attackCount--;
            IEnumerable<DamageResult> enumerable = await CreatureCmd.Damage(choiceContext, cardPlay.Card.CombatState?.HittableEnemies, base.DynamicVars.Damage, cardPlay.Player.Creature, cardPlay.Card, cardPlay);
            attackContext.AddHit(enumerable);
            attackCount += enumerable.Count((DamageResult r) => r.WasTargetKilled);
        }
    }

}
