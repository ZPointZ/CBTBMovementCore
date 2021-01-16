using System;
using System.Collections.Generic;
using BattleTech.Serialization;
using UnityEngine;

namespace BattleTech
{
	// Token: 0x02000F9C RID: 3996
	[SerializableContract("AbstractActorMovementInvocation")]
	public class AbstractActorMovementInvocation : InvocationMessage
	{
		// Token: 0x1700171F RID: 5919
		// (get) Token: 0x0600865D RID: 34397 RVA: 0x0022E368 File Offset: 0x0022C568
		public override MessageCenterMessageType MessageType
		{
			get
			{
				return MessageCenterMessageType.AbstractActorMovementInvocation;
			}
		}

		// Token: 0x17001720 RID: 5920
		// (get) Token: 0x0600865E RID: 34398 RVA: 0x0022E36F File Offset: 0x0022C56F
		// (set) Token: 0x0600865F RID: 34399 RVA: 0x0022E377 File Offset: 0x0022C577
		[SerializableMember(SerializationTarget.Networking)]
		public string ActorGUID { get; private set; }

		// Token: 0x17001721 RID: 5921
		// (get) Token: 0x06008660 RID: 34400 RVA: 0x0022E380 File Offset: 0x0022C580
		// (set) Token: 0x06008661 RID: 34401 RVA: 0x0022E388 File Offset: 0x0022C588
		[SerializableMember(SerializationTarget.Networking)]
		public bool AbilityConsumesFiring { get; private set; }

		// Token: 0x17001722 RID: 5922
		// (get) Token: 0x06008662 RID: 34402 RVA: 0x0022E391 File Offset: 0x0022C591
		// (set) Token: 0x06008663 RID: 34403 RVA: 0x0022E399 File Offset: 0x0022C599
		[SerializableMember(SerializationTarget.Networking)]
		public List<WayPoint> Waypoints { get; private set; }

		// Token: 0x17001723 RID: 5923
		// (get) Token: 0x06008664 RID: 34404 RVA: 0x0022E3A2 File Offset: 0x0022C5A2
		// (set) Token: 0x06008665 RID: 34405 RVA: 0x0022E3AA File Offset: 0x0022C5AA
		[SerializableMember(SerializationTarget.Networking)]
		public MoveType MoveType { get; private set; }

		// Token: 0x17001724 RID: 5924
		// (get) Token: 0x06008666 RID: 34406 RVA: 0x0022E3B3 File Offset: 0x0022C5B3
		// (set) Token: 0x06008667 RID: 34407 RVA: 0x0022E3BB File Offset: 0x0022C5BB
		[SerializableMember(SerializationTarget.Networking)]
		public Vector3 FinalOrientation { get; private set; }

		// Token: 0x17001725 RID: 5925
		// (get) Token: 0x06008668 RID: 34408 RVA: 0x0022E3C4 File Offset: 0x0022C5C4
		// (set) Token: 0x06008669 RID: 34409 RVA: 0x0022E3CC File Offset: 0x0022C5CC
		[SerializableMember(SerializationTarget.Networking)]
		public string MeleeTargetGUID { get; private set; }

		// Token: 0x0600866A RID: 34410 RVA: 0x0022E3D8 File Offset: 0x0022C5D8
		public AbstractActorMovementInvocation(AbstractActor actor, bool abilityConsumesFiring) : base(UnityEngine.Random.Range(0, 99999))
		{
			Pathing pathing = actor.Pathing;
			this.ActorGUID = actor.GUID;
			this.AbilityConsumesFiring = abilityConsumesFiring;
			List<WayPoint> collection = ActorMovementSequence.ExtractWaypointsFromPath(actor, pathing.CurrentPath, pathing.ResultDestination, pathing.CurrentMeleeTarget, this.MoveType);
			this.Waypoints = new List<WayPoint>(collection);
			this.MoveType = pathing.MoveType;
			this.FinalOrientation = pathing.ResultAngleAsVector;
			if (pathing.CurrentMeleeTarget == null)
			{
				this.MeleeTargetGUID = string.Empty;
				return;
			}
			this.MeleeTargetGUID = pathing.CurrentMeleeTarget.GUID;
		}

		// Token: 0x0600866B RID: 34411 RVA: 0x0022E478 File Offset: 0x0022C678
		public AbstractActorMovementInvocation(string actorGUID, bool abilityConsumesFiring, List<WayPoint> waypoints, MoveType moveType, Vector3 orientation, string meleeTargetGUID) : base(UnityEngine.Random.Range(0, 99999))
		{
			this.ActorGUID = actorGUID;
			this.AbilityConsumesFiring = abilityConsumesFiring;
			this.Waypoints = new List<WayPoint>(waypoints);
			this.FinalOrientation = orientation;
			this.MoveType = moveType;
			this.MeleeTargetGUID = meleeTargetGUID;
		}

		// Token: 0x0600866C RID: 34412 RVA: 0x0022E4C8 File Offset: 0x0022C6C8
		protected AbstractActorMovementInvocation()
		{
		}

		// Token: 0x0600866D RID: 34413 RVA: 0x0022E4D0 File Offset: 0x0022C6D0
		public override bool Invoke(CombatGameState combatGameState)
		{
			InvocationMessage.logger.Log("Invoking a MOVE!");
			AbstractActor abstractActor = combatGameState.FindActorByGUID(this.ActorGUID);
			if (abstractActor == null)
			{
				InvocationMessage.logger.LogError(string.Format("MechMovement.Invoke Actor with GUID {0} not found!", this.ActorGUID));
				return false;
			}
			ICombatant combatant = null;
			if (!string.IsNullOrEmpty(this.MeleeTargetGUID))
			{
				combatant = combatGameState.FindCombatantByGUID(this.MeleeTargetGUID, false);
				if (combatant == null)
				{
					InvocationMessage.logger.LogError(string.Format("MechMovement.Invoke ICombatant with GUID {0} not found!", this.MeleeTargetGUID));
					return false;
				}
			}
			if (!combatGameState.TurnDirector.IsInterleaved && this.MoveType != MoveType.Sprinting)
			{
				abstractActor.AutoBrace = true;
			}
			ActorMovementSequence stackSequence = new ActorMovementSequence(abstractActor, this.Waypoints, this.FinalOrientation, this.MoveType, combatant, this.AbilityConsumesFiring);
			base.PublishStackSequence(combatGameState.MessageCenter, stackSequence, this);
			return true;
		}

		// Token: 0x0600866E RID: 34414 RVA: 0x0000F9E0 File Offset: 0x0000DBE0
		public override void FromJSON(string json)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0600866F RID: 34415 RVA: 0x0000F9E0 File Offset: 0x0000DBE0
		public override string ToJSON()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06008670 RID: 34416 RVA: 0x0000F9E0 File Offset: 0x0000DBE0
		public override string GenerateJSONTemplate()
		{
			throw new NotImplementedException();
		}
	}
}
