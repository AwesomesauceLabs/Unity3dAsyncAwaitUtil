using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityAsyncAwaitUtil
{
    public static class SyncContextUtil
    {
#if UNITY_EDITOR
        private static System.Reflection.MethodInfo executionMethod;

        /// <summary>
        /// HACK: makes Unity Editor execute continuations in edit mode.
        /// </summary>
        private static void ExecuteContinuations()
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            var context = SynchronizationContext.Current;

            if (executionMethod == null)
            {
                executionMethod = context.GetType().GetMethod("Exec", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            }

            executionMethod?.Invoke(context, null);
        }

        [UnityEditor.InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += ExecuteContinuations;
#endif
            UnitySynchronizationContext = SynchronizationContext.Current;
            UnityThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public static int UnityThreadId
        {
            get; private set;
        }

        public static SynchronizationContext UnitySynchronizationContext
        {
            get; private set;
        }
    }
}

