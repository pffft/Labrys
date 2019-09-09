using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Labrys.Editor.FeatureEditor
{
    public abstract class Tool
    {
        public string Name { get; protected set; }

        protected EditorWindow window;

        protected Tool(EditorWindow window)
        {
            this.window = window;
        }

        public virtual void Draw() { }

        // Used to tell if the left mouse button is down
        protected bool IsPrimaryControl(Event e)
        {
            return e.isMouse && e.button == 0;
        }

        // Used to tell if the right mouse button is down
        protected bool IsSecondaryControl(Event e)
        {
            return e.isMouse && e.button == 1;
        }

        public abstract bool HandleEvent(Event e);

        protected void ChangeAsset(Object asset, string actionDesc, UnityAction action)
        {
            Undo.RegisterCompleteObjectUndo(asset, actionDesc);
            action?.Invoke();
            EditorUtility.SetDirty(asset);
        }
    }
}
