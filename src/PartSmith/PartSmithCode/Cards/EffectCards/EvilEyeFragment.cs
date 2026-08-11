#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
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

/// <summary>效果卡:点数 7。Evil Eye。效果同原版 EvilEye。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class EvilEyeFragment : EffectCardModelBase
{
    public EvilEyeFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 7;

    public override bool GainsBlock => true;
    protected override CardModel PortraitSourceCard => ModelDb.Card<EvilEye>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(8m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Block" => 3m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        VfxCmd.PlayOnCreatureCenter(cardPlay.Player.Creature, "vfx/vfx_gaze");
        int blockGains = WasCardExhaustedThisTurn(cardPlay) ? 2 : 1;
        for (int i = 0; i < blockGains; i++)
        {
            await CreatureCmd.GainBlock(cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars.Block.BaseValue, 3m), base.DynamicVars.Block.Props, cardPlay);
        }
    }


    private bool WasCardExhaustedThisTurn(CardPlay cardPlay)
        => CombatManager.Instance.History.Entries.OfType<CardExhaustedEntry>().Any(e => e.HappenedThisTurn(cardPlay.Card.CombatState) && e.Actor == cardPlay.Player.Creature);

}
