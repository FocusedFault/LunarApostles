using EntityStates.BrotherMonster;
using EntityStates.LunarWisp;
using EntityStates.ScavMonster;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace LunarApostles
{
  public class DrainSit : BaseSitState
  {
    public override void OnEnter()
    {
      base.OnEnter();
      FireWave(characterBody, damageStat);
      outer.SetNextState(new BaseExitSit());
    }

    private void FireWave(CharacterBody body, float damageStat)
    {
      Util.PlaySound(ExitSkyLeap.soundString, gameObject);
      float num2 = 30f;
      Vector3 vector3 = Vector3.ProjectOnPlane(body.inputBank.aimDirection, Vector3.up);
      Vector3 footPosition = body.footPosition;
      for (int index = 0; index < 12; ++index)
      {
        Vector3 forward = Quaternion.AngleAxis(num2 * index, Vector3.up) * vector3;
        ProjectileManager.instance.FireProjectile(ExitSkyLeap.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), body.gameObject, body.damage * (ExitSkyLeap.waveProjectileDamageCoefficient * 0.5f), ExitSkyLeap.waveProjectileForce, Util.CheckRoll(body.crit, body.master));
      }
      float num3 = 45f;
      for (int index = 0; index < 8; ++index)
      {
        Vector3 forward = Quaternion.AngleAxis(num3 * index, Vector3.up) * vector3;
        ProjectileManager.instance.FireProjectile(FistSlam.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), body.gameObject, body.damage * SeekingBomb.bombDamageCoefficient, FistSlam.waveProjectileForce, Util.CheckRoll(body.crit, body.master));
      }
      for (int index = 0; index < 16; ++index)
      {
        double f = index * 3.14159274101257 * 2.0 / 16.0;
        Vector3 position = transform.position + new Vector3(Mathf.Cos((float)f) * 5f, 0.0f, Mathf.Sin((float)f) * 5f);
        Quaternion rotation = Quaternion.Euler(0.0f, (float)(-f * 57.2957801818848), 0.0f);
        ProjectileManager.instance.FireProjectile(LunarApostles.Instance.vagrantProjectile, position, rotation, gameObject, damageStat * 4f, FireEnergyCannon.force, Util.CheckRoll(critStat, characterBody.master), speedOverride: 45f);
      }
    }
  }
}
