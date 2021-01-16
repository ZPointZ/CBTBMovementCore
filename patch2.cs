using System;
using BattleTech;
using BattleTech.Serialization;
using Harmony;

namespace CBTBehaviors
{
	// Token: 0x02000029 RID: 41
	[HarmonyPatch(typeof(AbstractActorMovementInvocation), "Invoke")]
	public class AbstractActorMovementInvocation_Invoke
	{
		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000052 RID: 82
		// (set) Token: 0x06000053 RID: 83
		[SerializableMember(SerializationTarget.Networking)]
		public string ActorGUID { get; private set; }

		// Token: 0x06000054 RID: 84
		public AbstractActorMovementInvocation_Invoke(AbstractActor actor)
		{
			this.ActorGUID = actor.GUID;
		}

		// Token: 0x06000055 RID: 85
		public AbstractActorMovementInvocation_Invoke(string actorGUID)
		{
			this.ActorGUID = actorGUID;
		}

		// Token: 0x06000056 RID: 86
		private void Postfix(CombatGameState combatGameState)
		{
			AbstractActor abstractActor = combatGameState.FindActorByGUID(this.ActorGUID);
			if (abstractActor != null && !abstractActor.Combat.TurnDirector.IsInterleaved)
			{
				abstractActor.AutoBrace = true;
			}
		}
	}
}
