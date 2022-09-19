﻿using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VFECore
{
    public class CompProperties_AmbientSound : CompProperties
    {
        public SoundDef ambientSound;


        public CompProperties_AmbientSound()
        {
            this.compClass = typeof(CompAmbientSound);
        }
    }

    public class CompAmbientSound : ThingComp
    {
        private Sustainer sustainerAmbient;

        public CompProperties_AmbientSound Props => base.props as CompProperties_AmbientSound;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            CompPowerTrader compPower = parent.TryGetComp<CompPowerTrader>();

            if ((compPower == null || compPower.PowerOn) && FlickUtility.WantsToBeOn(parent))
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    StartSustainer();
                });
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            EndSustainer();
        }

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);

            switch (signal)
            {
                case CompPowerTrader.PowerTurnedOffSignal:
                case CompFlickable.FlickedOffSignal:
                    EndSustainer();
                    break;
                case CompPowerTrader.PowerTurnedOnSignal:
                case CompFlickable.FlickedOnSignal:
                    StartSustainer();
                    break;
                default:
                    break;
            }
        }

        private void StartSustainer()
        {
            if (sustainerAmbient != null)
                return;

            SoundInfo info = SoundInfo.InMap(this.parent);
            if (this.parent is Pawn pawn)
            {
                pawn.pather ??= new Pawn_PathFollower(pawn);
                pawn.stances ??= new Pawn_StanceTracker(pawn);
            }
            sustainerAmbient = Props.ambientSound.TrySpawnSustainer(info);

        }

        private void EndSustainer()
        {
            if (sustainerAmbient != null)
            {
                sustainerAmbient.End();
                sustainerAmbient = null;
            }
        }
    }
}

