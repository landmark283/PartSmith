#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
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

/// <summary>效果卡:点数 5。Adrenaline。效果同原版 Adrenaline(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class AdrenalineFragment : EffectCardModelBase
{
    public AdrenalineFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCost => 5;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Adrenaline>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyVar(1),
        new CardsVar(2),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Energy" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMe(cardPlay.Player))
        {
            VfxCmd.PlayFullScreenInCombat("vfx/vfx_adrenaline", cardPlay.Player.Creature);
        }
        await PlayerCmd.GainEnergy(UpgradedIntValue(cardPlay, base.DynamicVars.Energy.IntValue, 1), cardPlay.Player);
        await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, cardPlay.Player);
    }

}
