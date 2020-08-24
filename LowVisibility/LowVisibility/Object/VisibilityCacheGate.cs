using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;

namespace LowVisibility.Object
{
    /// <summary>
    /// Pool all requests to rebuild the visibility cache into one place.
    /// </summary>
    public class VisibilityCacheGate : ActionSemaphore
    {
        private static VisibilityCacheGate cacheGate = new VisibilityCacheGate();

        private readonly HashSet<AbstractActor> actors = new HashSet<AbstractActor>();

        private VisibilityCacheGate()
            : base(null, null)
        {
            shouldTakeaction = () => counter == 0;
            actionToTake = () =>
            {
                List<ICombatant> combatants = UnityGameInstance.BattleTechGame.Combat.GetAllLivingCombatants();
                List<ICombatant> uniDirectionalList = new List<ICombatant>();
                List<ICombatant> biDirectionalList = new List<ICombatant>();

                foreach (ICombatant combatant in combatants)
                {
                    if (actors.Contains(combatant))
                    {
                        uniDirectionalList.Add(combatant);
                    }
                    else
                    {
                        biDirectionalList.Add(combatant);
                    }
                }

                foreach (AbstractActor actor in actors)
                {
                    actor.VisibilityCache?.UpdateCacheReciprocal(biDirectionalList);
                    actor.VisibilityCache?.RebuildCache(uniDirectionalList);
                }

                actors.Clear();
            };
        }

        public static bool Active => cacheGate.counter > 0;

        public static int GetCounter => cacheGate.counter;

        public static void EnterGate()
        {
            cacheGate.Enter();
        }

        public static void ExitGate()
        {
            cacheGate.Exit();
        }

        public static void ExitAll() {
            cacheGate.ResetHard();
        }

        public static void Reset() {
            cacheGate.ResetSemaphore();
        }

        public static void AddActorToRefresh(AbstractActor actor) {
            cacheGate.actors.Add(actor);
        }

        #region Overrides of ActionSemaphore

        public override void ResetSemaphore() {
            base.ResetSemaphore();
            actors.Clear();
        }

        #endregion
    }
}
