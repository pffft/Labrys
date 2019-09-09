using Labrys.Editor.FeatureEditor.Panels;
using Labrys.Editor.FeatureEditor.Tools;
using Labrys.FeatureEditor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
    public class FeatureEditorWindow : EditorWindow
    {
        private const string EDITOR_PREF_SETTINGS = "FeatureEditorWindowSettings";

        private static FeatureEditorWindow instance;

        public static FeatureEditorWindow GetInstance()
        {
            if (instance == null)
            {
                instance = OpenWindow();
            }
            return instance;
        }

        [MenuItem("Window/Labrys Feature Editor")]
        private static FeatureEditorWindow OpenWindow()
        {
            FeatureEditorWindow window = GetWindow<FeatureEditorWindow>();
            window.titleContent = new GUIContent("Labrys Feature Editor");
            return window;
        }

        [OnOpenAsset(1)]
        private static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject != null && Selection.activeObject is FeatureAsset)
            {
                GetInstance().feature = (FeatureAsset)Selection.activeObject;

                return true;
            }
            return false;
        }

        public FeatureAsset Feature { get { return feature; } }
        [SerializeField]
        private FeatureAsset feature;
        private ToolBox toolBox;

        public FeatureEditorWindow()
        {
            toolBox = new ToolBox(this, InternalPanel.DockPosition.left, 125f);
            toolBox.AddTool(new SelectionTool(this));
            toolBox.AddTool(new TilePaintTool(this));
            toolBox.AddTool(new ConnectionEditorTool(this));
        }

        private void OnUndoRedo()
        {
            Repaint();
        }

        private void OnEnable()
        {
            EditorGrid.GetInstance().viewport = position;
            EditorGrid.GetInstance().Recenter();

            string data = EditorPrefs.GetString(EDITOR_PREF_SETTINGS, EditorJsonUtility.ToJson(this));
            EditorJsonUtility.FromJsonOverwrite(data, this);

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;

            string data = EditorJsonUtility.ToJson(this);
            EditorPrefs.SetString(EDITOR_PREF_SETTINGS, data);
        }

        private void OnGUI()
        {
            EditorGrid.GetInstance().viewport = position;
            EditorGrid.GetInstance().Draw();
            toolBox.ActiveTool.Draw();

            toolBox.Draw();

            HandleEvent(Event.current);

            if (GUI.changed)
            {
                Repaint();
            }
        }

        private void HandleEvent(Event e)
        {
            if (toolBox.HandleEvent(e))
            {
                GUI.changed = true;
                return;
            }

            if (toolBox.ActiveTool.HandleEvent(e))
            {
                GUI.changed = true;
                return;
            }

            if (EditorGrid.GetInstance().HandleEvent(e))
            {
                GUI.changed = true;
                return;
            }
        }
    }
}
