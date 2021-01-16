using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using Harmony;
using UnityEngine;

namespace CBTBehaviors
{
	// Token: 0x02000007 RID: 7
	public static class MovementPatches
	{
		// Token: 0x02000010 RID: 16
		[HarmonyPatch(typeof(ToHit), "GetAllModifiers")]
		public static class ToHit_GetAllModifiers
		{
			// Token: 0x06000025 RID: 37
			private static void Postfix(ToHit __instance, ref float __result, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot)
			{
				if (UnityGameInstance.BattleTechGame.Simulation != null && weapon != null)
				{
					Mod.Log.Trace("TH:GAM entered");
					bool flag3;
					if (!attacker.HasMovedThisRound || !attacker.JumpedLastRound || attacker.SkillTactics >= Mod.Config.TacticsSkillNegateJump)
					{
						CombatHUD combatHUD = ModState.CombatHUD;
						bool flag2;
						if (combatHUD == null)
						{
							flag2 = false;
						}
						else
						{
							CombatSelectionHandler selectionHandler = combatHUD.SelectionHandler;
							flag2 = (((selectionHandler != null) ? selectionHandler.ActiveState : null) != null);
						}
						if (flag2)
						{
							CombatHUD combatHUD2 = ModState.CombatHUD;
							object obj;
							if (combatHUD2 == null)
							{
								obj = null;
							}
							else
							{
								CombatSelectionHandler selectionHandler2 = combatHUD2.SelectionHandler;
								obj = ((selectionHandler2 != null) ? selectionHandler2.ActiveState : null);
							}
							if (obj is SelectionStateJump)
							{
								flag3 = (attacker.SkillTactics < Mod.Config.TacticsSkillNegateJump);
								goto IL_D8;
							}
						}
						flag3 = false;
					}
					else
					{
						flag3 = true;
					}
				IL_D8:
					if (flag3)
					{
						__result += (float)Mod.Config.ToHitSelfJumped;
					}
				}
			}
		}

		// Token: 0x02000011 RID: 17
		[HarmonyPatch(typeof(ToHit), "GetAllModifiersDescription")]
		public static class ToHit_GetAllModifiersDescription
		{
			// Token: 0x06000026 RID: 38
			private static void Postfix(ToHit __instance, ref string __result, AbstractActor attacker, Weapon weapon, ICombatant target, Vector3 attackPosition, Vector3 targetPosition, LineOfFireLevel lofLevel, bool isCalledShot)
			{
				if (UnityGameInstance.BattleTechGame.Simulation != null)
				{
					Mod.Log.Trace("TH:GAMD entered");
					if (attacker.HasMovedThisRound && attacker.JumpedLastRound && attacker.SkillTactics < Mod.Config.TacticsSkillNegateJump)
					{
						__result = string.Format("{0}JUMPED {1:+#;-#}; ", __result, Mod.Config.ToHitSelfJumped);
					}
				}
			}
		}

		// Token: 0x02000012 RID: 18
		[HarmonyPatch(typeof(CombatHUDWeaponSlot), "SetHitChance", new Type[]
		{
			typeof(ICombatant)
		})]
		public static class CombatHUDWeaponSlot_SetHitChance
		{
			// Token: 0x06000027 RID: 39
			private static void Postfix(CombatHUDWeaponSlot __instance, ICombatant target)
			{
				if (UnityGameInstance.BattleTechGame.Simulation != null)
				{
					Mod.Log.Trace("CHUDWS:SHC entered");
					AbstractActor parent = __instance.DisplayedWeapon.parent;
					Traverse.Create(__instance);
					if (parent.HasMovedThisRound && parent.JumpedLastRound && parent.SkillTactics < Mod.Config.TacticsSkillNegateJump)
					{
						Traverse traverse = Traverse.Create(__instance).Method("AddToolTipDetail", new object[]
						{
							"JUMPED SELF",
							Mod.Config.ToHitSelfJumped
						});
						Mod.Log.Debug(string.Format("Invoking addToolTipDetail for: JUMPED SELF = {0}", Mod.Config.ToHitSelfJumped));
						traverse.GetValue();
					}
				}
			}
		}

