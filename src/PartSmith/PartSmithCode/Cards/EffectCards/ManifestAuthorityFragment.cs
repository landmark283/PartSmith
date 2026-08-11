#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 7。Manifest Authority。效果同原版 ManifestAuthority(储君)。</summary>
[Pool(typeof(PartSmithWangEffectCardPool))]
public class ManifestAuthorityFragment : EffectCardModelBase
{
    public ManifestAuthorityFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 7;

    public override bool GainsBlock => true;
    protected override CardModel PortraitSourceCard => ModelDb.Card<ManifestAuthority>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(7m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Block" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars.Block.BaseValue, 1m), base.DynamicVars.Block.Props, cardPlay);
        CardModel cardModel = CardFactory.GetDistinctForCombat(cardPlay.Player, ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(cardPlay.Player.UnlockState, cardPlay.Player.RunState.CardMultiplayerConstraint), 1, cardPlay.Player.RunState.Rng.CombatCardGeneration).FirstOrDefault();
        if (cardModel != null)
        {
            if (cardPlay.Card.IsUpgraded)
            {
                CardCmd.Upgrade(cardModel);
            }
            await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, cardPlay.Player);
        }
    }

}
