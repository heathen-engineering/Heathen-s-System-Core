using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace HeathenEngineering.Core.Editors
{
    public class SystemCoreEditorTools
    {
        private static List<IEnumerator> cooroutines;

        [InitializeOnLoadMethod]
        public static void InitOnLoadMethod()
        {
            if (!SessionState.GetBool("SystemCoreValidating", false))
            {
                StartCoroutine(Validate());
            }
        }

        [MenuItem("Help/Heathen/System Core/Update (Package Manager)", priority = 0)]
        public static void InstallSystemCoreMenuItem()
        {
            if (!SessionState.GetBool("SystemCoreInstalling", false))
            {
                StartCoroutine(InstallSystemCore());
            }
        }

        [MenuItem("Help/Heathen/System Core/Source Code (GitHub)", priority = 1)]
        public static void SourceCode()
        {
            Application.OpenURL("https://github.com/heathen-engineering/SystemCore");
        }

        [MenuItem("Help/Heathen/System Core/Documentation", priority = 2)]
        public static void Documentation()
        {
            Application.OpenURL("https://kb.heathenengineering.com/assets/system-core");
        }

        [MenuItem("Help/Heathen/System Core/Support", priority = 3)]
        public static void Support()
        {
            Application.OpenURL("https://discord.gg/RMGtDXV");
        }

        private static IEnumerator Validate()
        {
            SessionState.SetBool("SystemCoreValidating", true);
            var listRequest = Client.List();
            var hasMathmatics = false;

            if (listRequest.Status == StatusCode.Success)
            {
                if (listRequest.Result.Any(p => p.name == "com.unity.mathematics"))
                    hasMathmatics = true;
                else
                    hasMathmatics = false;
            }
            else
            {
                while (listRequest.Status == StatusCode.InProgress)
                {
                    yield return null;
                }
            }

            if (listRequest.Status == StatusCode.Success)
            {
                if (listRequest.Result.Any(p => p.name == "com.unity.mathematics"))
                    hasMathmatics = true;
                else
                    hasMathmatics = false;
            }

            if (!hasMathmatics)
            {
                var process = Client.Add("com.unity.mathematics");
                if (process.Status == StatusCode.Failure)
                    Debug.LogError("PackageManager install failed for Unity Mathematics");
                else if (process.Status == StatusCode.Success)
                    Debug.Log("Mathematics " + process.Result.version + " installation complete");
                else
                {
                    Debug.Log("Installing Mathematics ...");
                    while (process.Status == StatusCode.InProgress)
                    {
                        yield return null;
                    }
                }

                if (process.Status == StatusCode.Failure)
                    Debug.LogError("PackageManager install failed, Error Message: " + process.Error.message);
                else if (process.Status == StatusCode.Success)
                {
                    Debug.Log("Mathematics " + process.Result.version + " installation complete");
                    hasMathmatics = true;
                }
            }

            if (hasMathmatics)
            {
                string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                HashSet<string> defines = new HashSet<string>(currentDefines.Split(';'))
                {
                    "HE_SYSCORE"
                };

                string newDefines = string.Join(";", defines);
                if (newDefines != currentDefines)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newDefines);
                }
            }
            else
            {
                Debug.LogWarning("Failed to locate or install Unity's Mathmatics. This is a requirement to use System Core. The HE_SYSCORE define will not be added and related and dependent code will not be compiled.\nPlease install Unity's Mathematics from Package Manager and then update System Core via the menu.");
            }

            SessionState.SetBool("SystemCoreValidating", false);
        }

        private static IEnumerator InstallSystemCore()
        {
            SessionState.SetBool("SystemCoreInstalling", true);
            yield return null;

            var process = Client.Add("com.unity.mathematics");
            if (process.Status == StatusCode.Failure)
                Debug.LogError("PackageManager install failed for Unity Mathematics");
            else if (process.Status == StatusCode.Success)
                Debug.Log("Mathematics " + process.Result.version + " installation complete");
            else
            {
                Debug.Log("Installing Mathematics ...");
                while (process.Status == StatusCode.InProgress)
                {
                    yield return null;
                }
            }

            if (process.Status == StatusCode.Failure)
                Debug.LogError("PackageManager install failed, Error Message: " + process.Error.message);
            else if (process.Status == StatusCode.Success)
                Debug.Log("Mathematics " + process.Result.version + " installation complete");

            process = Client.Add("https://github.com/heathen-engineering/Heathen-System-Core.git?path=/com.heathen.systemcore");

            if (process.Status == StatusCode.Failure)
                Debug.LogError("PackageManager install failed, Error Message: " + process.Error.message + "\n\nA solution to your issue may be found in our Knowledge base [https://kb.heathenengineering.com/assets/system-core] or you can ask for more help on our Discord [https://discord.gg/RMGtDXV]");
            else if (process.Status == StatusCode.Success)
                Debug.Log("System Core " + process.Result.version + " installation complete");
            else
            {
                Debug.Log("Installing System Core ...");
                while (process.Status == StatusCode.InProgress)
                {
                    yield return null;
                }
            }

            if (process.Status == StatusCode.Failure)
                Debug.LogError("PackageManager install failed, Error Message: " + process.Error.message + "\n\nA solution to your issue may be found in our Knowledge base [https://kb.heathenengineering.com/assets/system-core] or you can ask for more help on our Discord [https://discord.gg/RMGtDXV]");
            else if (process.Status == StatusCode.Success)
                Debug.Log("System Core " + process.Result.version + " installation complete");

            SessionState.SetBool("SystemCoreInstalling", false);
        }

        private static void StartCoroutine(IEnumerator handle)
        {
            if (cooroutines == null)
            {
                EditorApplication.update -= EditorUpdate;
                EditorApplication.update += EditorUpdate;
                cooroutines = new List<IEnumerator>();
            }

            cooroutines.Add(handle);
        }

        private static void EditorUpdate()
        {
            List<IEnumerator> done = new List<IEnumerator>();

            if (cooroutines != null)
            {
                foreach (var e in cooroutines)
                {
                    if (!e.MoveNext())
                        done.Add(e);
                    else
                    {
                        if (e.Current != null)
                            Debug.Log(e.Current.ToString());
                    }
                }
            }

            foreach (var d in done)
                cooroutines.Remove(d);
        }
    }
}
