using EntityStates.Missions.LunarScavengerEncounter;
using EntityStates.MoonElevator;
using EntityStates.ScavMonster;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;

namespace LunarApostles
{
  public class Hooks
  {
    private readonly Material massMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matMoonbatteryMass.mat").WaitForCompletion();
    private readonly Material designMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matMoonbatteryDesign.mat").WaitForCompletion();
    private readonly Material bloodMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matMoonbatteryBlood.mat").WaitForCompletion();
    private readonly Material glassMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matMoonbatteryGlassOverlay.mat").WaitForCompletion();
    private readonly Material glassDistortionMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matMoonbatteryGlassDistortion.mat").WaitForCompletion();
    private readonly Material moonBridgeMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon/matMoonBridge.mat").WaitForCompletion();

    public Hooks()
    {
      IL.RoR2.CharacterMaster.TryRegenerateScrap += PreventRegenScrap;
      IL.RoR2.SceneDirector.PopulateScene += RemoveExtraLoot;
      On.RoR2.Run.Start += Run_Start;
      On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += AddTimedBuff_BuffDef_float;
      On.RoR2.Run.GenerateStageRNG += Run_GenerateStageRNG;
      On.RoR2.CharacterBody.Start += CharacterBody_Start;
      On.RoR2.ClassicStageInfo.Start += ClassicStageInfo_Start;
      On.RoR2.HoldoutZoneController.Start += HoldoutZoneController_Start;
      On.EntityStates.MoonElevator.MoonElevatorBaseState.OnEnter += MoonElevatorBaseState_OnEnter;
      On.EntityStates.Missions.LunarScavengerEncounter.FadeOut.OnEnter += FadeOut_OnEnter;
      On.EntityStates.ScavMonster.PrepEnergyCannon.OnEnter += PrepEnergyCannon_OnEnter;
      On.EntityStates.ScavMonster.PrepSack.OnEnter += PrepSack_OnEnter;
      On.EntityStates.ScavMonster.EnterSit.OnEnter += EnterSit_OnEnter;
    }

    private void PreventRegenScrap(ILContext il)
    {
      ILCursor ilCursor = new(il);
      if (ilCursor.TryGotoNext(0, x => ILPatternMatchingExt.MatchLdsfld(x, typeof(DLC1Content.Items), "RegeneratingScrapConsumed")))
      {
        ilCursor.Index += 2;
        ilCursor.EmitDelegate<Func<int, int>>(itemCount =>
        {
          if (LunarApostles.Instance.completedPillar || SceneManager.GetActiveScene().name == "limbo")
            itemCount = 0;
          return itemCount;
        });
      }
      else
        Debug.LogWarning("LunarApostles: RegenScrap IL hook failed");
    }

    private void RemoveExtraLoot(ILContext il)
    {
      ILCursor ilCursor = new(il);

      static int ItemFunction(int itemCount)
      {
        if (LunarApostles.Instance.completedPillar)
          itemCount = 0;
        return itemCount;
      }

      if (ilCursor.TryGotoNext(0, x => ILPatternMatchingExt.MatchLdsfld(x, typeof(RoR2Content.Items), "TreasureCache")))
      {
        ilCursor.Index += 2;
        ilCursor.EmitDelegate(ItemFunction);
      }
      else
        Debug.LogWarning("LunarApostles: TreasureCache IL hook failed");

      ilCursor.Index = 0;

      if (ilCursor.TryGotoNext(0, x => ILPatternMatchingExt.MatchLdsfld(x, typeof(DLC1Content.Items), "TreasureCacheVoid")))
      {
        ilCursor.Index += 2;
        ilCursor.EmitDelegate(ItemFunction);
      }
      else
        Debug.LogWarning("LunarApostles: TreasureCacheVoid IL hook failed");

      ilCursor.Index = 0;

      if (ilCursor.TryGotoNext(0, x => ILPatternMatchingExt.MatchLdsfld(x, typeof(DLC1Content.Items), "FreeChest")))
      {
        ilCursor.Index += 2;
        ilCursor.EmitDelegate(ItemFunction);
      }
      else
        Debug.LogWarning("LunarApostles: FreeChest IL hook failed");
    }

