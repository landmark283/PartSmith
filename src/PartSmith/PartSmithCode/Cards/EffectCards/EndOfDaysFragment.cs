#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
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
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 17。End of Days。效果同原版 EndOfDays(亡灵契约师)。</summary>
[Pool(typeof(PartSmithBoneManEffectCardPool))]
public class EndOfDaysFragment : EffectCardModelBase
{
    public EndOfDaysFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    public override int PointCost => 17;

    protected override CardModel PortraitSourceCard => ModelDb.Card<EndOfDays>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<DoomPower>(29m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "DoomPower" => 8m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        Vector2? sideCenterFloor = VfxCmd.GetSideCenterFloor(CombatSide.Enemy, cardPlay.Card.CombatState);
        if (sideCenterFloor.HasValue)
        {
            NLargeMagicMissileVfx nLargeMagicMissileVfx = NLargeMagicMissileVfx.Create(sideCenterFloor.Value, new Color("8c2447"));
            if (nLargeMagicMissileVfx != null)
            {
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nLargeMagicMissileVfx);
                await Cmd.Wait(nLargeMagicMissileVfx.WaitTime);
            }
        }
        foreach (Creature hittableEnemy in cardPlay.Card.CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<DoomPower>(choiceContext, hittableEnemy, UpgradedValue(cardPlay, base.DynamicVars.Doom.BaseValue, 8m), cardPlay.Player.Creature, cardPlay.Card);
        }
        await DoomPower.DoomKill(DoomPower.GetDoomedCreatures(cardPlay.Card.CombatState.HittableEnemies));
    }

}
