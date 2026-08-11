#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 5。Brand。效果同原版 Brand。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class BrandFragment : EffectCardModelBase
{
    public BrandFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCost => 5;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Brand>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new HpLossVar(1m),
        new PowerVar<StrengthPower>(1m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "StrengthPower" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        VfxCmd.PlayOnCreatureCenter(cardPlay.Player.Creature, "vfx/vfx_bloody_impact");
        await CreatureCmd.Damage(choiceContext, cardPlay.Player.Creature, base.DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, cardPlay.Card, cardPlay);
        CardModel cardModel = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1), context: choiceContext, player: cardPlay.Player, filter: null, source: cardPlay.Card)).FirstOrDefault();
        if (cardModel != null)
        {
            await CardCmd.Exhaust(choiceContext, cardModel);
        }
        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(cardPlay.Player.Creature));
        SfxCmd.Play("event:/sfx/characters/attack_fire");
        await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars.Strength.BaseValue, 1m), cardPlay.Player.Creature, cardPlay.Card);
    }

}
