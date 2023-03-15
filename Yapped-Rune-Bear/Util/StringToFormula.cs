namespace Chomp.Util {
    // Token: 0x02000009 RID: 9
    public unsafe class StringToFormula {
        // Token: 0x0600010A RID: 266 RVA: 0x0000EF90 File Offset: 0x0000D190
        public decimal Eval(string expression) {
            List<string> tokens = getTokens(expression);
            var operandStack = new Stack<decimal>();
            var operatorStack = new Stack<string>();
            int tokenIndex = 0;
            while (tokenIndex < tokens.Count) {
                string token = tokens[tokenIndex];
                if (token == "(") {
                    operandStack.Push(this.Eval(getSubExpression(tokens, ref tokenIndex)));
                } else {
                    if (token == ")") {
                        throw new ArgumentException("Mis-matched parentheses in expression");
                    }
                    if (Array.IndexOf(_operators, token) >= 0) {
                        while (operatorStack.Count > 0 && Array.IndexOf(_operators, token) < Array.IndexOf(_operators, operatorStack.Peek())) {
                            string op = operatorStack.Pop();
                            decimal arg2 = operandStack.Pop();
                            decimal arg3 = operandStack.Pop();
                            operandStack.Push(__operations[Array.IndexOf(_operators, op)](arg3, arg2));
                        }
                        operatorStack.Push(token);
                    } else {
                        operandStack.Push(decimal.Parse(token));
                    }
                    tokenIndex++;
                }
            }
            while (operatorStack.Count > 0) {
                string op2 = operatorStack.Pop();
                decimal arg4 = operandStack.Pop();
                decimal arg5 = operandStack.Pop();
                operandStack.Push(__operations[Array.IndexOf(_operators, op2)](arg5, arg4));
            }
            return operandStack.Pop();
        }

        // Token: 0x0600010B RID: 267 RVA: 0x0000F0F0 File Offset: 0x0000D2F0
        private static string getSubExpression(List<string> tokens, ref int index) {
            var subExpr = new StringBuilder();
            int parenlevels = 1;
            index++;
            while (index < tokens.Count && parenlevels > 0) {
                string token = tokens[index];
                if (tokens[index] == "(") {
                    parenlevels++;
                }
                if (tokens[index] == ")") {
                    parenlevels--;
                }
                if (parenlevels > 0) {
                    _ = subExpr.Append(token);
                }
                index++;
            }
            return parenlevels > 0 ? throw new ArgumentException("Mis-matched parentheses in expression") : subExpr.ToString();
        }

        // Token: 0x0600010C RID: 268 RVA: 0x0000F17C File Offset: 0x0000D37C
        private static List<string> getTokens(string expression) {
            var tokens = new List<string>();
            var sb = new StringBuilder();
            foreach (char c in expression.Replace(" ", string.Empty)) {
                if (operators.Contains(c)) {
                    if (sb.Length > 0) {
                        tokens.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    tokens.Add(c.ToString());
                } else {
                    _ = sb.Append(c);
                }
            }
            if (sb.Length > 0) {
                tokens.Add(sb.ToString());
            }
            return tokens;
        }

        private static readonly string operators = "()^*/+-";
        // Token: 0x0400008E RID: 142
        private static readonly string[] _operators = new[] { "-", "+", "/", "*", "^" };
        private static decimal __Mul(decimal a1, decimal a2) => a1 * a2;
        private static decimal __Div(decimal a1, decimal a2) => a1 / a2;
        private static decimal __Sum(decimal a1, decimal a2) => a1 + a2;
        private static decimal __Sub(decimal a1, decimal a2) => a1 - a2;
        private static decimal __Pow(decimal a1, decimal a2) => (decimal)Math.Pow((double)a1, (double)a2);

        // Token: 0x0400008F RID: 143
        private static readonly delegate* managed<decimal, decimal, decimal>[] _operations = new delegate* managed<decimal, decimal, decimal>[] {
            &__Sub,
            &__Sum,
            &__Div,
            &__Mul,
            &__Pow
        };
        private static readonly delegate* managed<decimal, decimal, decimal>* __operations;

        static StringToFormula() {
            fixed (delegate* managed<decimal, decimal, decimal>* __ops = _operations) {
                __operations = __ops;
            }
        }
    }
}
