using BepInEx;
using RoR2;
using RoR2.Skills;
using RoR2.CharacterAI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AurelioniteUnchained
{
  [BepInPlugin("com.Nuxlar.AurelioniteUnchained", "AurelioniteUnchained", "1.0.0")]

  public class AbyssalVoidling : BaseUnityPlugin
  {
    public void Awake()
    {
      LaserRework();
      ModifyAI();
      On.RoR2.Run.Start += Run_Start;
      On.RoR2.GoldshoresMissionController.Start += GoldshoresMissionController_Start;
      On.EntityStates.TitanMonster.FireGoldFist.PlacePredictedAttack += FireGoldFist_PlacePredictedAttack;
    }
    private void GoldshoresMissionController_Start(On.RoR2.GoldshoresMissionController.orig_Start orig, GoldshoresMissionController self)
    {
      self.beaconsToSpawnOnMap = 4;
      orig.Invoke(self);
    }
    private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
    {
      orig(self);
    }
    private void LaserRework()
    {
      SetAddressableEntityStateField("RoR2/Base/Titan/EntityStates.TitanMonster.FireGoldMegaLaser.asset", "damageCoefficient", "1.6");    //Vanilla 1
      SetAddressableEntityStateField("RoR2/Base/Titan/EntityStates.TitanMonster.FireGoldMegaLaser.asset", "fireFrequency", "12");    //Vanilla 8
      Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Titan/ChargeGoldLaser.asset").WaitForCompletion().baseRechargeInterval = 15f;   //Vanilla 20
      SetAddressableEntityStateField("RoR2/Base/Titan/EntityStates.TitanMonster.ChargeGoldMegaLaser.asset", "baseDuration", "2"); //Vanilla 3
    }

    private void ModifyAI()
    {
      GameObject masterObject = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Titan/TitanGoldMaster.prefab").WaitForCompletion();

      BaseAI ba = masterObject.GetComponent<BaseAI>();
      ba.aimVectorMaxSpeed = 90f; //Vanilla 180

      AISkillDriver[] aiDrivers = masterObject.GetComponents<AISkillDriver>();
      foreach (AISkillDriver ai in aiDrivers)
      {
        if (ai.skillSlot == SkillSlot.Special)
        {
          ai.minDistance = 0f;
          ai.maxDistance = 200f;
          ai.aimType = AISkillDriver.AimType.AtMoveTarget;    //See if this makes it smoother
          ai.driverUpdateTimerOverride = 10f;  //laser firing = 8s, laser chargeup = 2s
        }
      }
    }

    private void FireGoldFist_PlacePredictedAttack(On.EntityStates.TitanMonster.FireGoldFist.orig_PlacePredictedAttack orig, EntityStates.TitanMonster.FireGoldFist self)
    {
      float num1 = UnityEngine.Random.Range(0.0f, 360f);
      for (int index1 = 0; index1 < 4; ++index1)
      {
        int num2 = 0;
        for (int index2 = 0; index2 < 6; ++index2)
        {
          Vector3 vector3 = Quaternion.Euler(0.0f, num1 + 90f * (float)index1, 0.0f) * Vector3.forward;
          Vector3 position = self.predictedTargetPosition + vector3 * EntityStates.TitanMonster.FireGoldFist.distanceBetweenFists * (float)index2;
          float maxDistance = 60f;
          RaycastHit hitInfo;
          if (Physics.Raycast(new Ray(position + Vector3.up * (maxDistance / 2f), Vector3.down), out hitInfo, maxDistance, (int)LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            position = hitInfo.point;
          self.PlaceSingleDelayBlast(position, EntityStates.TitanMonster.FireGoldFist.delayBetweenFists * (float)num2);
          ++num2;
        }
      }
    }

    public static bool SetAddressableEntityStateField(string fullEntityStatePath, string fieldName, string value)
    {
      EntityStateConfiguration esc = Addressables.LoadAssetAsync<EntityStateConfiguration>(fullEntityStatePath).WaitForCompletion();
      for (int i = 0; i < esc.serializedFieldsCollection.serializedFields.Length; i++)
      {
        if (esc.serializedFieldsCollection.serializedFields[i].fieldName == fieldName)
        {
          esc.serializedFieldsCollection.serializedFields[i].fieldValue.stringValue = value;
          return true;
        }
      }
      return false;
    }

  }
}