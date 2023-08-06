using EntityStates;
using EntityStates.LunarWisp;
using EntityStates.ScavMonster;
using EntityStates.VagrantMonster.Weapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace LunarApostles
{
  public class ArtilleryBarrage : BaseState
  {
    public static float baseDuration;
    public static string sound;
    public static float damageCoefficient;
    public static string attackSoundString;
    public static float timeToTarget = 3f;
    public static int projectileCount;
    private float missileStopwatch;
    private float duration;
    private ChildLocator childLocator;

    public override void OnEnter()
    {
      base.OnEnter();
      missileStopwatch = 0.0f;
      duration = ThrowSack.baseDuration * 2f / attackSpeedStat;
      Util.PlayAttackSpeedSound(ThrowSack.sound, gameObject, attackSpeedStat);
      PlayAnimation("Body", "ThrowSack", "ThrowSack.playbackRate", duration);
      Transform modelTransform = GetModelTransform();
      if (!(bool)modelTransform)
        return;
      childLocator = modelTransform.GetComponent<ChildLocator>();
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      RainFire(0.2f, 8f, 20f);
      missileStopwatch += Time.deltaTime;
      if (missileStopwatch >= 1.0 / JellyBarrage.missileSpawnFrequency)
      {
        missileStopwatch -= 1f / JellyBarrage.missileSpawnFrequency;
        Transform child = childLocator.FindChild(EnergyCannonState.muzzleName);
        if ((bool)child)
        {
          Ray aimRay = GetAimRay();
          Ray ray = new() { direction = aimRay.direction };
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
      if ((double)fixedAge < duration || !isAuthority)
        return;
      outer.SetNextStateToMain();
    }

    public static void RainFire(float meteorInterval, float meteorRadius, float meteorBaseDamage)
    {
      if (!NetworkServer.active)
        return;
      Vector2 vector2 = 150f * Random.insideUnitCircle;
      Vector3 meteorPosition = new(vector2.x, 0.0f, vector2.y);
      if (Physics.Raycast(new Ray(meteorPosition, Vector3.down), out RaycastHit hitInfo, 500f, (int)LayerIndex.world.mask, QueryTriggerInteraction.UseGlobal))
        meteorPosition.y = hitInfo.point.y;
      meteorPosition += Vector3.up * 0.2f;
      RoR2Application.fixedTimeTimers.CreateTimer(Random.Range(0.0f, meteorInterval), () =>
      {
        EffectManager.SpawnEffect(LunarApostles.Instance.meteorStormController.warningEffectPrefab, new EffectData()
        {
          origin = meteorPosition,
          scale = meteorRadius
        }, true);
        RoR2Application.fixedTimeTimers.CreateTimer(2f, () =>
        {
          EffectManager.SpawnEffect(LunarApostles.Instance.meteorStormController.impactEffectPrefab, new EffectData()
          {
            origin = meteorPosition
          }, true);
          new BlastAttack()
          {
            baseDamage = meteorBaseDamage * (float)(0.800000011920929 + 0.200000002980232 * Run.instance.ambientLevelFloor),
            crit = false,
            falloffModel = BlastAttack.FalloffModel.None,
            bonusForce = Vector3.zero,
            damageColorIndex = DamageColorIndex.Default,
            position = meteorPosition,
            procChainMask = new ProcChainMask(),
            procCoefficient = 1f,
            teamIndex = TeamIndex.Monster,
            radius = meteorRadius
          }.Fire();
        });
      });
    }
  }
}
