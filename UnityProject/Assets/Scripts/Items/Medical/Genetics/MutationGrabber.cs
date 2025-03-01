using System.Collections;
using System.Collections.Generic;
using Systems.MobAIs;
using UnityEngine;

public class MutationGrabber : MonoBehaviour, IExaminable , ICheckedInteractable<PositionalHandApply>
{

	public List<MutationSO> CarryingMutations = new List<MutationSO>();
	public bool WillInteract(PositionalHandApply interaction, NetworkSide side)
	{
		if (DefaultWillInteract.Default(interaction, side) == false) return false;
		if (interaction.TargetObject == gameObject) return false;
		if (Validations.HasComponent<DinosaurLivingMutationCarrier>(interaction.TargetObject) == false && Validations.HasComponent<DNAConsole>(interaction.TargetObject) == false) return false;
		return true;
	}


	public void ServerPerformInteraction(PositionalHandApply interaction)
	{
		var  DinosaurLivingMutationCarrier = interaction.TargetObject.GetComponent<DinosaurLivingMutationCarrier>();

		if (DinosaurLivingMutationCarrier != null)
		{
			if (DinosaurLivingMutationCarrier.StageSynchronise == (DinosaurLivingMutationCarrier.GrowingStages.Count - 1))
			{
				foreach (var Mutation in DinosaurLivingMutationCarrier.CarryingMutations)
				{
					if (CarryingMutations.Contains(Mutation) == false)
					{
						CarryingMutations.Add(Mutation);
					}
				}
				Chat.AddExamineMsgFromServer(interaction.Performer, $" You scan and add the DNA mutations from {interaction.TargetObject.ExpensiveName()} to the buffer of {this.gameObject.ExpensiveName()} ");

				return;
			}
			else
			{
				string Adding = "";
				var mobfood = DinosaurLivingMutationCarrier.GetComponent<MobExplore>();
				if (mobfood != null)
				{
					if (mobfood.HasFoodPrefereces)
					{
						Adding = "Try feeding them some food Such as " ;
						foreach (var food in mobfood.FoodPreferences)
						{
							Adding += food.name + ", ";
						}
					}
					else
					{
						Adding = "Try feeding them some food";
					}

				}


				Chat.AddExamineMsgFromServer(interaction.Performer, $" The DNA mutations are too unstable from {interaction.TargetObject.ExpensiveName()} needs to become stabilised from growth. " + Adding);

			}

		}
		var DNAConsole = interaction.TargetObject.GetComponent<DNAConsole>();
		if (DNAConsole != null)
		{
			List<MutationSO> mutations = new List<MutationSO>();
			foreach (var Mutation in CarryingMutations)
			{
				if (DNAConsole.UnlockedMutations.Contains(Mutation) == false)
				{
					mutations.Add(Mutation);

				}
			}

			foreach (var mutation in mutations)
			{
				DNAConsole.AddMutationOfficial(mutation);
			}

			Chat.AddExamineMsgFromServer(interaction.Performer, $" You plug-in the {this.gameObject.ExpensiveName()} Into the {interaction.TargetObject.ExpensiveName()} Transferring all the unknownMutations, And clearing the buffer ");
			CarryingMutations.Clear();
		}

	}

	string IExaminable.Examine(Vector3 worldPos)
	{
		if (CarryingMutations.Count == 0)
		{
			return "It's blooming empty";
		}
		else
		{
			var Buildingstring = " Contained within the buffer is  ";
			foreach (var Mutation in CarryingMutations)
			{
				Buildingstring += Mutation.name + ", ";
			}

			Buildingstring += " and That is it ";
			return Buildingstring;
		}
	}
}
