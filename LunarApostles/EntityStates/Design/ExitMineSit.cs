using EntityStates.ScavMonster;
using RoR2;

namespace LunarApostles
{
  public class ExitMineSit : BaseSitState
  {
    public static float baseDuration;
    public static string soundString;
    private float duration;

    public override void OnEnter()
    {
      base.OnEnter();
      duration = ExitSit.baseDuration / 2f / attackSpeedStat;
      Util.PlaySound(ExitSit.soundString, gameObject);
      PlayCrossfade("Body", "ExitSit", "Sit.playbackRate", duration, 0.1f);
      modelLocator.normalizeToFloor = false;
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if ((double)fixedAge < duration)
        return;
      outer.SetNextStateToMain();
    }
  }
}