    private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
    {
      orig(self);
      LunarApostles.Instance.moonSeed = new ulong?();
      LunarApostles.Instance.completedPillar = false;
    }

    private void AddTimedBuff_BuffDef_float(
      On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig,
      CharacterBody self,
      BuffDef buffDef,
      float duration)
    {
      if (self.name.Contains("ScavLunar") && buffDef == RoR2Content.Buffs.Cripple)
        return;
      orig(self, buffDef, duration);
    }

    private void Run_GenerateStageRNG(On.RoR2.Run.orig_GenerateStageRNG orig, Run self)
    {
      if ((bool)self.nextStageScene && self.nextStageScene.cachedName == "moon2")
        self.stageRngGenerator = new Xoroshiro128Plus(LunarApostles.Instance.moonSeed.Value);
      orig(self);
    }

    private void ClassicStageInfo_Start(On.RoR2.ClassicStageInfo.orig_Start orig, ClassicStageInfo self)
    {
      if (SceneManager.GetActiveScene().name == "limbo")
      {
        Functions.CreateTube();
        if (LunarApostles.Instance.activatedMass)
          Functions.Beautify(new Material[] { massMat });
        if (LunarApostles.Instance.activatedDesign)
          Functions.Beautify(new Material[] { designMat });
        if (LunarApostles.Instance.activatedBlood)
          Functions.Beautify(new Material[] { bloodMat });
        if (LunarApostles.Instance.activatedSoul)
          Functions.Beautify(new Material[] { glassMat, glassDistortionMat });
      }
      if (SceneManager.GetActiveScene().name == "moon2")
      {
        if (!LunarApostles.Instance.moonSeed.HasValue)
          LunarApostles.Instance.moonSeed = new ulong?(Run.instance.stageRngGenerator.nextUlong);
        LunarApostles.Instance.pillarDropRng = new Xoroshiro128Plus((ulong)Functions.GenerateRandomULong(1000000000000000000L, 9000000000000000000L, new System.Random()));
        Run.instance.stageRngGenerator = new Xoroshiro128Plus(LunarApostles.Instance.moonSeed.Value);
        Run.instance.GenerateStageRNG();
        if (LunarApostles.Instance.completedPillar)
          self.sceneDirectorMonsterCredits /= 3;
      }
      orig(self);
    }

    private void MoonElevatorBaseState_OnEnter(
      On.EntityStates.MoonElevator.MoonElevatorBaseState.orig_OnEnter orig,
      MoonElevatorBaseState self)
    {
      orig(self);
      self.outer.SetNextState(new Ready());
    }

    private void FadeOut_OnEnter(On.EntityStates.Missions.LunarScavengerEncounter.FadeOut.orig_OnEnter orig, FadeOut self)
    {
      orig.Invoke(self);
      LunarApostles.Instance.completedPillar = true;
      if (LunarApostles.Instance.activatedMass)
        LunarApostles.Instance.completedMass = true;
      if (LunarApostles.Instance.activatedDesign)
        LunarApostles.Instance.completedDesign = true;
      if (LunarApostles.Instance.activatedBlood)
        LunarApostles.Instance.completedBlood = true;
      if (LunarApostles.Instance.activatedSoul)
        LunarApostles.Instance.completedSoul = true;
      Functions.SavePersistentBuffs();
      Functions.SetScene("moon2");
    }

    private void HoldoutZoneController_Start(
       On.RoR2.HoldoutZoneController.orig_Start orig,
       HoldoutZoneController self)
    {
      orig(self);
      if (!self.name.Contains("MoonBattery"))
        return;
      if (self.name.Contains("Mass"))
        LunarApostles.Instance.activatedMass = true;
      if (self.name.Contains("Design"))
        LunarApostles.Instance.activatedDesign = true;
      if (self.name.Contains("Blood"))
        LunarApostles.Instance.activatedBlood = true;
      if (self.name.Contains("Soul"))
        LunarApostles.Instance.activatedSoul = true;
      Functions.SavePersistentBuffs();
      Functions.SetScene("limbo");
    }

