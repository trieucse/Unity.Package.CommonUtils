using UnityEditor;
using UnityEditor.Compilation;

namespace Trackman.Editor.Core
{
    public static class RecompileUtils
    {
        #region Methods
        [MenuItem("Trackman/Recompile scripts %r")]
        public static void RecompileScripts() => CompilationPipeline.RequestScriptCompilation();
        [MenuItem("Trackman/Recompile all scripts %&r")]
        public static void RecompileAllScripts() => CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
        #endregion
    }
}
