
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
