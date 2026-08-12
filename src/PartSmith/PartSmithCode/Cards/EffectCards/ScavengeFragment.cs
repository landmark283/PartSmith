#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
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
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 7。Scavenge。效果同原版 Scavenge(机器人)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class ScavengeFragment : EffectCardModelBase
{
    public ScavengeFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Scavenge>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyVar(2),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Energy" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel cardModel = (await CardSelectCmd.FromHand(choiceContext, cardPlay.Player, new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1), null, cardPlay.Card)).FirstOrDefault();
        if (cardModel != null)
        {
            await CardCmd.Exhaust(choiceContext, cardModel);
        }
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, cardPlay.Player.Creature, UpgradedIntValue(cardPlay, base.DynamicVars.Energy.IntValue, 1), cardPlay.Player.Creature, cardPlay.Card);
    }

}
