using EntityStates.BrotherMonster;
using EntityStates.LunarWisp;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace LunarApostles
{
  public class MineSit : BaseSitState
  {
    public override void OnEnter()
    {
      base.OnEnter();
      FireWave(characterBody, GetAimRay(), damageStat);
      outer.SetNextState(new BaseExitSit());
    }

    private void FireWave(CharacterBody body, Ray aimRay, float damageStat)
    {
      Util.PlaySound(ExitSkyLeap.soundString, gameObject);
      float num2 = 30f;
      Vector3 vector3_1 = Vector3.ProjectOnPlane(body.inputBank.aimDirection, Vector3.up);
      Vector3 footPosition = body.footPosition;
      for (int index = 0; index < 12; ++index)
      {
        Vector3 forward = Quaternion.AngleAxis(num2 * index, Vector3.up) * vector3_1;
        ProjectileManager.instance.FireProjectile(ExitSkyLeap.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), body.gameObject, body.damage * (ExitSkyLeap.waveProjectileDamageCoefficient * 0.5f), ExitSkyLeap.waveProjectileForce, Util.CheckRoll(body.crit, body.master));
      }
      float num3 = 45f;
      for (int index = 0; index < 8; ++index)
      {
        Vector3 forward = Quaternion.AngleAxis(num3 * index, Vector3.up) * vector3_1;
        ProjectileManager.instance.FireProjectile(FistSlam.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), body.gameObject, body.damage * SeekingBomb.bombDamageCoefficient, FistSlam.waveProjectileForce, Util.CheckRoll(body.crit, body.master));
      }
      for (int index = 0; index < 12; ++index)
      {
        Ray ray = new() { direction = aimRay.direction };
        Vector3 vector3_2 = characterBody.corePosition + new Vector3(Random.Range(-80f, 80f), Random.Range(0.0f, 1f), Random.Range(-80f, 80f));
        ray.origin = vector3_2;
        EffectManager.SpawnEffect(LunarApostles.Instance.severPrefab, new EffectData()
        {
          origin = ray.origin,
          rotation = Util.QuaternionSafeLookRotation(ray.direction)
        }, false);
        ProjectileManager.instance.FireProjectile(LunarApostles.Instance.trackingProjectile, ray.origin, Util.QuaternionSafeLookRotation(ray.direction), gameObject, damageStat * SeekingBomb.bombDamageCoefficient, SeekingBomb.bombForce, Util.CheckRoll(critStat, characterBody.master), speedOverride: 10f);
      }
    }
  }
}
