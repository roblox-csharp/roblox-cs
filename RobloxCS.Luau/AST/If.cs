﻿namespace RobloxCS.Luau
{
    public class If : Statement
    {
        public Expression Condition { get; }
        public Statement Body { get; }
        public Statement? ElseBranch { get; }

        public If(Expression condition, Statement body, Statement? elseBranch = null)
        {
            Condition = condition;
            Body = body;
            ElseBranch = elseBranch;

            AddChildren([Condition, Body]);
            if (ElseBranch != null)
            {
                AddChild(ElseBranch);
            }
        }

        public override void Render(LuauWriter luau)
        {
            luau.Write("if ");
            Condition.Render(luau);
            luau.WriteLine(" then");
            luau.PushIndent();
            Body.Render(luau);

            var isElseIf = ElseBranch is If;
            if (ElseBranch != null)
            {
                luau.PopIndent();
                luau.Write("else" + (isElseIf ? "" : '\n'));
                if (!isElseIf)
                {
                    luau.PushIndent();
                }
                ElseBranch.Render(luau);
            }

            luau.PopIndent();
            if (!isElseIf)
            {
                luau.WriteLine("end");
            }
        }
    }
}