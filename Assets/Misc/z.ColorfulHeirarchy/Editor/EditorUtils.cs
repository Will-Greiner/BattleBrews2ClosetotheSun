using UnityEditor;

// This script is editor-only and must remain inside an Editor folder.
namespace PrettyHierarchy
{
    public static class EditorUtils
    {
        public static bool IsHierarchyFocused { get { return EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.titleContent.text == "Hierarchy"; } }
    }
}
