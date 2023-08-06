using EntityStates.ScavMonster;
using RoR2;
using UnityEngine;

namespace LunarApostles
{
  public class PrepBlunderbuss : BaseCannonState
  {
    public static float baseDuration;
    public static string sound;
    public static GameObject chargeEffectPrefab;
    private GameObject chargeInstance;
    private float duration;

    public override void OnEnter()
    {
      base.OnEnter();
      duration = PrepEnergyCannon.baseDuration / 2f / attackSpeedStat;
      PlayCrossfade("Body", "PrepEnergyCannon", "PrepEnergyCannon.playbackRate", duration, 0.1f);
      Util.PlaySound(PrepEnergyCannon.sound, gameObject);
      if (!(bool)muzzleTransform || !(bool)PrepEnergyCannon.chargeEffectPrefab)
        return;
      chargeInstance = Object.Instantiate<GameObject>(PrepEnergyCannon.chargeEffectPrefab, muzzleTransform.position, muzzleTransform.rotation);
      chargeInstance.transform.parent = muzzleTransform;
      ScaleParticleSystemDuration component = chargeInstance.GetComponent<ScaleParticleSystemDuration>();
      if (!(bool)component)
        return;
      component.newDuration = duration;
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      StartAimMode(0.5f);
      if ((double)fixedAge < duration || !isAuthority)
        return;
      outer.SetNextState(new FireBlunderbuss());
    }

    public override void OnExit()
    {
      base.OnExit();
      if (!(bool)chargeInstance)
        return;
      Destroy(chargeInstance);
    }
  }
}