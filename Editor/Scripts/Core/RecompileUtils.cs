using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Trackman.Editor.Core
{
    public class RecompileUtils : MonoBehaviour
    {
        #region Methods
        [MenuItem("Trackman/Recompile scripts %r")]
        public static void RecompileScripts() => CompilationPipeline.RequestScriptCompilation();
        [MenuItem("Trackman/Recompile all scripts %&r")]
        public static void RecompileAllScripts() => CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
        #endregion
    }
}
