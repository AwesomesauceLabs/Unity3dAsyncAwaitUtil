

using System;
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityAsyncAwaitUtil
{
    /// <summary>
    /// This class runs coroutines in Edit Mode.
    ///
    /// In Edit Mode, Unity does not automatically automatically advance
    /// coroutines in each frame (by calling `MoveNext`), like it does in Play
    /// Mode.  Instead, we must call `MoveNext` ourselves in response to the
    /// `EditorApplication.update` event.
    /// </summary>
    public class EditModeCoroutineRunner : ICoroutineRunner
    {
        private static EditModeCoroutineRunner _instance;

        public static EditModeCoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EditModeCoroutineRunner();
                return _instance;
            }
        }

        private List<IEnumerator> _coroutines;

        private EditModeCoroutineRunner()
        {
            _coroutines = new List<IEnumerator>();
            EditorApplication.update += Update;
        }

        public void StartCoroutine(IEnumerator coroutine)
        {
            // Make first call to `coroutine.MoveNext`, so that
            // we can safely use `coroutine.Current` in `Update`.
            if (coroutine.MoveNext())
            {
                _coroutines.Add(coroutine);
            }
        }

        private void Update()
        {
            // Loop in reverse order so that we can remove
            // completed coroutines while iterating.

            for (int i = _coroutines.Count - 1; i >= 0; i--)
            {
                // object returned by `yield return`

                var yieldInstruction = _coroutines[i].Current;

                // `WaitForSeconds` and `WaitForSecondsRealtime`
                // have unpredictable behaviour in Edit Mode and are
                // therefore forbidden.

                if (IEnumeratorAwaitExtensions.IllegalInstruction(
                    yieldInstruction))
                {
                    Debug.LogErrorFormat(string.Format(
                        "{0} is not supported in Edit Mode",
                        yieldInstruction.GetType().ToString()));

                    _coroutines.RemoveAt(i);

                    continue;
                }

                // Examples of `AsyncOperation`:
                // `ResourceRequest`, `AssetBundleRequest`
                // and `AssetBundleCreateRequest`.

                AsyncOperation asyncOperation
                    = yieldInstruction as AsyncOperation;
                if (asyncOperation != null)
                {
                    if (asyncOperation.isDone)
                        _coroutines.RemoveAt(i);
                    continue;
                }

                // Default case: `yield return` is `null` or is a type with no
                // special Unity-defined behaviour.

                if (!_coroutines[i].MoveNext())
                    _coroutines.RemoveAt(i);
            }
        }
    }
}

#endif
