using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowVisibility.Object
{
    public abstract class ActionSemaphore
    {
        protected Func<bool> shouldTakeaction;

        protected Action actionToTake;

        protected int counter;

        protected ActionSemaphore(Func<bool> shouldTakeaction, Action actionToTake)
        {
            this.shouldTakeaction = shouldTakeaction;
            this.actionToTake = actionToTake;
        }

        public int Counter => counter;

        public virtual void ResetSemaphore() {
            counter = 0;
        }

        public virtual void Enter()
        {
            ++counter;
            if (shouldTakeaction()) {
                actionToTake();
            }
        }

        public virtual void Exit()
        {
            --counter;
            counter = counter < 0 ? 0 : counter;
            if (shouldTakeaction()) {
                actionToTake();
            }
        }

        public virtual void ResetHard() {
            counter = 0;
            if (shouldTakeaction()) {
                actionToTake();
            }
        }

        public virtual void ResetSoft() {
            while (counter-- > 0) {
                if (shouldTakeaction()) {
                    actionToTake();
                }
            }

            if (counter < 0)
                ResetHard();
        }
    }
}
