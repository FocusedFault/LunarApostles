using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using EntityStates.Missions.LunarScavengerEncounter;
using EntityStates.MoonElevator;
using EntityStates.ScavMonster;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Networking;
using RoR2.Projectile;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;

namespace LunarApostles
{
  [BepInPlugin("com.Nuxlar.LunarApostles", "LunarApostles", "1.2.0")]

  public class LunarApostles : BaseUnityPlugin
  {
    internal static LunarApostles Instance { get; private set; }
    public bool activatedMass;
    public bool activatedDesign;
    public bool activatedBlood;
    public bool activatedSoul;
    public bool completedMass;
    public bool completedDesign;
    public bool completedBlood;
    public bool completedSoul;
    public bool completedPillar = false;
    public bool spawnedBag = false;
    public Vector3 playerSpawnPos = new(586f, -156f, 651f);
    public GameObject cannonProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Scav/ScavEnergyCannonProjectile.prefab").WaitForCompletion(), "MICannonProjectile");
    public GameObject vagrantProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Vagrant/VagrantCannon.prefab").WaitForCompletion(), "MIVagrantProjectile");
    public GameObject lunarProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLunar/LunarMissileProjectile.prefab").WaitForCompletion(), "MILunarProjectile");
    public GameObject trackingProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/LunarWispTrackingBomb.prefab").WaitForCompletion(), "MITrackingProjectile");
    public GameObject severPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/moon/MoonExitArenaOrbEffect.prefab").WaitForCompletion();
    public MeteorStormController meteorStormController = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Meteor/MeteorStorm.prefab").WaitForCompletion().GetComponent<MeteorStormController>();
    public Material cannonRedMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Vagrant/matVagrantCannonRed.mat").WaitForCompletion();
    public GameObject vagrantProjectileGhost = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Vagrant/VagrantCannonGhost.prefab").WaitForCompletion(), "MIVagrantProjectileGhost");
    public SceneDef limbo = Addressables.LoadAssetAsync<SceneDef>("RoR2/Base/limbo/limbo.asset").WaitForCompletion();
    public GameObject mithrixBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherBody.prefab").WaitForCompletion();
    public GameObject lunarBackpack = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ScavLunar/ScavLunarBackpack.prefab").WaitForCompletion();
    public GameObject kipkipBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ScavLunar/ScavLunar1Body.prefab").WaitForCompletion();
    public GameObject wipwipBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ScavLunar/ScavLunar2Body.prefab").WaitForCompletion();
    public GameObject twiptwipBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ScavLunar/ScavLunar3Body.prefab").WaitForCompletion();
    public GameObject guraguraBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ScavLunar/ScavLunar4Body.prefab").WaitForCompletion();
    public GameObject kipkipMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ScavLunar/ScavLunar1Master.prefab").WaitForCompletion();
    public GameObject wipwipMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ScavLunar/ScavLunar2Master.prefab").WaitForCompletion();
    public GameObject twiptwipMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ScavLunar/ScavLunar3Master.prefab").WaitForCompletion();
    public GameObject guraguraMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ScavLunar/ScavLunar4Master.prefab").WaitForCompletion();
    public GameObject voidling = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase3.prefab").WaitForCompletion();
    public Dictionary<CharacterMaster, Dictionary<BuffIndex, int>> persistentBuffs = new();
    public ulong? moonSeed = new ulong?();
    public Xoroshiro128Plus pillarDropRng;

    public void Awake()
    {
      Instance = this;
      ModConfig.InitConfig(Config);
      WipeConfig(Config);

      limbo.suppressNpcEntry = true;
      SkillStateSetup();
      ProjectileSetup();
      BodyChanges();
      MasterChanges();

      new Hooks();
    }

    private void WipeConfig(ConfigFile configFile)
    {
      PropertyInfo orphanedEntriesProp = typeof(ConfigFile).GetProperty("OrphanedEntries", BindingFlags.Instance | BindingFlags.NonPublic);
      Dictionary<ConfigDefinition, string> orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(configFile);
      orphanedEntries.Clear();

      configFile.Save();
    }

    private void ProjectileSetup()
    {
      Instance.vagrantProjectileGhost.transform.GetChild(1).GetComponent<MeshRenderer>().material = Instance.cannonRedMat;
      Instance.cannonProjectile.GetComponent<ProjectileController>().cannotBeDeleted = true;
      ProjectileController component = Instance.vagrantProjectile.GetComponent<ProjectileController>();
      component.cannotBeDeleted = true;
      component.ghost = Instance.vagrantProjectileGhost.GetComponent<ProjectileGhostController>();
      component.ghostPrefab = Instance.vagrantProjectileGhost;
      Instance.lunarProjectile.GetComponent<ProjectileController>().cannotBeDeleted = true;
      Instance.trackingProjectile.GetComponent<ProjectileController>().cannotBeDeleted = true;
      Instance.trackingProjectile.GetComponent<CharacterBody>().baseMaxHealth = ModConfig.oBaseHealth.Value;
      Instance.trackingProjectile.GetComponent<CharacterBody>().levelMaxHealth = ModConfig.oLevelHealth.Value;
    }

