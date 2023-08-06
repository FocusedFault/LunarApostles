using EntityStates.ScavMonster;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace LunarApostles
{
  public class FireSeveredCannon : SeveredCannonState
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
    private ChildLocator childLocator;
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
      Transform modelTransform = GetModelTransform();
      if (!(bool)modelTransform)
        return;
      childLocator = modelTransform.GetComponent<ChildLocator>();
      if (!(bool)childLocator)
        return;
      duration = FireEnergyCannon.baseDuration / attackSpeedStat;
      refireDuration = refireDurationBase / attackSpeedStat;
      Util.PlayAttackSpeedSound(FireEnergyCannon.sound, gameObject, attackSpeedStat);
      PlayCrossfade("Body", "FireEnergyCannon", "FireEnergyCannon.playbackRate", duration, 0.1f);
      AddRecoil(-2f * FireEnergyCannon.recoilAmplitude, -3f * FireEnergyCannon.recoilAmplitude, -1f * FireEnergyCannon.recoilAmplitude, 1f * FireEnergyCannon.recoilAmplitude);
      if ((bool)FireEnergyCannon.effectPrefab)
        EffectManager.SimpleMuzzleFlash(LunarApostles.Instance.severPrefab, gameObject, EnergyCannonState.muzzleName, false);
      if (!isAuthority)
        return;
      Transform child = childLocator.FindChild(EnergyCannonState.muzzleName);
      if (!(bool)child)
        return;
      Ray aimRay = GetAimRay();
      Ray projectileRay = new() { direction = aimRay.direction };
      float maxDistance = 1000f;
      Vector3 vector3_1 = new(Random.Range(-15f, 15f), Random.Range(10f, 15f), Random.Range(-15f, 15f));
      Vector3 vector3_2 = child.position + vector3_1;
      projectileRay.origin = vector3_2;
      if (!Physics.Raycast(aimRay, out RaycastHit hitInfo, maxDistance, (int)LayerIndex.world.collisionMask))
        return;
      projectileRay.direction = hitInfo.point - projectileRay.origin;
      FireCannon(projectileRay);
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if ((double)fixedAge >= refireDuration && currentRefire + 1 < FireEnergyCannon.maxRefireCount && isAuthority)
        outer.SetNextState(new FireSeveredCannon()
        {
          currentRefire = currentRefire + 1
        });
      if ((double)fixedAge < duration || !isAuthority)
        return;
      outer.SetNextStateToMain();
    }

    private void FireCannon(Ray projectileRay)
    {
      EffectManager.SpawnEffect(LunarApostles.Instance.severPrefab, new EffectData()
      {
        origin = projectileRay.origin,
        rotation = Util.QuaternionSafeLookRotation(projectileRay.direction)
      }, false);
      for (int index = 0; index < FireEnergyCannon.projectileCount; ++index)
      {
        projectileRay.direction = Util.ApplySpread(projectileRay.direction, FireEnergyCannon.minSpread, FireEnergyCannon.maxSpread, 1f, 1f, 1f, FireEnergyCannon.projectilePitchBonus);
        ProjectileManager.instance.FireProjectile(LunarApostles.Instance.cannonProjectile, projectileRay.origin, Util.QuaternionSafeLookRotation(projectileRay.direction), gameObject, damageStat * 3f, FireEnergyCannon.force, Util.CheckRoll(critStat, characterBody.master), speedOverride: speedOverride);
      }
    }
  }
}
