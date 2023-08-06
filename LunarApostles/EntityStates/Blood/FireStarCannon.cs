using EntityStates.ScavMonster;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace LunarApostles
{
  public class FireStarCannon : BaseCannonState
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
      speedOverride = 65f;
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
      FireStarFormation();
    }

    private void FireStarFormation()
    {
      Ray aimRay = GetAimRay();
      float num = 2.5f;
      for (int index = 0; index < 12; ++index)
      {
        Vector3 forward = (double)num * index <= 15.0 ? Quaternion.AngleAxis(num * (float)index, Vector3.up) * aimRay.direction : Quaternion.AngleAxis(-num * (index - 6), Vector3.up) * aimRay.direction;
        ProjectileManager.instance.FireProjectile(LunarApostles.Instance.cannonProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(forward), gameObject, damageStat * 2.5f, FireEnergyCannon.force, Util.CheckRoll(critStat, characterBody.master), speedOverride: speedOverride);
      }
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if ((double)fixedAge >= refireDuration && currentRefire + 1 < FireEnergyCannon.maxRefireCount && isAuthority)
        outer.SetNextState(new FireStarCannon()
        {
          currentRefire = currentRefire + 1
        });
      if ((double)fixedAge < duration || !isAuthority)
        return;
      outer.SetNextStateToMain();
    }
  }
}
