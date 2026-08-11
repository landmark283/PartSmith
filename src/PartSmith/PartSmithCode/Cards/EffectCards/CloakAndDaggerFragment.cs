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
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 6。Cloak and Dagger。效果同原版 CloakAndDagger(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class CloakAndDaggerFragment : EffectCardModelBase
{
    public CloakAndDaggerFragment() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCost => 6;

    public override bool GainsBlock => true;
    protected override CardModel PortraitSourceCard => ModelDb.Card<CloakAndDagger>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(6m, ValueProp.Move),
        new CardsVar(1),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Cards" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(cardPlay.Player.Creature, base.DynamicVars.Block, cardPlay);
        for (int i = 0; i < UpgradedIntValue(cardPlay, base.DynamicVars.Cards.IntValue, 1); i++)
        {
            await Shiv.CreateInHand(cardPlay.Player, cardPlay.Card.CombatState);
            await Cmd.Wait(0.25f);
        }
    }

}
