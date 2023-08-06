using EntityStates.BrotherMonster;
using EntityStates.VagrantMonster.Weapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LunarApostles
{
  public class CrystalSit : BaseSitState
  {
    private float stopwatch;
    private float shockwaveStopwatch;
    private float missileStopwatch;
    public static float stormDuration;
    public static float stormToIdleTransitionDuration;
    public static float missileSpawnFrequency = 20f;
    public static int missileTurretCount;
    public static float missileTurretYawFrequency;
    public static float missileTurretPitchFrequency;
    public static float missileTurretPitchMagnitude;
    public static float missileSpeed;
    public static float damageCoefficient;
    public static GameObject projectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLunar/LunarMissileProjectile.prefab").WaitForCompletion();

    public override void OnEnter()
    {
      base.OnEnter();
      Util.PlaySound("Play_moonBrother_phase4_transition", gameObject);
      FireShockwave();
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      stopwatch += Time.fixedDeltaTime;
      shockwaveStopwatch += Time.fixedDeltaTime;
      missileStopwatch += Time.fixedDeltaTime;
      if (shockwaveStopwatch >= 2.0)
      {
        shockwaveStopwatch -= 2f;
        FireShockwave();
      }
      if (missileStopwatch >= 1.0 / missileSpawnFrequency)
      {
        Ray aimRay = GetAimRay();
        missileStopwatch -= 1f / missileSpawnFrequency;
        for (int index = 0; index < JellyStorm.missileTurretCount; ++index)
        {
          float bonusYaw = (float)(360.0 / JellyStorm.missileTurretCount * index + 270.0 * stopwatch);
          FireBlob(new Ray()
          {
            origin = aimRay.origin + new Vector3(0.0f, 5f, 0.0f),
            direction = aimRay.direction
          }, Mathf.Sin(6.283185f * JellyStorm.missileTurretPitchFrequency * stopwatch) * JellyStorm.missileTurretPitchMagnitude, bonusYaw, 75f);
        }
      }
      if (stopwatch < 4.0)
        return;
      outer.SetNextState(new BaseExitSit());
    }

    private void FireShockwave()
    {
      Util.PlaySound(ExitSkyLeap.soundString, gameObject);
      CharacterBody characterBody = this.characterBody;
      float num2 = 30f;
      Vector3 vector3 = Vector3.ProjectOnPlane(characterBody.inputBank.aimDirection, Vector3.up);
      Vector3 footPosition = characterBody.footPosition;
      for (int index = 0; index < 12; ++index)
      {
        Vector3 forward = Quaternion.AngleAxis(num2 * index, Vector3.up) * vector3;
        ProjectileManager.instance.FireProjectile(ExitSkyLeap.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), characterBody.gameObject, characterBody.damage * (ExitSkyLeap.waveProjectileDamageCoefficient * 0.5f), ExitSkyLeap.waveProjectileForce, Util.CheckRoll(characterBody.crit, characterBody.master));
      }
    }

    private void FireBlob(Ray aimRay, float bonusPitch, float bonusYaw, float speed)
    {
      Vector3 forward = Util.ApplySpread(aimRay.direction, 0.0f, 0.0f, 1f, 1f, bonusYaw, bonusPitch);
      ProjectileManager.instance.FireProjectile(LunarApostles.Instance.lunarProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(forward), gameObject, damageStat * 2f, 0.0f, Util.CheckRoll(critStat, characterBody.master), speedOverride: speed);
    }
  }
}
