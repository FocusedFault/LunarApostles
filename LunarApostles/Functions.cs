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
  public class Functions
  {
    private static readonly Material moonTerrainMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon/matMoonTerrain.mat").WaitForCompletion();
    private static readonly Material artifactDistortionMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/artifactworld/matArtifactShellDistortion.mat").WaitForCompletion();

    public static long GenerateRandomULong(long min, long max, System.Random rand)
    {
      byte[] buffer = new byte[8];
      rand.NextBytes(buffer);
      return Math.Abs(BitConverter.ToInt64(buffer, 0) % (max - min)) + min;
    }

    public static void Beautify(Material[] mats)
    {
      GameObject gameObject = GameObject.Find("HOLDER: Gameplay Space");
      gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = moonTerrainMat;
      for (int index = 1; index < 19; ++index)
        gameObject.transform.GetChild(index).GetChild(0).GetComponent<MeshRenderer>().materials = mats;
    }

    public static void CreateTube()
    {
      GameObject gameObject = new("WallHolder");
      gameObject.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
      GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      primitive.GetComponent<MeshRenderer>().material = artifactDistortionMat;
      UnityEngine.Object.Destroy(primitive.GetComponent<CapsuleCollider>());
      primitive.AddComponent<MeshCollider>();
      primitive.AddComponent<ReverseNormals>();
      primitive.transform.localScale = new Vector3(200f, 60f, 200f);
      primitive.name = "Cheese Deterrent";
      primitive.transform.SetParent(gameObject.transform);
      primitive.transform.localPosition = Vector3.zero;
      primitive.layer = 10;
    }

    public static void HandlePillars()
    {
      GameObject gameObject1 = GameObject.Find("HOLDER: Pillars");
      for (int index1 = 0; index1 < 4; ++index1)
      {
        Transform child = gameObject1.transform.GetChild(index1);
        for (int index2 = 0; index2 < 4; ++index2)
        {
          GameObject gameObject2 = child.GetChild(index2).gameObject;
          if (index2 == 0 && (!LunarApostles.Instance.completedMass && index1 == 1 || !LunarApostles.Instance.completedDesign && index1 == 2 || !LunarApostles.Instance.completedBlood && index1 == 3 || !LunarApostles.Instance.completedSoul && index1 == 0))
            gameObject2.SetActive(true);
          else
            gameObject2.SetActive(false);
        }
      }
    }

    public static void SavePersistentBuffs()
    {
      if (!NetworkServer.active)
        return;
      foreach (PlayerCharacterMasterController instance in PlayerCharacterMasterController.instances)
      {
        CharacterBody body = instance.master?.GetBody();
        if (!(body == null))
        {
          Dictionary<BuffIndex, int> dictionary = new();
          int buffCount1 = body.GetBuffCount(RoR2Content.Buffs.PermanentCurse.buffIndex);
          if (buffCount1 > 0)
            dictionary.Add(RoR2Content.Buffs.PermanentCurse.buffIndex, buffCount1);
          int buffCount2 = body.GetBuffCount(RoR2Content.Buffs.BanditSkull.buffIndex);
          if (buffCount2 > 0)
            dictionary.Add(RoR2Content.Buffs.BanditSkull.buffIndex, buffCount2);
          if (dictionary.Count > 0)
          {
            LunarApostles.Instance.persistentBuffs[instance.master] = dictionary;
            Debug.Log(string.Format("Saved buffs for player `{0}` : Curse={1}, BanditSkulls={2}", instance.GetDisplayName(), buffCount1, buffCount2));
          }
        }
      }
    }

    public static void LoadPersistentBuffs(CharacterBody body)
    {
      if (!NetworkServer.active || body.master == null || !LunarApostles.Instance.persistentBuffs.TryGetValue(body.master, out Dictionary<BuffIndex, int> dictionary))
        return;
      foreach (KeyValuePair<BuffIndex, int> keyValuePair in dictionary)
        body.SetBuffCount(keyValuePair.Key, keyValuePair.Value);
      Debug.Log("Loaded buffs for player `" + body.GetDisplayName() + "`");
      LunarApostles.Instance.persistentBuffs.Remove(body.master);
    }

    public static void SetPosition(Vector3 newPosition, CharacterBody body)
    {
      if (!(bool)body.characterMotor)
        return;
      body.characterMotor.Motor.SetPositionAndRotation(newPosition, Quaternion.identity);
    }

    public static void SetScene(string sceneName)
    {
      if (!(bool)NetworkManagerSystem.singleton)
        throw new ConCommandException("set_scene failed: NetworkManagerSystem is not available.");
      SceneCatalog.GetSceneDefForCurrentScene();
      SceneDef defFromSceneName = SceneCatalog.GetSceneDefFromSceneName(sceneName);
      if (!(bool)defFromSceneName)
        throw new ConCommandException("\"" + sceneName + "\" is not a valid scene.");
      if (NetworkManager.singleton.isNetworkActive)
      {
        if (defFromSceneName.isOfflineScene)
          throw new ConCommandException("Cannot switch to scene \"" + sceneName + "\": Cannot switch to offline-only scene while in a network session.");
      }
      else if (!defFromSceneName.isOfflineScene)
        throw new ConCommandException("Cannot switch to scene \"" + sceneName + "\": Cannot switch to online-only scene while not in a network session.");
      if (NetworkServer.active)
      {
        Debug.LogFormat("Setting server scene to {0}", sceneName);
        NetworkManagerSystem.singleton.ServerChangeScene(sceneName);
      }
      else
      {
        if (NetworkClient.active)
          throw new ConCommandException("Cannot change scene while connected to a remote server.");
        Debug.LogFormat("Setting offline scene to {0}", sceneName);
        NetworkManagerSystem.singleton.ServerChangeScene(sceneName);
      }
    }

    public static void SpawnReward()
    {
      Vector3 vector3 = new(UnityEngine.Random.Range(-3, 3), 2f, UnityEngine.Random.Range(-3, 3));
      Vector3 bagSpawnPos = new(587f, -156f, 640f);
      PickupIndex pickupIndex1 = SelectItem();
      if (!(pickupIndex1 != PickupIndex.none))
        return;
      PickupCatalog.GetPickupDef(pickupIndex1);
      int participatingPlayerCount = Run.instance.participatingPlayerCount;
      if (participatingPlayerCount == 0)
        return;
      int num2 = participatingPlayerCount;
      double angle = 360.0 / num2;
      Vector3 velocity = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
      Vector3 up = Vector3.up;
      Quaternion quaternion = Quaternion.AngleAxis((float)angle, up);
      int num3 = 0;
      while (num3 < num2)
      {
        PickupDropletController.CreatePickupDroplet(pickupIndex1, bagSpawnPos + vector3, velocity);
        ++num3;
        velocity = quaternion * velocity;
      }
    }

    private static PickupIndex SelectItem()
    {
      float greenChance = 40f;
      float pearlChance = 30f;
      float redChance = 15f;
      float shinyPearlChance = 10f;

      Xoroshiro128Plus pillarDropRng = LunarApostles.Instance.pillarDropRng;
      PickupIndex pickupIndex = PickupIndex.none;

      List<PickupIndex> list;

      float total = greenChance + pearlChance + redChance + shinyPearlChance;
      if (pillarDropRng.RangeFloat(0f, total) <= greenChance)// drop green
      {
        list = Run.instance.availableTier1DropList;
      }
      else
      {
        total -= greenChance;
        if (pillarDropRng.RangeFloat(0f, total) <= pearlChance)// drop pearl
          list = new List<PickupIndex> { PickupCatalog.FindPickupIndex(RoR2Content.Items.Pearl.itemIndex) };
        else
        {
          total -= pearlChance;
          if (pillarDropRng.RangeFloat(0f, total) <= redChance) // drop red
            list = Run.instance.availableTier3DropList;
          else
            list = new List<PickupIndex> { PickupCatalog.FindPickupIndex(RoR2Content.Items.ShinyPearl.itemIndex) }; // drop irradiant
        }
      }
      if (list.Count > 0)
        pickupIndex = pillarDropRng.NextElementUniform(list);
      return pickupIndex;
    }
  }
}