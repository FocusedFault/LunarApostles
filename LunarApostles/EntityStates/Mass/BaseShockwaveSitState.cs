using EntityStates;
using RoR2;
using UnityEngine.Networking;

namespace LunarApostles
{
  public class BaseShockwaveSitState : BaseState
  {
    public override void OnEnter()
    {
      base.OnEnter();
      if (!NetworkServer.active || !(bool)characterBody)
        return;
      characterBody.AddBuff(RoR2Content.Buffs.ArmorBoost);
    }

    public override void OnExit()
    {
      if (NetworkServer.active && (bool)characterBody)
        characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
      base.OnExit();
    }
  }
}