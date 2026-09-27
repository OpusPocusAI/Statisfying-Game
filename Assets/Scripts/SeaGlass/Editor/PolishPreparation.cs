using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    public static class PolishPreparation
    {
        [MenuItem("Sea Glass/Polish/Connect gameplay presentation")]
        public static void ConnectPresentation()
        {
            var scene=EditorSceneManager.GetActiveScene();
            if(EditorApplication.isPlaying||scene.path!=CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode before connecting its presentation.");
            var session=Object.FindFirstObjectByType<GameSession>();
            if(!session)throw new System.InvalidOperationException("The scene has no GameSession.");
            if(!session.GetComponent<StudioPresentation>())Undo.AddComponent<StudioPresentation>(session.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
