using UnityEditor;
using UnityEngine;

namespace RhythmGameStarter
{
    public static class EditorMenuHelper
    {
        [MenuItem("Tools/Rhythm Game/Review...", false, 100)]
        private static void Review()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/templates/systems/rhythm-game-starter-160117");
        }

        [MenuItem("Tools/Rhythm Game/Help/Documentation...", false, 100)]
        private static void OpenDocumentation()
        {
            //Opening the online docs in the gitbook
            Application.OpenURL("https://bennykok.gitbook.io/rhythm-game-starter/");
        }

        [MenuItem("Tools/Rhythm Game/Help/FAQ...", false, 100)]
        private static void OpenFAQ()
        {
            //Opening the faq in the gitbook
            Application.OpenURL("https://bennykok.gitbook.io/rhythm-game-starter/faq");
        }


        [MenuItem("Tools/Rhythm Game/Help/Discord...", false, 100)]
        private static void JoinDiscord()
        {
            //Opening our discord forum
            Application.OpenURL("https://discord.gg/fHGsArj");
        }

        [MenuItem("Tools/Rhythm Game/Toggle Help Comment", false)]
        private static void ToggleHelpComment()
        {
            var target = !EditorPrefs.GetBool(CommentAttributeDrawer.ENABLE_KEY, true);
            EditorPrefs.SetBool(CommentAttributeDrawer.ENABLE_KEY, target);
            CommentAttributeDrawer.enableHelpComment = target;
        }
    }
}