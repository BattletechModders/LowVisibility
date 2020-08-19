using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace LowVisibility
{
    public static class HarmonyHelper
    {
        public delegate ref U RefGetter<in T, U>(T obj);

        /// <summary>
        /// Create a pointer for instance field <paramref name="s_field"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="s_field"></param>
        /// <returns> Pointer to <paramref name="s_field"/></returns>
        /// <remarks> Source: https://stackoverflow.com/a/45046664/13073994 </remarks>
        public static RefGetter<T, U> CreateInstanceFieldRef<T, U>(String s_field)
        {
            const BindingFlags bf = BindingFlags.NonPublic |
                                    BindingFlags.Instance |
                                    BindingFlags.DeclaredOnly;

            var fi = typeof(T).GetField(s_field, bf);
            if (fi == null)
                throw new MissingFieldException(typeof(T).Name, s_field);

            var s_name = "__refget_" + typeof(T).Name + "_fi_" + fi.Name;

            // workaround for using ref-return with DynamicMethod:
            //   a.) initialize with dummy return value
            var dm = new DynamicMethod(s_name, typeof(U), new[] { typeof(T) }, typeof(T), true);

            //   b.) replace with desired 'ByRef' return value
            dm.GetType().GetField("returnType", bf).SetValue(dm, typeof(U).MakeByRefType());

            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, fi);
            il.Emit(OpCodes.Ret);

            return (RefGetter<T, U>)dm.CreateDelegate(typeof(RefGetter<T, U>));
        }
    }
}
