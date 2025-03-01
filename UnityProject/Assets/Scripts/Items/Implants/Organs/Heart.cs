﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Chemistry;

namespace HealthV2
{
	public class Heart : BodyPartFunctionality
	{
		public bool HeartAttack = false;

		public bool CanTriggerHeartAttack = true;

		public int heartAttackThreshold = -80;

		public int SecondsOfRevivePulse = 30;

		public int CurrentPulse = 0;

		private bool alarmedForInternalBleeding = false;

		[SerializeField] private Reagent salt;

		[SerializeField] private float dangerSaltLevel = 20f; //in u

		public override void EmpResult(int strength)
		{
			if (DMMath.Prob(0.5f))
			{
				DoHeartAttack();
			}
			else
			{
				base.EmpResult(strength);
			}
		}

		public override void ImplantPeriodicUpdate()
		{
			base.ImplantPeriodicUpdate();
			if (RelatedPart.HealthMaster.OverallHealth <= heartAttackThreshold)
			{
				if (CanTriggerHeartAttack)
				{
					DoHeartAttack();
					CanTriggerHeartAttack = false;
					CurrentPulse = 0;
					return;
				}

				if (HeartAttack == false)
				{
					CurrentPulse++;
					if (SecondsOfRevivePulse < CurrentPulse)
					{
						DoHeartAttack();
					}
				}
			}
			else if (RelatedPart.HealthMaster.OverallHealth < heartAttackThreshold/2)
			{
				CanTriggerHeartAttack = true;
				CurrentPulse = 0;
			}

			DoHeartBeat();
		}

		public override void RemovedFromBody(LivingHealthMasterBase livingHealth)
		{
			livingHealth.CirculatorySystem.Hearts.Remove(this);
		}

		public override void AddedToBody(LivingHealthMasterBase livingHealth)
		{
			livingHealth.CirculatorySystem.Hearts.Add(this);
		}

		public override void InternalDamageLogic()
		{
			base.InternalDamageLogic();
			if (RelatedPart.CurrentInternalBleedingDamage > 50 && alarmedForInternalBleeding == false)
			{
				Chat.AddActionMsgToChat(RelatedPart.HealthMaster.gameObject,
					$"You feel a sharp pain in your {RelatedPart.gameObject.ExpensiveName()}!",
					$"{RelatedPart.HealthMaster.playerScript.visibleName} holds their {RelatedPart.gameObject.ExpensiveName()} in pain!");
				alarmedForInternalBleeding = true;
			}

			if (RelatedPart.CurrentInternalBleedingDamage > RelatedPart.MaximumInternalBleedDamage)
			{
				DoHeartAttack();
			}
		}

		public void DoHeartBeat()
		{
			//If we actually have a circulatory system.
			if (HeartAttack)
			{
				if (SecondsOfRevivePulse < CurrentPulse) return;
				if (DMMath.Prob(0.1))
				{
					HeartAttack = false;
					alarmedForInternalBleeding = false;
				}

				return;
			}

			if (RelatedPart.HealthMaster.IsDead)
				return; //For some reason the heart will randomly still continue to try and beat after death.
			if (RelatedPart.HealthMaster.CirculatorySystem.BloodPool.MajorMixReagent == salt ||
			    RelatedPart.HealthMaster.CirculatorySystem.BloodPool[salt] > dangerSaltLevel)
			{
				Chat.AddActionMsgToChat(RelatedPart.HealthMaster.gameObject,
					"<color=red>Your body spasms as a jolt of pain surges all over your body then into your heart!</color>",
					$"<color=red>{RelatedPart.HealthMaster.playerScript.visibleName} spasms before holding " +
					$"{RelatedPart.HealthMaster.playerScript.characterSettings.TheirPronoun(RelatedPart.HealthMaster.playerScript)} chest in shock before falling to the ground!</color>");
				RelatedPart.HealthMaster.Death();
			}
		}


		public float CalculateHeartbeat()
		{
			if (HeartAttack)
			{
				return 0;
			}

			//To exclude stuff like hunger and oxygen damage
			var TotalModified = 1f;
			foreach (var modifier in RelatedPart.AppliedModifiers)
			{
				var toMultiply = 1f;
				if (modifier == RelatedPart.DamageModifier)
				{
					toMultiply = Mathf.Max(0f,
						Mathf.Max(RelatedPart.MaxHealth - RelatedPart.TotalDamageWithoutOxyCloneRadStam, 0) / RelatedPart.MaxHealth);
				}
				else if (modifier == RelatedPart.HungerModifier)
				{
					continue;
				}
				else
				{
					toMultiply = Mathf.Max(0f, modifier.Multiplier);
				}

				TotalModified *= toMultiply;
			}

			return TotalModified;
		}

		public void DoHeartAttack()
		{
			HeartAttack = true;
		}
	}
}