#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 12。Fan of Knives。效果同原版 FanOfKnives(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class FanOfKnivesFragment : EffectCardModelBase
{
    public FanOfKnivesFragment() : base(0, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCost => 12;

    protected override CardModel PortraitSourceCard => ModelDb.Card<FanOfKnives>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar("Shivs", 4),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Shivs" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FanOfKnivesPower>(choiceContext, cardPlay.Player.Creature, 1m, cardPlay.Player.Creature, cardPlay.Card);
        for (int i = 0; i < UpgradedIntValue(cardPlay, base.DynamicVars["Shivs"].IntValue, 1); i++)
        {
            await Shiv.CreateInHand(cardPlay.Player, cardPlay.Card.CombatState);
            await Cmd.CustomScaledWait(0.1f, 0.2f);
        }
    }

}
