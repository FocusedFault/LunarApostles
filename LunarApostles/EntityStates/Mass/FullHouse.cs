using EntityStates.LunarWisp;
using EntityStates.ScavMonster;
using EntityStates.VagrantMonster.Weapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace LunarApostles
{
  public class FullHouse : EntityStates.BaseState
  {
    public static float baseDuration;
    public static string sound;
    public static float damageCoefficient;
    public static string attackSoundString;
    public static float timeToTarget = 3f;
    public static int projectileCount;
    private float duration;
    private float orbStopwatch;
    private float missileStopwatch;
    private ChildLocator childLocator;

    public override void OnEnter()
    {
      base.OnEnter();
      missileStopwatch = 0.0f;
      orbStopwatch = 0.0f;
      duration = 4f / attackSpeedStat;
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
      if (!(bool)childLocator.FindChild(EnergyCannonState.muzzleName))
        return;
      for (int index = 0; index < 16; ++index)
      {
        double f = index * 3.14159274101257 * 2.0 / 16.0;
        Vector3 position = transform.position + new Vector3(Mathf.Cos((float)f) * 5f, 0.0f, Mathf.Sin((float)f) * 5f);
        Quaternion rotation = Quaternion.Euler(0.0f, (float)(-f * 57.2957801818848), 0.0f);
        ProjectileManager.instance.FireProjectile(LunarApostles.Instance.vagrantProjectile, position, rotation, gameObject, damageStat * SeekingBomb.bombDamageCoefficient, FireEnergyCannon.force, Util.CheckRoll(critStat, characterBody.master), speedOverride: 45f);
      }
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      missileStopwatch += Time.fixedDeltaTime;
      orbStopwatch += Time.fixedDeltaTime;
      if (orbStopwatch >= 2.0)
      {
        orbStopwatch -= 2f;
        SpawnOrbs();
      }
      if (missileStopwatch >= 1.0 / (JellyBarrage.missileSpawnFrequency * 2.0))
      {
        missileStopwatch -= (float)(1.0 / (JellyBarrage.missileSpawnFrequency * 2.0));
        Transform child = childLocator.FindChild(EnergyCannonState.muzzleName);
        if ((bool)child)
        {
          Ray aimRay = GetAimRay();
          Ray ray = new();
          ray.direction = aimRay.direction;
          float maxDistance = 1000f;
          Vector3 vector3_1 = new(Random.Range(-25f, 25f), Random.Range(10f, 25f), Random.Range(-25f, 25f));
          Vector3 vector3_2 = child.position + vector3_1;
          ray.origin = vector3_2;
          if (Physics.Raycast(aimRay, out RaycastHit hitInfo, maxDistance, (int)LayerIndex.world.collisionMask))
          {
            ray.direction = hitInfo.point - ray.origin;
            EffectManager.SpawnEffect(LunarApostles.Instance.severPrefab, new EffectData()
            {
              origin = ray.origin,
              rotation = Util.QuaternionSafeLookRotation(ray.direction)
            }, false);
            ProjectileManager.instance.FireProjectile(LunarApostles.Instance.vagrantProjectile, ray.origin, Util.QuaternionSafeLookRotation(ray.direction), gameObject, damageStat * 4f, SeekingBomb.bombForce, Util.CheckRoll(critStat, characterBody.master), speedOverride: 125f);
          }
        }
      }
      if ((double)fixedAge < duration)
        return;
      outer.SetNextStateToMain();
    }
  }
}
