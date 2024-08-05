namespace RobloxCS.Luau
{
    public abstract class Node
    {
        public Node? Parent { get; set; } = null;
        public List<Node> Children { get; } = [];

        private List<Node> _descendants = null!;
        public List<Node> Descendants
        {
            get
            {
                if (_descendants == null)
                {
                    _descendants = [];
                    foreach (var child in Children)
                    {
                        _descendants.Add(child);
                        _descendants.AddRange(child.Descendants);
                    }
                }
                return _descendants;
            }
            set
            {
                _descendants = value;
            }
        }

        public abstract void Render(LuauWriter luau);

        protected void AddChild(Node child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        protected void AddChildren(IEnumerable<Node> children)
        {
            foreach (var child in children)
            {
                AddChild(child);
            }
        }
    }
}