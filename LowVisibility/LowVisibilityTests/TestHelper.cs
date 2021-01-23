using BattleTech;
using Harmony;
using LowVisibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            // Init the combat ref for constants

            ConstructorInfo constantsCI = AccessTools.Constructor(typeof(CombatGameConstants), new Type[] { });
            CombatGameConstants constants = (CombatGameConstants)constantsCI.Invoke(new object[] { });
            
            VisibilityConstantsDef visibilityDef = constants.Visibility;
            visibilityDef.UseAsymmetricalSensors = true;
            Traverse visibilityT = Traverse.Create(constants).Property("Visibility");
            visibilityT.SetValue(visibilityDef);

            CombatGameState cgs = new CombatGameState();
            Traverse constantsT = Traverse.Create(cgs).Property("Constants");
            constantsT.SetValue(constants);

            Traverse combatT = Traverse.Create(actor).Property("Combat");
            combatT.SetValue(cgs);

            // Init any required stats
            actor.StatCollection = new StatCollection();
            
            // ModStats
            actor.StatCollection.AddStatistic<int>(ModStats.TacticsMod, 0);
            actor.StatCollection.AddStatistic<int>(ModStats.CurrentRoundEWCheck, 0);
            actor.StatCollection.AddStatistic<int>(ModStats.ECMShield, 0);
            actor.StatCollection.AddStatistic<int>(ModStats.ECMJamming, 0);
            actor.StatCollection.AddStatistic<int>(ModStats.AdvancedSensors, 0);
            actor.StatCollection.AddStatistic<int>(ModStats.ProbeCarrier, 0);
            actor.StatCollection.AddStatistic<int>(ModStats.PingedByProbe, 0);
            actor.StatCollection.AddStatistic<string>(ModStats.StealthEffect, "");
            actor.StatCollection.AddStatistic<string>(ModStats.MimeticEffect, "");
            actor.StatCollection.AddStatistic<int>(ModStats.MimeticCurrentSteps, 0);
            actor.StatCollection.AddStatistic<string>(ModStats.HeatVision, "");
            actor.StatCollection.AddStatistic<string>(ModStats.ZoomVision, "");
            actor.StatCollection.AddStatistic<string>(ModStats.NarcEffect, "");
            actor.StatCollection.AddStatistic<string>(ModStats.TagEffect, "");
            actor.StatCollection.AddStatistic<bool>(ModStats.SharesVision, false);
            actor.StatCollection.AddStatistic<bool>(ModStats.NightVision, false);
            actor.StatCollection.AddStatistic<int>(ModStats.DisableSensors, 2);
            
            // Vanilla
            actor.StatCollection.AddStatistic<float>("SensorSignatureModifier", 1.0f);



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
