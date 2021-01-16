using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using Harmony;
using BattleTech.Serialization;
using UnityEngine;

namespace CBTBehaviors
{
	// Token: 0x0200007F RID: 127
	[HarmonyPatch(typeof(AbstractActorMovementInvocation), "Invoke")]
	public class AbstractActorMovementInvocation_Invoke
	{
		// Token: 0x17001720 RID: 5920
		// (get) Token: 0x0600865E RID: 34398 RVA: 0x0022E36F File Offset: 0x0022C56F
		// (set) Token: 0x0600865F RID: 34399 RVA: 0x0022E377 File Offset: 0x0022C577
		[SerializableMember(SerializationTarget.Networking)]
		public string ActorGUID { get; private set; }

		// Token: 0x0600866A RID: 34410 RVA: 0x0022E3D8 File Offset: 0x0022C5D8
		public AbstractActorMovementInvocation_Invoke(AbstractActor actor)
		{
			this.ActorGUID = actor.GUID;
		}

		// Token: 0x0600866B RID: 34411 RVA: 0x0022E478 File Offset: 0x0022C678
		public AbstractActorMovementInvocation_Invoke(string actorGUID)
		{
			this.ActorGUID = actorGUID;
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x00014B50 File Offset: 0x00012D50
		public override bool Postfix(CombatGameState combatGameState)
		{
			AbstractActor abstractActor = combatGameState.FindActorByGUID(this.ActorGUID);
			if (abstractActor != null)
			{
				if (!abstractActor.Combat.TurnDirector.IsInterleaved)
				{
					abstractActor.AutoBrace = true;
				}
			}
		}
	}
}
