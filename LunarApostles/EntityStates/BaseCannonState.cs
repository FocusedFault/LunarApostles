using EntityStates;
using EntityStates.ScavMonster;
using UnityEngine;

namespace LunarApostles
{
  public class BaseCannonState : BaseState
  {
    protected Transform muzzleTransform;

    public override void OnEnter()
    {
      base.OnEnter();
      muzzleTransform = FindModelChild(EnergyCannonState.muzzleName);
    }

    public override void FixedUpdate() => base.FixedUpdate();
    public override void OnExit() => base.OnExit();
    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
  }
}
