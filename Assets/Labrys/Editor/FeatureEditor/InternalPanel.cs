using UnityEngine;
using UnityEditor;

namespace Labrys.Editor.FeatureEditor
{
    public abstract class InternalPanel
    {
        protected Rect bounds;
        private float scale;
        private DockPosition alignment;
        private EditorWindow window;

        public float Width { get { return bounds.width; } set { bounds.width = value; } }
        public float Height { get { return bounds.height; } set { bounds.height = value; } }

        protected InternalPanel(EditorWindow window, DockPosition alignment, float scale)
        {
            this.window = window;
            this.alignment = alignment;
            this.scale = scale;
            CalculateRect();
        }

        public virtual void Draw()
        {
            CalculateRect();
            GUI.color = Color.white;
            GUI.Box(bounds, "");
        }

        public virtual bool HandleEvent(Event e)
        {
            switch(e.type)
            {
            case EventType.MouseDown:
                if (bounds.Contains(e.mousePosition))
                {
                    e.Use();
                    return true;
                }
                break;
            }
            return false;
        }

        public EditorWindow GetWindow()
        {
            return window;
        }

        private void CalculateRect()
        {
            bounds = new Rect();
            switch(alignment)
            {
            case DockPosition.left:
                bounds.position = new Vector2(0f, 0f);
                bounds.size = new Vector2(scale, window.position.height);
                return;
            case DockPosition.right:
                bounds.position = new Vector2(window.position.width - scale, 0f);
                bounds.size = new Vector2(scale, window.position.height);
                return;
            case DockPosition.top:
                bounds.position = new Vector2(0f, 0f);
                bounds.size = new Vector2(window.position.width, scale);
                return;
            case DockPosition.bottom:
                bounds.position = new Vector2(0f, window.position.height - scale);
                bounds.size = new Vector2(window.position.width, scale);
                return;
            }
        }

        public enum DockPosition { left, right, top, bottom }
    }
}
