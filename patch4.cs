
			// Token: 0x06000043 RID: 67
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
					Mod.Log.Debug(string.Format(" -- !consumesActivation: {0}, skipping", __instance.ConsumesActivation));
					return;
				}
				if (mech.IsShutDown)
				{
					Mod.Log.Debug(" -- Mech is shutdown, assuming a MechStartupSequence will handle this - skipping.");
					return;
				}
				if (true)
				{
					Mod.Log.Debug(" -- Combat is interleaved, should be handled by OnUpdate() or MechStartupSequence - skipping.");
					return;
				}
				DoneWithActorSequence dwaSeq = __instance as DoneWithActorSequence;
				if (dwaSeq != null)
				{
					Mod.Log.Debug(string.Format(" -- sequence is DoneWithActorSequence: {0}, skipping.", dwaSeq != null));
					return;
				}
				bool sequenceIsComplete = Traverse.Create(__instance).Property("sequenceIsComplete", null).GetValue<bool>();
				if (!sequenceIsComplete)
				{
					Mod.Log.Debug(string.Format(" -- !sequenceIsComplete: {0}, skipping", sequenceIsComplete));
					return;
				}
				MechHeatSequence heatSequence = mech.GenerateEndOfTurnHeat(__instance);
				if (heatSequence != null)
				{
					Mod.Log.Debug(" -- Creating heat sequence for non-interleaved mode");
					__instance.AddChildSequence(heatSequence, __instance.MessageIndex);
					return;
				}
				Mod.Log.Trace(string.Format("FAILED TO CREATE HEAT SEQUENCE FOR MECH: {0} - UNIT WILL CONTINUE TO GAIN HEAT!", mech));
			}
		}
