using BattleTech;
using LowVisibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowVisibilityTests
{
    public static class TestHelper
    {
        public static Mech TestMech()
        {
            Mech mech = new Mech();
            return (Mech)InitAbstractActor(mech);
        }

        public static Turret TestTurret()
        {
            Turret turret = new Turret();
            return (Turret)InitAbstractActor(turret);
        }

        public static Vehicle TestVehicle()
        {
            Vehicle vehicle = new Vehicle();
            return (Vehicle)InitAbstractActor(vehicle);
        }

        private static AbstractActor InitAbstractActor(AbstractActor actor)
        {
            actor.StatCollection = new StatCollection();

            actor.StatCollection.AddStatistic<int>(ModStats.TacticsMod, 0);

            actor.StatCollection.AddStatistic<int>(ModStats.CurrentRoundEWCheck, 0);

            // ECM
            actor.StatCollection.AddStatistic<int>(ModStats.ECMShield, 0);
            actor.StatCollection.AddStatistic<int>(ModStats.ECMJamming, 0);

            // Sensors
            actor.StatCollection.AddStatistic<int>(ModStats.AdvancedSensors, 0);

            // Probe
            actor.StatCollection.AddStatistic<int>(ModStats.ProbeCarrier, 0);
            actor.StatCollection.AddStatistic<int>(ModStats.PingedByProbe, 0);

            // Sensor Stealth
            actor.StatCollection.AddStatistic<string>(ModStats.StealthEffect, "");

            // Visual Stealth
            actor.StatCollection.AddStatistic<string>(ModStats.MimeticEffect, "");
            actor.StatCollection.AddStatistic<int>(ModStats.MimeticCurrentSteps, 0);

            // Vision
            actor.StatCollection.AddStatistic<string>(ModStats.HeatVision, "");
            actor.StatCollection.AddStatistic<string>(ModStats.ZoomVision, "");

            // Narc 
            actor.StatCollection.AddStatistic<string>(ModStats.NarcEffect, "");

            // Tag
            actor.StatCollection.AddStatistic<string>(ModStats.TagEffect, "");

            // Vision sharing
            actor.StatCollection.AddStatistic<bool>(ModStats.SharesVision, false);

            // Night vision
            actor.StatCollection.AddStatistic<bool>(ModStats.NightVision, false);

            // Disabled sensors flag
            actor.StatCollection.AddStatistic<int>(ModStats.DisableSensors, 2);

            return actor;
        }

        private static Pilot TestPilot()
        {
            PilotDef pilotDef = new PilotDef();
            Guid guid = new Guid();

            Pilot pilot = new Pilot(pilotDef, guid.ToString(), false);

            return pilot;
        }
    }
}
