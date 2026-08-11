#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
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

/// <summary>
/// 效果卡:点数 5。Misery。效果同原版 Misery(亡灵契约师,完整移植,不简化):
/// 造成伤害后,把目标身上的所有 debuff 复制/叠加到所有其他敌人(临时 debuff 合并到
/// 其底层 power,与原版一致)。cardSource 用宿主拼卡(cardPlay.Card)。
/// </summary>
[Pool(typeof(PartSmithBoneManEffectCardPool))]
public class MiseryFragment : EffectCardModelBase
{
    public MiseryFragment() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 5;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Misery>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(7m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 2m,
        _ => 0m,
    };

    /// <summary>原版 Misery 升级时额外获得 Retain,由 EffectAttachmentModifier.OnUpgrade 补加到宿主。</summary>
    public override CardKeyword? UpgradeKeyword => CardKeyword.Retain;

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }

        // 快照目标身上的全部 debuff(克隆成独立 power 实例,量级各自记录)。
        Dictionary<PowerModel, int> debuffAmounts = (from p in cardPlay.Target.Powers
            where p.TypeForCurrentAmount == PowerType.Debuff
            select ((PowerModel)p.ClonePreservingMutability(), Amount: p.Amount)).ToDictionary();

        // 临时 debuff(如易伤叠加层)合并到其底层 power,避免对同一敌人重复施加两份。
        foreach (KeyValuePair<PowerModel, int> item in debuffAmounts)
        {
            PowerModel key = item.Key;
            if (key is ITemporaryPower temporaryPower)
            {
                KeyValuePair<PowerModel, int> merged = debuffAmounts.FirstOrDefault(
                    p => p.Key.Id == temporaryPower.InternallyAppliedPower.Id);
                if (merged.Key != null)
                {
                    debuffAmounts[merged.Key] += item.Value;
                }
            }
        }

        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 2m)).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 把每份 debuff 施加给除目标外的所有可攻击敌人(已有则叠加,没有则新建)。
        foreach (Creature enemy in cardPlay.Card.CombatState.HittableEnemies)
        {
            if (enemy == cardPlay.Target)
            {
                continue;
            }
            foreach (KeyValuePair<PowerModel, int> item2 in debuffAmounts)
            {
                if (item2.Value == 0)
                {
                    continue;
                }
                PowerModel powerModel = PowerCmd.FindExistingInstanceForStacking(item2.Key, enemy, item2.Key.Applier);
                if (powerModel != null)
                {
                    await PowerCmd.ModifyAmount(choiceContext, powerModel, item2.Value, item2.Key.Applier, cardPlay.Card);
                }
                else
                {
                    PowerModel power = (PowerModel)item2.Key.ClonePreservingMutability();
                    await PowerCmd.Apply(choiceContext, power, enemy, item2.Value, item2.Key.Applier, cardPlay.Card);
                }
            }
        }
    }
}
