using System.Reflection;
using Gilzoide.UpdateManager.Jobs;
using UnityEditor;
using UnityEngine;

namespace Gilzoide.UpdateManager.Editor
{
    [CustomEditor(typeof(AJobBehaviour<>), true)]
    [CanEditMultipleObjects]
    public class AJobBehaviourEditor : UnityEditor.Editor
    {
        private GUIStyle TopAnchoredLabel => _topAnchoredLabel != null
            ? _topAnchoredLabel
            : (_topAnchoredLabel = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.UpperLeft
            });
        private GUIStyle _topAnchoredLabel;

        public override bool HasPreviewGUI()
        {
            return Application.isPlaying
                && !serializedObject.isEditingMultipleObjects
                && ((MonoBehaviour) target).isActiveAndEnabled;
        }

        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent($"{target.GetType().Name} Job Data");
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            PropertyInfo prop = target.GetType().GetProperty("JobData");
            GUI.Label(r, JsonUtility.ToJson(prop.GetValue(target), true), TopAnchoredLabel);
        }
    }
}
