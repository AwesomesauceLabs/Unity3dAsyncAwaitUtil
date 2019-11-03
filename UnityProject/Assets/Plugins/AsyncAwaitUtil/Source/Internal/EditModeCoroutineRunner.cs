

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
        private List<IEnumerator> _coroutinesNext;

        private EditModeCoroutineRunner()
        {
            _coroutines = new List<IEnumerator>();
            _coroutinesNext = new List<IEnumerator>();

            EditorApplication.update += Update;
        }

        public void StartCoroutine(IEnumerator coroutine)
        {
            // Make first call to `coroutine.MoveNext`, so that
            // we can safely use `coroutine.Current` in `Update`.
            if (coroutine.MoveNext())
            {
                Debug.Log("_coroutines.Add(coroutine)");
                _coroutines.Add(coroutine);
            }
        }

        private void Update()
        {
            // Accumulates list of coroutines that 
            // haven't completed in this `Update`.
            // (Using a second temporary list avoids the
            // trickiness of removing items from `_coroutines`
            // while iterating through `_coroutines`.)
            _coroutinesNext.Clear();

            foreach (var coroutine in _coroutines)
            {
                // object returned by current `yield return` statement 
                var yieldInstruction = coroutine.Current;

                if (yieldInstruction == null)
                    Debug.Log("yieldInstruction == null");
                else
                    Debug.LogFormat("yieldInstruction.GetType().ToString(): {0}",
                        yieldInstruction.GetType().ToString());

                // Notes:
                //
                // (1) `yield return null` and
                // `yield return new WaitForUpdate()` simply suspend
                // coroutine execution until the next `Update`.
                //
                // (2) `WaitForEndOfFrame`/`WaitForFixedUpdate`
                // lose their meaning in Edit Mode, but 
                // treating them like `null`/`WaitForUpdate`
                // improves the reusability of coroutines across
                // Play Mode and Edit Mode.

                if (yieldInstruction == null
                    || yieldInstruction is WaitForUpdate
                    || yieldInstruction is WaitForEndOfFrame
                    || yieldInstruction is WaitForFixedUpdate)
                {
                    if (coroutine.MoveNext())
                        _coroutinesNext.Add(coroutine);
                    continue;
                }

                // Examples of `CustomYieldInstruction`:
                // `WaitUntil`, `WaitWhile`, `WaitForSecondsRealtime`.

                CustomYieldInstruction customYieldInstruction
                    = yieldInstruction as CustomYieldInstruction;
                Debug.LogFormat("Time.realtimeSinceStartup: {0}", Time.realtimeSinceStartup);
                Debug.LogFormat("customYieldInstruction != null: {0}", customYieldInstruction != null);
                if (customYieldInstruction != null)
                {
                    if (customYieldInstruction.MoveNext())
                        _coroutinesNext.Add(coroutine);
                    continue;
                }

                // Examples of `AsyncOperation`:
                // `ResourceRequest`, `AssetBundleRequest`
                // and `AssetBundleCreateRequest`.

                AsyncOperation asyncOperation
                    = yieldInstruction as AsyncOperation;
                if (asyncOperation != null)
                {
                    if (!asyncOperation.isDone)
                        _coroutinesNext.Add(coroutine);
                    continue;
                }

                // Unfortunately, `WaitForSeconds` doesn't expose any public
                // fields/properties/methods that allow us to easily emulate it
                // in Edit Mode.
                //
                // It might be possible to emulate `WaitForSeconds` in a
                // reasonable way using reflection, but for now we will
                // recommend using `WaitForSecondsRealtime` instead.

                if (yieldInstruction is WaitForSeconds)
                {
                    throw new NotSupportedException(
                        "AsyncAwaitUtil: "
                        + "WaitForSeconds() is not supported in Edit Mode. "
                        + "Consider using WaitforSecondsRealtime() instead.");
                }

            }

            // copy over coroutines that are not yet complete
            List<IEnumerator> tmp;
            _coroutines = _coroutinesNext;
        }
    }
}

#endif