    private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
    {
      orig.Invoke(self);
      string name = SceneManager.GetActiveScene().name;
      if (name == "moon2" && self.isPlayerControlled)
        Functions.HandlePillars();
      if (name == "moon2" && LunarApostles.Instance.completedPillar && self.isPlayerControlled)
      {
        NetworkServer.Spawn(UnityEngine.Object.Instantiate(LunarApostles.Instance.lunarBackpack, new Vector3(587f, -156f, 640f), Quaternion.identity));
        Functions.SetPosition(LunarApostles.Instance.playerSpawnPos, self);
        LunarApostles.Instance.completedPillar = false;
        LunarApostles.Instance.activatedMass = false;
        LunarApostles.Instance.activatedDesign = false;
        LunarApostles.Instance.activatedBlood = false;
        LunarApostles.Instance.activatedSoul = false;
        Functions.LoadPersistentBuffs(self);
        Functions.SpawnReward();
      }
      if (name == "limbo" && self.name.Contains("ScavLunar") && (bool)self.inventory)
      {
        Functions.SetPosition(new Vector3(9.4f, -6.6f, 16.2f), self);
        if (ModConfig.enableAdaptiveArmor.Value)
          self.inventory.GiveItemString(RoR2Content.Items.AdaptiveArmor.name);
        self.inventory.GiveEquipmentString(RoR2Content.Equipment.CrippleWard.name);
      }
      if (!(name == "limbo") || !LunarApostles.Instance.activatedMass && !LunarApostles.Instance.activatedDesign && !LunarApostles.Instance.activatedBlood && !LunarApostles.Instance.activatedSoul || !self.isPlayerControlled)
        return;
      Functions.LoadPersistentBuffs(self);
      Functions.SetPosition(new Vector3(7f, -5f, -98f), self);
    }

    private void PrepEnergyCannon_OnEnter(On.EntityStates.ScavMonster.PrepEnergyCannon.orig_OnEnter orig, PrepEnergyCannon self)
    {
      if (self.characterBody.name.Contains("ScavLunar"))
      {
        if (LunarApostles.Instance.activatedMass)
          self.outer.SetState(new PrepSeveredCannon());
        if (LunarApostles.Instance.activatedDesign)
          self.outer.SetState(new PrepBlunderbuss());
        if (LunarApostles.Instance.activatedBlood)
          self.outer.SetState(new PrepStarCannon());
        if (LunarApostles.Instance.activatedSoul)
          self.outer.SetState(new PrepBlunderbuss());
      }
      else
        orig(self);
    }

    private void PrepSack_OnEnter(On.EntityStates.ScavMonster.PrepSack.orig_OnEnter orig, PrepSack self)
    {
      if (self.characterBody.name.Contains("ScavLunar"))
      {
        if (LunarApostles.Instance.activatedMass)
          self.outer.SetState(new FullHouse());
        if (LunarApostles.Instance.activatedDesign)
          self.outer.SetState(new ArtilleryBarrage());
        if (LunarApostles.Instance.activatedBlood)
          self.outer.SetState(new StarFall());
        if (LunarApostles.Instance.activatedSoul)
          self.outer.SetState(new OrbBarrage());
      }
      else
        orig(self);
    }

    private void EnterSit_OnEnter(On.EntityStates.ScavMonster.EnterSit.orig_OnEnter orig, EnterSit self)
    {
      if (self.characterBody.name.Contains("ScavLunar"))
      {
        if (LunarApostles.Instance.activatedMass)
          self.outer.SetState(new EnterShockwaveSit());
        if (LunarApostles.Instance.activatedDesign)
          self.outer.SetState(new EnterMineSit());
        if (LunarApostles.Instance.activatedBlood)
          self.outer.SetState(new EnterDrainSit());
        if (LunarApostles.Instance.activatedSoul)
          self.outer.SetState(new EnterCrystalSit());
      }
      else
        orig(self);
    }

  }
}