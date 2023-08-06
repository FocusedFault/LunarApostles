using EntityStates.LunarWisp;
using EntityStates.ScavMonster;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace LunarApostles
{
  public class OrbBarrage : EntityStates.BaseState
  {
    public static float baseDuration;
    public static string sound;
    public static float damageCoefficient;
    public static string attackSoundString;
    public static float timeToTarget = 3f;
    public static int projectileCount;
    private float duration;
    private ChildLocator childLocator;

    public override void OnEnter()
    {
      base.OnEnter();
      duration = ThrowSack.baseDuration / attackSpeedStat;
      Util.PlayAttackSpeedSound(ThrowSack.sound, gameObject, attackSpeedStat);
      PlayAnimation("Body", "ThrowSack", "ThrowSack.playbackRate", duration);
      Transform modelTransform = GetModelTransform();
      if (!(bool)modelTransform)
        return;
      childLocator = modelTransform.GetComponent<ChildLocator>();
      if (!(bool)childLocator)
        return;
      SpawnOrbs();
    }

    private void SpawnOrbs()
    {
      Transform child = childLocator.FindChild(EnergyCannonState.muzzleName);
      if (!(bool)child)
        return;
      for (int index = 0; index < 6; ++index)
      {
        Ray aimRay = GetAimRay();
        Ray ray = new() { direction = aimRay.direction };
        Vector3 vector3_1 = new(Random.Range(-25f, 25f), Random.Range(10f, 25f), Random.Range(-25f, 25f));
        Vector3 vector3_2 = child.position + vector3_1;
        ray.origin = vector3_2;
        EffectManager.SpawnEffect(LunarApostles.Instance.severPrefab, new EffectData()
        {
          origin = ray.origin,
          rotation = Util.QuaternionSafeLookRotation(ray.direction)
        }, false);
        ProjectileManager.instance.FireProjectile(LunarApostles.Instance.trackingProjectile, ray.origin, Util.QuaternionSafeLookRotation(ray.direction), gameObject, damageStat * SeekingBomb.bombDamageCoefficient, SeekingBomb.bombForce, Util.CheckRoll(critStat, characterBody.master), speedOverride: 0.0f);
      }
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if ((double)fixedAge < duration || !isAuthority)
        return;
      outer.SetNextStateToMain();
    }

    public override void OnExit()
    {
      base.OnExit();
      ProjectileSimple[] objectsOfType = Object.FindObjectsOfType<ProjectileSimple>();
      if (objectsOfType.Length == 0)
        return;
      foreach (ProjectileSimple projectileSimple in objectsOfType)
      {
        if (projectileSimple.name == "MITrackingProjectile(Clone)")
          projectileSimple.desiredForwardSpeed = 50f;
      }
    }
  }
}
