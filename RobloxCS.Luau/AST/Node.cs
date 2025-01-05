using RobloxCS.Macros;

namespace RobloxCS.Luau
{
    public abstract class Node
    {
        public Node? Parent { get; private set; } = null;
        public List<Node> Children { get; } = [];
        public MacroKind? ExpandedByMacro { get; private set; }

        private List<Node> _descendants = null!;
        public List<Node> Descendants
        {
            get
            {
                if (_descendants != null)
                    return _descendants;
                
                _descendants = [];
                foreach (var child in Children)
                {
                    _descendants.Add(child);
                    _descendants.AddRange(child.Descendants);
                }

                return _descendants;
            }
            set => _descendants = value;
        }

        public abstract void Render(LuauWriter luau);

        public void MarkExpanded(MacroKind macroKind) => ExpandedByMacro = macroKind;

        protected void AddChild(Node child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        protected void AddChildren(IEnumerable<Node> children)
        {
            foreach (var child in children)
                AddChild(child);
        }
    }
}