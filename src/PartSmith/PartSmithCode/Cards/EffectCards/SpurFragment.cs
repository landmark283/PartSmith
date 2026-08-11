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
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 7。Spur。效果同原版 Spur(亡灵契约师)。</summary>
[Pool(typeof(PartSmithBoneManEffectCardPool))]
public class SpurFragment : EffectCardModelBase
{
    public SpurFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Spur>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new SummonVar(3m),
        new HealVar(5m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Heal" => 2m, "Summon" => 2m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, Necrobinder.GetSummonAnimIfApplicable(cardPlay.Player.Character), Necrobinder.GetSummonDelayIfApplicable(cardPlay.Player.Character));
        await OstyCmd.Summon(choiceContext, cardPlay.Player, UpgradedValue(cardPlay, base.DynamicVars.Summon.BaseValue, 2m), cardPlay.Card);
        await CreatureCmd.Heal(cardPlay.Player.Osty, UpgradedValue(cardPlay, base.DynamicVars.Heal.BaseValue, 2m));
    }

}