    private void BodyChanges()
    {
      CharacterBody component1 = Instance.kipkipBody.GetComponent<CharacterBody>();
      Instance.kipkipBody.GetComponent<CharacterMotor>().mass = 5000;
      component1.baseMaxHealth = ModConfig.sBaseHealth.Value;
      component1.levelMaxHealth = ModConfig.sLevelHealth.Value;
      component1.baseDamage = ModConfig.baseDamage.Value;
      component1.levelDamage = ModConfig.levelDamage.Value;
      CharacterBody component2 = Instance.wipwipBody.GetComponent<CharacterBody>();
      Instance.wipwipBody.GetComponent<CharacterMotor>().mass = 5000;
      component2.baseMaxHealth = ModConfig.sBaseHealth.Value;
      component2.levelMaxHealth = ModConfig.sLevelHealth.Value;
      component2.baseDamage = ModConfig.baseDamage.Value;
      component2.levelDamage = ModConfig.levelDamage.Value;
      CharacterBody component3 = Instance.twiptwipBody.GetComponent<CharacterBody>();
      Instance.twiptwipBody.GetComponent<CharacterMotor>().mass = 5000;
      component3.baseMaxHealth = ModConfig.sBaseHealth.Value;
      component3.levelMaxHealth = ModConfig.sLevelHealth.Value;
      component3.baseDamage = ModConfig.baseDamage.Value;
      component3.levelDamage = ModConfig.levelDamage.Value;
      CharacterBody component4 = Instance.guraguraBody.GetComponent<CharacterBody>();
      Instance.guraguraBody.GetComponent<CharacterMotor>().mass = 5000;
      component4.baseMaxHealth = ModConfig.sBaseHealth.Value;
      component4.levelMaxHealth = ModConfig.sLevelHealth.Value;
      component4.baseDamage = ModConfig.baseDamage.Value;
      component4.levelDamage = ModConfig.levelDamage.Value;
    }

    private void MasterChanges()
    {
      for (int i = 0; i < 4; i++)
      {
        GameObject master = i switch
        {
          0 => kipkipMaster,
          1 => wipwipMaster,
          2 => twiptwipMaster,
          _ => guraguraMaster,
        };
        foreach (UnityEngine.Object component in master.GetComponents<GivePickupsOnStart>())
          Destroy(component);
        AISkillDriver aiSkillDriver1 = master.GetComponents<AISkillDriver>().Where(x => x.customName == "UseEquipmentAndFireCannon").First();
        aiSkillDriver1.maxDistance = 160f;
        aiSkillDriver1.maxUserHealthFraction = 0.9f;
        master.GetComponents<AISkillDriver>().Where(x => x.customName == "FireCannon").First().maxDistance = 160f;
        AISkillDriver aiSkillDriver2 = master.GetComponents<AISkillDriver>().Where(x => x.skillSlot == SkillSlot.Secondary).First();
        aiSkillDriver2.maxUserHealthFraction = 0.95f;
        aiSkillDriver2.maxDistance = 160f;
        master.GetComponents<AISkillDriver>().Where(x => x.skillSlot == SkillSlot.Utility).First().maxUserHealthFraction = 0.85f;
      }
    }

    private void SkillStateSetup()
    {
      // severedcannon fullhouse shockwavesit MASS
      // blunderbuss artillerybarrage minesit DESIGN
      // starcannon starfall drainsit BLOOD
      // blunderbuss orbbarrage crystalsit SOUL
      // Base
      ContentAddition.AddEntityState<BaseCannonState>(out _);
      ContentAddition.AddEntityState<BaseSitState>(out _);
      ContentAddition.AddEntityState<BaseExitSit>(out _);
      // Design + Soul
      ContentAddition.AddEntityState<PrepBlunderbuss>(out _);
      ContentAddition.AddEntityState<FireBlunderbuss>(out _);
      // Mass
      ContentAddition.AddEntityState<PrepSeveredCannon>(out _);
      ContentAddition.AddEntityState<FireSeveredCannon>(out _);
      ContentAddition.AddEntityState<FullHouse>(out _);
      ContentAddition.AddEntityState<EnterShockwaveSit>(out _);
      ContentAddition.AddEntityState<ShockwaveSit>(out _);
      // Design
      ContentAddition.AddEntityState<ArtilleryBarrage>(out _);
      ContentAddition.AddEntityState<EnterMineSit>(out _);
      ContentAddition.AddEntityState<MineSit>(out _);
      // Blood
      ContentAddition.AddEntityState<PrepStarCannon>(out _);
      ContentAddition.AddEntityState<FireStarCannon>(out _);
      ContentAddition.AddEntityState<StarFall>(out _);
      ContentAddition.AddEntityState<EnterDrainSit>(out _);
      ContentAddition.AddEntityState<DrainSit>(out _);
      // Soul
      ContentAddition.AddEntityState<OrbBarrage>(out _);
      ContentAddition.AddEntityState<EnterCrystalSit>(out _);
      ContentAddition.AddEntityState<CrystalSit>(out _);
    }

  }
}