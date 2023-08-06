using EntityStates.ScavMonster;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace LunarApostles
{
  public class FireBlunderbuss : BaseCannonState
  {
    public static float baseDuration;
    public static float baseRefireDuration;
    public static string sound;
    public static GameObject effectPrefab;
    public static GameObject projectilePrefab;
    public static float damageCoefficient;
    public static float force;
    public static float minSpread;
    public static float maxSpread;
    public static float recoilAmplitude = 1f;
    public static float projectilePitchBonus;
    public static float projectileYawBonusPerRefire;
    public static int projectileCount;
    public static int maxRefireCount;
    public int currentRefire;
    private float duration;
    private float refireDuration;
    private float speedOverride;
    private float refireDurationBase;

    public override void OnEnter()
    {
      base.OnEnter();
      speedOverride = 75f;
      refireDurationBase = 0.35f;
      duration = FireEnergyCannon.baseDuration / attackSpeedStat;
      refireDuration = refireDurationBase / attackSpeedStat;
      Util.PlayAttackSpeedSound(FireEnergyCannon.sound, gameObject, attackSpeedStat);
      PlayCrossfade("Body", "FireEnergyCannon", "FireEnergyCannon.playbackRate", duration, 0.1f);
      AddRecoil(-2f * FireEnergyCannon.recoilAmplitude, -3f * FireEnergyCannon.recoilAmplitude, -1f * FireEnergyCannon.recoilAmplitude, 1f * FireEnergyCannon.recoilAmplitude);
      if ((bool)FireEnergyCannon.effectPrefab)
        EffectManager.SimpleMuzzleFlash(FireEnergyCannon.effectPrefab, gameObject, EnergyCannonState.muzzleName, false);
      if (!isAuthority)
        return;
      float num2 = currentRefire % 2 == 0 ? 1f : -1f;
      float num3 = Mathf.Ceil(currentRefire / 2f) * FireEnergyCannon.projectileYawBonusPerRefire;
      for (int index = 0; index < FireEnergyCannon.projectileCount * 4; ++index)
      {
        Ray aimRay = GetAimRay();
        aimRay.direction = Util.ApplySpread(aimRay.direction, 0.0f, 10f, 1f, 1f, num2 * num3, FireEnergyCannon.projectilePitchBonus);
        ProjectileManager.instance.FireProjectile(LunarApostles.Instance.cannonProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), gameObject, damageStat * 2f, FireEnergyCannon.force, Util.CheckRoll(critStat, characterBody.master), speedOverride: speedOverride);
      }
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if ((double)fixedAge >= refireDuration && currentRefire + 1 < FireEnergyCannon.maxRefireCount && isAuthority)
        outer.SetNextState(new FireBlunderbuss()
        {
          currentRefire = currentRefire + 1
        });
      if ((double)fixedAge < duration || !isAuthority)
        return;
      outer.SetNextStateToMain();
    }

  }
}