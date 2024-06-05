using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace BlockStackGridLibrary
{
    public class LevelBuildPreProcess:IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            //Clear level at the scene
            var levelEditor = UnityEngine.Object.FindObjectOfType<LevelCreator>();
            if (levelEditor != null)
            {
                levelEditor.ClearBoard();
            }
            
        }
    }
}