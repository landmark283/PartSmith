using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace PartSmith.PartSmithCode.Cards;

/// <summary>
/// 原型卡(里程碑 1):0 费,打出直接获胜。
/// 实现方式 = 游戏官方 "win" 控制台命令的等价逻辑:
/// 杀掉所有主要敌人 + 显式判胜(每个动作结束后游戏本来也会自动判胜)。
/// </summary>
[Pool(typeof(IroncladCardPool))]
public class InstantWinCard : PartSmithCard
{
    public InstantWinCard() : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = CombatManager.Instance.DebugOnlyGetState()?.Enemies;
        if (enemies == null)
        {
            return;
        }

        foreach (var enemy in enemies.ToList())
        {
            await CreatureCmd.Kill(enemy);
        }

        await CombatManager.Instance.CheckWinCondition();
    }
}
