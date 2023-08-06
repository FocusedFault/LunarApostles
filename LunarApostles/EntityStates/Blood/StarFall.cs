using EntityStates;
using EntityStates.ScavMonster;
using EntityStates.TitanMonster;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace LunarApostles
{
  public class StarFall : BaseState
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
      duration = ThrowSack.baseDuration * 2f / attackSpeedStat;
      Util.PlayAttackSpeedSound(ThrowSack.sound, gameObject, attackSpeedStat);
      PlayAnimation("Body", "ThrowSack", "ThrowSack.playbackRate", duration);
      Transform modelTransform = GetModelTransform();
      if (!(bool)modelTransform)
        return;
      childLocator = modelTransform.GetComponent<ChildLocator>();
      if (!(bool)childLocator)
        return;
      PlayerCharacterMasterController instance = PlayerCharacterMasterController.instances[new System.Random().Next(0, PlayerCharacterMasterController.instances.Count - 1)];
      Vector3 pos = new(instance.body.footPosition.x, 100f, instance.body.footPosition.z);
      FireStarFormation(pos);
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if ((double)fixedAge < duration || !isAuthority)
        return;
      outer.SetNextStateToMain();
    }

    private void FireStarFormation(Vector3 pos)
    {
      float num = Random.Range(0.0f, 360f);
      for (int index1 = 0; index1 < 8; ++index1)
      {
        for (int index2 = 0; index2 < 6; ++index2)
        {
          Vector3 vector3 = Quaternion.Euler(0.0f, num + 45f * index1, 0.0f) * Vector3.forward;
          Vector3 position = pos + vector3 * FireGoldFist.distanceBetweenFists * index2;
          ProjectileManager.instance.FireProjectile(LunarApostles.Instance.vagrantProjectile, position, Util.QuaternionSafeLookRotation(Vector3.down), gameObject, damageStat * 4f, FireEnergyCannon.force, Util.CheckRoll(critStat, characterBody.master), speedOverride: 100f);
        }
      }
    }
  }
}
