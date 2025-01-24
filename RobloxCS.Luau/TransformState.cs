using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobloxCS.Luau;

namespace RobloxCS.Luau {
    public class TransformState {
        public List<Statement> preReqStatementStack = new List<Statement>();
        public void prereq(Statement statement) {
            preReqStatementStack.Add(statement);
        }
        public void prereqList(List<Statement> statements) {
            preReqStatementStack.AddRange(statements);
        }
    }
}
