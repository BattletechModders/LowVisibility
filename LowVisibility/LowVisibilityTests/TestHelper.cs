using LowVisibility;
using System;
using System.Reflection;

namespace LowVisibilityTests
{
    public static class TestHelper
    {
        public static Mech BuildTestMech()
        {
            Mech mech = new Mech();
            return (Mech)InitAbstractActor(mech);
        }

        public static Turret BuildTestTurret()
        {
            Turret turret = new Turret();
            return (Turret)InitAbstractActor(turret);
        }

        public static Vehicle BuildTestVehicle()
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
            visibilityDef.ShutDownSignatureModifier = 0.5f;
            visibilityDef.ShutDownVisibilityModifier = 0.5f;

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
            actor.StatCollection.AddStatistic<string>(ModStats.ZoomAttack, "");
            actor.StatCollection.AddStatistic<int>(ModStats.ZoomVision, 0);
            actor.StatCollection.AddStatistic<string>(ModStats.NarcEffect, "");
            actor.StatCollection.AddStatistic<string>(ModStats.TagEffect, "");
            actor.StatCollection.AddStatistic<bool>(ModStats.SharesVision, false);
            actor.StatCollection.AddStatistic<bool>(ModStats.NightVision, false);
            actor.StatCollection.AddStatistic<int>(ModStats.DisableSensors, 2);

            // Vanilla
            actor.StatCollection.AddStatistic<float>("SensorSignatureModifier", 1.0f);



            return actor;
        }

        private static Pilot BuildTestPilot()
        {
            PilotDef pilotDef = new PilotDef();
            Guid guid = new Guid();

            Pilot pilot = new Pilot(pilotDef, guid.ToString(), false);

            return pilot;
        }

        public static Weapon BuildTestWeapon(float minRange = 0f, float shortRange = 0f,
            float mediumRange = 0f, float longRange = 0f, float maxRange = 0f)
        {
            Weapon weapon = new Weapon();

            StatCollection statCollection = new StatCollection();
            statCollection.AddStatistic("MinRange", minRange);
            statCollection.AddStatistic("MinRangeMultiplier", 1f);
            statCollection.AddStatistic("LongRangeModifier", 0f);
            statCollection.AddStatistic("MaxRange", maxRange);
            statCollection.AddStatistic("MaxRangeModifier", 0f);
            statCollection.AddStatistic("ShortRange", shortRange);
            statCollection.AddStatistic("MediumRange", mediumRange);
            statCollection.AddStatistic("LongRange", longRange);

            Traverse statCollectionT = Traverse.Create(weapon).Field("statCollection");
            statCollectionT.SetValue(statCollection);

            return weapon;
        }
    }
}
