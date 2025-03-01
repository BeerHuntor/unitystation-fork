using UnityEngine;

/// <summary>
/// Encapsulates all of the info needed for handling a hand apply interaction.
///
/// A hand apply interaction occurs when a player clicks something in the game world. The object
/// in their hand (or their empty hand) is applied to the target object.
/// </summary>
public class HandApply : BodyPartTargetedInteraction
{
	private static readonly HandApply Invalid = new HandApply(null, null, null, BodyPartType.None, null, Intent.Help, null, false);

	public GameObject HandObject => UsedObject;

	public ItemSlot HandSlot { get; protected set; }

	/// <summary>True if the alt button is pressed by the user. Performed clientside</summary>
	public bool IsAltClick { get; protected set; }

	/// <summary>True if the highlight system is calling it, doesnt get sent to server, clientside only</summary>
	public bool IsHighlight;

	/// <param name="performer">The gameobject of the player performing the drop interaction</param>
	/// <param name="handObject">Object in the player's active hand. Null if player's hand is empty.</param>
	/// <param name="targetObject">Object that the player clicked on</param>
	/// <param name="handSlot">active hand slot that is being used.</param>
	/// <param name="targetBodyPart">targeted body part</param>
	protected HandApply(GameObject performer, GameObject handObject, GameObject targetObject, BodyPartType targetBodyPart,
		ItemSlot handSlot, Intent intent, Mind inMind, bool isAltClick, bool isHighlight = false) :
		base(performer, handObject, targetObject, targetBodyPart, intent, inMind)
	{
		HandSlot = handSlot;
		IsAltClick = isAltClick;
		IsHighlight = isHighlight;
	}

	/// <summary>
	/// Creates a HandApply interaction performed by the local player targeting the specified object.
	/// </summary>
	/// <param name="targetObject">object targeted by the interaction</param>
	public static HandApply ByLocalPlayer(GameObject targetObject)
	{
		if (PlayerManager.LocalPlayerScript.IsGhost)
		{
			//hand apply never works when local player
			return HandApply.Invalid;
		}

		return new HandApply(PlayerManager.LocalPlayerObject,
			PlayerManager.LocalPlayerScript.OrNull()?.DynamicItemStorage.OrNull()?.GetActiveHandSlot()?.ItemObject,
			targetObject,
			UIManager.DamageZone,
			PlayerManager.LocalPlayerScript.OrNull()?.DynamicItemStorage.OrNull()?.GetActiveHandSlot(),
			UIManager.CurrentIntent,
			PlayerManager.LocalPlayerScript.mind ,
			KeyboardInputManager.IsAltActionKeyPressed());
	}

	/// <summary>
	/// For server only. Create a hand apply interaction initiated by the client.
	/// </summary>
	/// <param name="clientPlayer">gameobject of the client's player</param>
	/// <param name="targetObject">object client is targeting.</param>
	/// <param name="targetBodyPart">targeted body part</param>
	/// <param name="handObject">object in the player's active hand. This parameter is used so
	/// it doesn't need to be looked up again, since it already should've been looked up in
	/// the message processing logic. Should match SentByPlayer.Script.playerNetworkActions.GetActiveHandItem().</param>
	/// <param name="handSlot">Player's active hand. This parameter is used so
	/// it doesn't need to be looked up again, since it already should've been looked up in
	/// the message processing logic. Should match SentByPlayer.Script.playerNetworkActions.activeHand.</param>
	/// <returns>a hand apply by the client, targeting the specified object with the item in the active hand</returns>
	public static HandApply ByClient(GameObject clientPlayer, GameObject handObject, GameObject targetObject, BodyPartType targetBodyPart,
		ItemSlot handSlot, Intent intent, Mind inMind, bool isAltClick)
	{
		return new HandApply(clientPlayer, handObject, targetObject, targetBodyPart, handSlot, intent, inMind, isAltClick);
	}
}
