using EntityStates.ScavMonster;
using RoR2;

namespace LunarApostles
{
  public class EnterCrystalSit : BaseSitState
  {
    public static float baseDuration;
    public static string soundString;
    private float duration;

    public override void OnEnter()
    {
      base.OnEnter();
      duration = EnterSit.baseDuration / 2f / attackSpeedStat;
      Util.PlaySound(EnterSit.soundString, gameObject);
      PlayCrossfade("Body", "EnterSit", "Sit.playbackRate", duration, 0.1f);
      modelLocator.normalizeToFloor = true;
      modelLocator.modelTransform.GetComponent<AimAnimator>().enabled = true;
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if ((double)fixedAge < duration)
        return;
      outer.SetNextState(new CrystalSit());
    }
  }
}