		// Token: 0x02000013 RID: 19
		[HarmonyPatch(typeof(AbstractActor), "ResolveAttackSequence", null)]
		public static class AbstractActor_ResolveAttackSequence_Patch
		{
			// Token: 0x06000028 RID: 40
			public static bool Prefix(AbstractActor __instance)
			{
				bool result;
				if (UnityGameInstance.BattleTechGame.Simulation == null)
				{
					result = true;
				}
				else
				{
					Mod.Log.Trace("AA:RAS:PRE entered");
					result = false;
				}
				return result;
			}

			// Token: 0x06000029 RID: 41
			public static void Postfix(AbstractActor __instance, string sourceID, int sequenceID, int stackItemID, AttackDirection attackDirection)
			{
				if (UnityGameInstance.BattleTechGame.Simulation != null)
				{
					Mod.Log.Trace("AA:RAS:POST entered");
					if (!Mod.Config.UsingSemiPermanentEvasion)
					{
						int evasivePipsCurrent = __instance.EvasivePipsCurrent;
						__instance.ConsumeEvasivePip(true);
						if (__instance.EvasivePipsCurrent < evasivePipsCurrent && !__instance.IsDead && !__instance.IsFlaggedForDeath)
						{
							__instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, "-1 EVASION", FloatieMessage.MessageNature.Debuff));
						}
						AttackDirector.AttackSequence attackSequence = __instance.Combat.AttackDirector.GetAttackSequence(sequenceID);
						if (attackSequence != null && attackSequence.GetAttackDidDamage(__instance.GUID))
						{
							List<Effect> list = __instance.Combat.EffectManager.GetAllEffectsTargeting(__instance).FindAll((Effect x) => x.EffectData.targetingData.effectTriggerType == EffectTriggerType.OnDamaged);
							for (int i = 0; i < list.Count; i++)
							{
								list[i].OnEffectTakeDamage(attackSequence.attacker, __instance);
							}
							if (attackSequence.isMelee)
							{
								int value = attackSequence.attacker.StatCollection.GetValue<int>("MeleeHitPushBackPhases");
								if (value > 0)
								{
									for (int j = 0; j < value; j++)
									{
										__instance.ForceUnitOnePhaseDown(sourceID, stackItemID, false);
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0200001F RID: 31
		[HarmonyPatch(typeof(AbstractActor), "InitEffectStats")]
		public static class AbstractActor_InitEffectStats
		{
			// Token: 0x06000039 RID: 57
			private static void Postfix(AbstractActor __instance)
			{
				Mod.Log.Trace("AA:IES entered");
				__instance.StatCollection.Set<bool>("CanShootAfterSprinting", true);
			}
		}

		// Token: 0x02000079 RID: 121
		[HarmonyPatch(typeof(OrderSequence), "OnUpdate")]
		public static class OrderSequence_OnUpdate
		{
			// Token: 0x060000F1 RID: 241 RVA: 0x0001427C File Offset: 0x0001247C
			public static void Prefix(OrderSequence __instance, ref bool __state)
			{
				__state = false;
				Mech mech = __instance.owningActor as Mech;
				if (__instance == null || __instance.owningActor == null || mech == null)
				{
					return;
				}
				Traverse traverse = Traverse.Create(__instance).Property("sequenceIsComplete", null);
				__state = traverse.GetValue<bool>();
			}

			// Token: 0x060000F2 RID: 242 RVA: 0x000142C4 File Offset: 0x000124C4
			public static void Postfix(OrderSequence __instance, bool __state)
			{
				Mech mech = __instance.owningActor as Mech;
				if (__instance == null || __instance.owningActor == null || mech == null)
				{
					return;
				}

				if (__state)
				{
					return;
				}

				if (!__instance.ConsumesActivation)
				{
					Mod.Log.Debug($" -- !consumesActivation: {__instance.ConsumesActivation}, skipping");
					return;
				}

				if (mech.IsShutDown)
				{
					Mod.Log.Debug(" -- Mech is shutdown, assuming a MechStartupSequence will handle this - skipping.");
					return;
				}

				bool isInterleaved = true;
				if (isInterleaved)
				{
					Mod.Log.Debug(" -- Combat is interleaved, should be handled by OnUpdate() or MechStartupSequence - skipping.");
					return;
				}

				DoneWithActorSequence dwaSeq = __instance as DoneWithActorSequence;
				if (dwaSeq != null)
				{
					Mod.Log.Debug($" -- sequence is DoneWithActorSequence: {dwaSeq != null}, skipping.");
					return;
				}

				Traverse sequenceIsCompleteT = Traverse.Create(__instance).Property("sequenceIsComplete");
				bool sequenceIsComplete = sequenceIsCompleteT.GetValue<bool>();
				if (!sequenceIsComplete)
				{
					Mod.Log.Debug($" -- !sequenceIsComplete: {sequenceIsComplete}, skipping");
					return;
				}

				MechHeatSequence heatSequence = mech.GenerateEndOfTurnHeat(__instance);
				if (heatSequence != null)
				{
					Mod.Log.Debug($" -- Creating heat sequence for non-interleaved mode");
					__instance.AddChildSequence(heatSequence, __instance.MessageIndex);
				}
				else
				{
					Mod.Log.Trace($"FAILED TO CREATE HEAT SEQUENCE FOR MECH: {mech} - UNIT WILL CONTINUE TO GAIN HEAT!");
				}
			}
		}

		// Token: 0x0200007A RID: 122
		[HarmonyPatch(typeof(AbstractActor), "DoneWithActor")]
		private static class AbstractActor_DoneWithActor
		{
			// Token: 0x060000F3 RID: 243 RVA: 0x00014616 File Offset: 0x00012816
			private static void Postfix(AbstractActor __instance)
			{
			}
		}

		// Token: 0x0200007B RID: 123
		[HarmonyPatch(typeof(ActorMovementSequence), "OnComplete")]
		public static class ActorMovementSequence_OnComplete
		{
			// Token: 0x060000F4 RID: 244 RVA: 0x00014618 File Offset: 0x00012818
			private static void Prefix(ActorMovementSequence __instance)
			{
				if (!__instance.owningActor.Combat.TurnDirector.IsInterleaved)
				{
					__instance.owningActor.AutoBrace = true;
				}
			}

			// Token: 0x0200007C RID: 124
			[HarmonyPatch(typeof(ActorMovementSequence), "ConsumesFiring", MethodType.Getter)]
			public static class ActorMovementSequence_ConsumesFiring_Getter
			{
				// Token: 0x060000F6 RID: 246 RVA: 0x00014860 File Offset: 0x00012A60
				private static void Postfix(ActorMovementSequence __instance, ref bool __result)
				{
					if (!__instance.OwningActor.Combat.TurnDirector.IsInterleaved)
					{
						__result = false;
					}
				}
			}

			// Token: 0x0200007D RID: 125
			[HarmonyPatch(typeof(MechJumpSequence), "ConsumesFiring", MethodType.Getter)]
			public static class MechJumpSequence_ConsumesFiring_Getter
			{
				// Token: 0x060000F7 RID: 247 RVA: 0x000148B4 File Offset: 0x00012AB4
				private static void Postfix(MechJumpSequence __instance, ref bool __result)
				{
					if (!__instance.owningActor.Combat.TurnDirector.IsInterleaved)
					{
						__result = false;
					}
				}
			}

			// Token: 0x0200007E RID: 126
			[HarmonyPatch(typeof(MechJumpSequence), "OnComplete")]
			public static class MechJumpSequence_OnComplete
			{
				// Token: 0x060000F8 RID: 248 RVA: 0x00014908 File Offset: 0x00012B08
				private static void Prefix(MechJumpSequence __instance)
				{
					if (!__instance.owningActor.Combat.TurnDirector.IsInterleaved)
					{
						__instance.owningActor.AutoBrace = true;
					}
					if (__instance.OwningMech == null)
					{
						return;
					}
				}
			}

			// Token: 0x0200007F RID: 127
			[HarmonyPatch(typeof(AbstractActorMovementInvocation), "Invoke")]
			public static class AbstractActorMovementInvocation_Invoke
			{
				// Token: 0x060000F9 RID: 249 RVA: 0x00014B50 File Offset: 0x00012D50
				private static void Postfix(AbstractActorMovementInvocation __instance)
				{
					AbstractActor abstractActor = (__instance.ActorGUID);
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
	}
}
