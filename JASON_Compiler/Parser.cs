using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace JASON_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();
        public string Name;

        public Node(string N)
        {
            this.Name = N;
        }
    }

    public class Parser
    {
        int InputPointer = 0;
        List<Token> TokenStream;
        public Node root;

        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            root = new Node("Program");
            root.Children.Add(Program());
            return root;
        }

        Node Program()
        {
            Node program = new Node("Program");
            program.Children.Add(FunctionList());
            program.Children.Add(Main_thing());
            return program;
        }

        Node Main_thing()
        {
            Node main = new Node("Main");
            main.Children.Add(DataType());
            main.Children.Add(match(Token_Class.main));
            main.Children.Add(match(Token_Class.LParanthesis));
            main.Children.Add(match(Token_Class.RParanthesis));
            main.Children.Add(function_body());
            return main;
        }

        Node function_body()
        {
            Node f_body = new Node("function_body");
            f_body.Children.Add(match(Token_Class.LCurly));
            bool has_return = false;
            f_body.Children.Add(Statement_list(ref has_return));
            if (has_return == false)
            {
                Errors.Error_List.Add("Expected a return at the end of function");
            }
            f_body.Children.Add(match(Token_Class.RCurly));
            return f_body;
        }

        Node return_statement()
        {
            Node ret = new Node("return_statement");
            ret.Children.Add(match(Token_Class.Return));
            ret.Children.Add(Expression());
            ret.Children.Add(match(Token_Class.Semicolon));
            return ret;
        }

        Node Expression()
        {
            Node expr = new Node("Expression");

            if (InputPointer < TokenStream.Count)
            {
                Token_Class current = TokenStream[InputPointer].token_type;

                if (current == Token_Class.StringLiteral)
                {
                    expr.Children.Add(match(Token_Class.StringLiteral));
                }
                else if (current == Token_Class.LParanthesis)
                {
                    expr.Children.Add(Equation());
                }
                else if (current == Token_Class.Number || current == Token_Class.Identifier)
                {
                    if (InputPointer + 1 < TokenStream.Count)
                    {
                        Token_Class next = TokenStream[InputPointer + 1].token_type;

                        if (next == Token_Class.PlusOp || next == Token_Class.MinusOp ||
                            next == Token_Class.MultiplyOp || next == Token_Class.DivideOp)
                        {
                            expr.Children.Add(Equation());
                        }
                        else if (next == Token_Class.Semicolon || next == Token_Class.RParanthesis ||
                                 next == Token_Class.Comma || next == Token_Class.Then ||
                                 next == Token_Class.Until)
                        {
                            expr.Children.Add(Term());
                        }
                        else
                        {
                            Errors.Error_List.Add($"Syntax Error: Expected operator (+, -, *, /) after '{TokenStream[InputPointer].lex}', but found '{TokenStream[InputPointer + 1].lex}'");
                            InputPointer++;
                            return expr;
                        }
                    }
                    else
                    {
                        expr.Children.Add(Term());
                    }
                }
                else
                {
                    Errors.Error_List.Add($"Invalid start of expression: Found '{current}'");
                    InputPointer++;
                }
            }

            return expr;
        }

        Node Equation()
        {
            Node eq = new Node("Equation");
            eq.Children.Add(Primary());
            eq.Children.Add(EquationTail());
            return eq;
        }

        Node Primary()
        {
            Node primary = new Node("Primary");

            if (InputPointer < TokenStream.Count)
            {
                Token_Class current = TokenStream[InputPointer].token_type;

                if (current == Token_Class.LParanthesis)
                {
                    primary.Children.Add(match(Token_Class.LParanthesis));
                    primary.Children.Add(Equation());
                    if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.RParanthesis)
                    {
                        primary.Children.Add(match(Token_Class.RParanthesis));
                    }
                    else
                    {
                        Errors.Error_List.Add("Expected ')' to close equation");
                    }
                }
                else
                {
                    primary.Children.Add(Term());
                }
            }

            return primary;
        }

        Node EquationTail()
        {
            Node eqTail = new Node("EquationTail");

            if (InputPointer < TokenStream.Count)
            {
                Token_Class current = TokenStream[InputPointer].token_type;
                if (current == Token_Class.PlusOp || current == Token_Class.MinusOp ||
                    current == Token_Class.MultiplyOp || current == Token_Class.DivideOp)
                {
                    eqTail.Children.Add(match(current));
                    eqTail.Children.Add(Primary());
                    eqTail.Children.Add(EquationTail());
                }
                else if (current == Token_Class.Number || current == Token_Class.Identifier)
                {
                    Errors.Error_List.Add($"Syntax Error: Missing operator before '{TokenStream[InputPointer].lex}'");
                    InputPointer++;
                }
            }

            return eqTail;
        }

        Node Term()
        {
            Node term = new Node("Term");

            if (InputPointer < TokenStream.Count)
            {
                Token_Class current = TokenStream[InputPointer].token_type;

                if (current == Token_Class.Number)
                {
                    term.Children.Add(match(Token_Class.Number));
                }
                else if (current == Token_Class.Identifier)
                {
                    if (InputPointer + 1 < TokenStream.Count &&
                        TokenStream[InputPointer + 1].token_type == Token_Class.LParanthesis)
                    {
                        term.Children.Add(Function_call());
                    }
                    else
                    {
                        term.Children.Add(match(Token_Class.Identifier));
                    }
                }
                else
                {
                    Errors.Error_List.Add($"Expected a valid term (Number or Identifier), but found '{current}'");
                    InputPointer++;
                }
            }

            return term;
        }

        Node Parameter()
        {
            Node p = new Node("Parameter");
            p.Children.Add(DataType());
            p.Children.Add(match(Token_Class.Identifier));
            return p;
        }

        Node Parameter_list()
        {
            Node p_list = new Node("Parameter_List");
            p_list.Children.Add(Parameter());
            p_list.Children.Add(Parameter_list_tail());
            return p_list;
        }

        Node Parameter_list_tail()
        {
            Node p_list = new Node("Parameter_List_tail");
            while (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                p_list.Children.Add(match(Token_Class.Comma));
                p_list.Children.Add(Parameter());
            }
            return p_list;
        }

        Node Function_call()
        {
            Node call = new Node("Function_Call");
            call.Children.Add(match(Token_Class.Identifier));
            call.Children.Add(match(Token_Class.LParanthesis));
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type != Token_Class.RParanthesis)
            {
                call.Children.Add(arguments());
            }
            call.Children.Add(match(Token_Class.RParanthesis));
            return call;
        }

        Node arguments()
        {
            Node arg = new Node("Arguments");
            arg.Children.Add(Expression());
            arg.Children.Add(arguments_tail());
            return arg;
        }

        Node arguments_tail()
        {
            Node args = new Node("Arguments_tail");
            while (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                args.Children.Add(match(Token_Class.Comma));
                args.Children.Add(Expression());
            }
            return args;
        }

        Node FunctionList()
        {
            Node list = new Node("FunctionList");
            if (InputPointer + 1 < TokenStream.Count &&
                TokenStream[InputPointer + 1].token_type == Token_Class.main)
            {
                return list;
            }

            while (InputPointer + 1 < TokenStream.Count &&
                (TokenStream[InputPointer].token_type == Token_Class.Int ||
                 TokenStream[InputPointer].token_type == Token_Class.Float ||
                 TokenStream[InputPointer].token_type == Token_Class.String) &&
                TokenStream[InputPointer + 1].token_type != Token_Class.main)
            {
                list.Children.Add(FunctionStatement());
            }
            return list;
        }

        Node FunctionStatement()
        {
            Node stmt = new Node("Func_Statement");
            stmt.Children.Add(Function_decl());
            stmt.Children.Add(function_body());
            return stmt;
        }

        Node Function_decl()
        {
            Node decl = new Node("Function_declaration");
            decl.Children.Add(DataType());
            decl.Children.Add(match(Token_Class.Identifier));
            decl.Children.Add(match(Token_Class.LParanthesis));
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type != Token_Class.RParanthesis)
            {
                decl.Children.Add(Parameter_list());
            }
            decl.Children.Add(match(Token_Class.RParanthesis));
            return decl;
        }

        Node DataType()
        {
            Node DataType = new Node("DataType");

            if (InputPointer >= TokenStream.Count)
            {
                return DataType;
            }

            Token_Class check = TokenStream[InputPointer].token_type;
            if (check == Token_Class.Int)
            {
                DataType.Children.Add(match(Token_Class.Int));
            }
            else if (check == Token_Class.Float)
            {
                DataType.Children.Add(match(Token_Class.Float));
            }
            else if (check == Token_Class.String)
            {
                DataType.Children.Add(match(Token_Class.String));
            }
            else
            {
                Errors.Error_List.Add($"Parsing Error: Expected data type (int, float, or string), but found '{check}'");
                InputPointer++;
            }
            return DataType;
        }

        Node Statement_list(ref bool has_return)
        {
            Node list = new Node("Statement_list");

            while (InputPointer < TokenStream.Count)
            {
                if (InputPointer >= TokenStream.Count)
                    break;

                Token_Class current = TokenStream[InputPointer].token_type;

                if (current == Token_Class.Int ||
                    current == Token_Class.Float ||
                    current == Token_Class.String ||
                    current == Token_Class.Identifier ||
                    current == Token_Class.Write ||
                    current == Token_Class.Read ||
                    current == Token_Class.Return ||
                    current == Token_Class.If ||
                    current == Token_Class.Repeat)
                {
                    if (current == Token_Class.Return)
                    {
                        has_return = true;
                    }
                    list.Children.Add(Statement());
                }
                else
                {
                    break;
                }
            }
            return list;
        }

        Node Statement()
        {
            Node stat = new Node("Statement");

            if (InputPointer < TokenStream.Count)
            {
                Token_Class current = TokenStream[InputPointer].token_type;

                if (current == Token_Class.Int ||
                    current == Token_Class.Float ||
                    current == Token_Class.String)
                {
                    stat.Children.Add(declaration_statement());
                }
                else if (current == Token_Class.Identifier)
                {
                    if (InputPointer + 1 < TokenStream.Count &&
                        TokenStream[InputPointer + 1].token_type == Token_Class.AssignOp)
                    {
                        stat.Children.Add(Assignment_statement());
                    }
                    else if (InputPointer + 1 < TokenStream.Count &&
                             TokenStream[InputPointer + 1].token_type == Token_Class.LParanthesis)
                    {
                        stat.Children.Add(Function_call());
                        stat.Children.Add(match(Token_Class.Semicolon));
                    }
                    else
                    {
                        Errors.Error_List.Add($"Syntax Error: Expected assignment or function call after identifier '{TokenStream[InputPointer].lex}'");
                        InputPointer++;
                    }
                }
                else if (current == Token_Class.Write)
                {
                    stat.Children.Add(Write());
                }
                else if (current == Token_Class.Read)
                {
                    stat.Children.Add(Read());
                }
                else if (current == Token_Class.Return)
                {
                    stat.Children.Add(return_statement());
                }
                else if (current == Token_Class.If)
                {
                    stat.Children.Add(if_statement());
                }
                else if (current == Token_Class.Repeat)
                {
                    stat.Children.Add(repeat_statement());
                }
                else
                {
                    Errors.Error_List.Add($"Syntax Error: Unexpected token '{current}' in Statement");
                    InputPointer++;
                }
            }
            return stat;
        }

        Node Write()
        {
            Node write = new Node("Write_Statement");
            write.Children.Add(match(Token_Class.Write));
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Endl)
            {
                write.Children.Add(match(Token_Class.Endl));
            }
            else
            {
                write.Children.Add(Expression());
            }
            write.Children.Add(match(Token_Class.Semicolon));
            return write;
        }

        Node Read()
        {
            Node read = new Node("Read_Statement");
            read.Children.Add(match(Token_Class.Read));
            read.Children.Add(match(Token_Class.Identifier));
            read.Children.Add(match(Token_Class.Semicolon));
            return read;
        }

        Node Assignment_statement()
        {
            Node assign = new Node("Assignment_Statement");
            assign.Children.Add(match(Token_Class.Identifier));
            if (assign.Children[0] == null) return assign;

            assign.Children.Add(match(Token_Class.AssignOp));
            if (assign.Children[1] == null) return assign;

            assign.Children.Add(Expression());
            assign.Children.Add(match(Token_Class.Semicolon));
            if (assign.Children[3] == null)
            {
                while (InputPointer < TokenStream.Count &&
                       TokenStream[InputPointer].token_type != Token_Class.Semicolon &&
                       TokenStream[InputPointer].token_type != Token_Class.RCurly &&
                       TokenStream[InputPointer].token_type != Token_Class.end &&
                       TokenStream[InputPointer].token_type != Token_Class.Until)
                {
                    InputPointer++;
                }
                if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Semicolon)
                {
                    assign.Children[3] = match(Token_Class.Semicolon);
                }
            }
            return assign;
        }

        Node Condition()
        {
            Node Cond = new Node("Condition");
            Cond.Children.Add(match(Token_Class.Identifier));
            if (Cond.Children[0] == null) return Cond;

            if (InputPointer < TokenStream.Count &&
                (TokenStream[InputPointer].token_type == Token_Class.EqualOp ||
                 TokenStream[InputPointer].token_type == Token_Class.LessThanOp ||
                 TokenStream[InputPointer].token_type == Token_Class.GreaterThanOp ||
                 TokenStream[InputPointer].token_type == Token_Class.NotEqualOp))
            {
                Cond.Children.Add(match(TokenStream[InputPointer].token_type));
            }
            else
            {
                Errors.Error_List.Add($"Syntax Error: Expected comparison operator (==, <, >, <>), but found '{TokenStream[InputPointer].token_type}'");
                InputPointer++;
                return Cond;
            }

            Cond.Children.Add(Term());
            return Cond;
        }

        Node Condition_Statement()
        {
            Node Cond_stat = new Node("Condition_Statement");
            Cond_stat.Children.Add(Condition());
            Cond_stat.Children.Add(Condition_Statement_tail());
            return Cond_stat;
        }

        Node Condition_Statement_tail()

        {
            Node tail = new Node("Condition_tail");
            while (InputPointer < TokenStream.Count &&
                   (TokenStream[InputPointer].token_type == Token_Class.OrOp ||
                    TokenStream[InputPointer].token_type == Token_Class.AndOp))
            {
                if (TokenStream[InputPointer].token_type == Token_Class.OrOp)
                {
                    tail.Children.Add(match(Token_Class.OrOp));
                }
                else
                {
                    tail.Children.Add(match(Token_Class.AndOp));
                }
                tail.Children.Add(Condition());
            }
            return tail;
        }

        Node repeat_statement()
        {
            Node rep = new Node("Repeat_Statement");
            rep.Children.Add(match(Token_Class.Repeat));
            bool has_return = false;
            rep.Children.Add(Statement_list(ref has_return));
            rep.Children.Add(match(Token_Class.Until));
            rep.Children.Add(Condition_Statement());
            return rep;
        }

        Node if_statement()
        {
            Node if_state = new Node("If_Statement");
            if_state.Children.Add(match(Token_Class.If));
            if_state.Children.Add(Condition_Statement());
            if_state.Children.Add(match(Token_Class.Then));
            bool has_return = false;
            if_state.Children.Add(Statement_list(ref has_return));
            if_state.Children.Add(else_if());
            if_state.Children.Add(is_else());
            if_state.Children.Add(match(Token_Class.end));
            return if_state;
        }

        Node else_if()
        {
            Node else_if_state = new Node("ElseIf_Statement");
            while (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.ElseIf)
            {
                else_if_state.Children.Add(match(Token_Class.ElseIf));
                else_if_state.Children.Add(Condition_Statement());
                else_if_state.Children.Add(match(Token_Class.Then));
                bool has_return = false;
                else_if_state.Children.Add(Statement_list(ref has_return));
            }
            return else_if_state;
        }

        Node is_else()
        {
            Node else_block = new Node("Else_Block");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Else)
            {
                else_block.Children.Add(match(Token_Class.Else));
                bool has_return = false;
                else_block.Children.Add(Statement_list(ref has_return));
            }
            return else_block;
        }

        Node declaration_statement()
        {
            Node decl = new Node("Declaration_Statement");
            decl.Children.Add(DataType());
            decl.Children.Add(init_decl());
            decl.Children.Add(init_decl_tail());
            decl.Children.Add(match(Token_Class.Semicolon));
            return decl;
        }

        Node init_decl()
        {
            Node init = new Node("Declaration");
            init.Children.Add(match(Token_Class.Identifier));
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.AssignOp)
            {
                init.Children.Add(optional_assign());
            }
            return init;
        }

        Node optional_assign()
        {
            Node asi = new Node("Optional_Assignment");
            asi.Children.Add(match(Token_Class.AssignOp));
            asi.Children.Add(Expression());
            
            return asi;
        }

        Node init_decl_tail()
        {
            Node init = new Node("Declaration_List");
            while (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                init.Children.Add(match(Token_Class.Comma));
                init.Children.Add(init_decl());
            }
            return init;
        }

        public Node match(Token_Class ExpectedToken)
        {
            if (InputPointer < TokenStream.Count)
            {
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                    InputPointer++;
                    return new Node(ExpectedToken.ToString());
                }
                else
                {
                    Errors.Error_List.Add($"Parsing Error: Expected '{ExpectedToken}', but found '{TokenStream[InputPointer].token_type}'");

                    if (ExpectedToken == Token_Class.Semicolon || ExpectedToken == Token_Class.end ||
                        ExpectedToken == Token_Class.RCurly || ExpectedToken == Token_Class.Until)
                    {
                        return null;
                    }

                    while (InputPointer < TokenStream.Count)
                    {
                        Token_Class current = TokenStream[InputPointer].token_type;
                        if (current == Token_Class.Semicolon ||
                            current == Token_Class.RCurly ||
                            current == Token_Class.end ||
                            current == Token_Class.Until ||
                            current == Token_Class.If ||
                            current == Token_Class.Repeat ||
                            current == Token_Class.Return ||
                            current == Token_Class.Int ||
                            current == Token_Class.Float ||
                            current == Token_Class.String ||
                            current == Token_Class.Read ||
                            current == Token_Class.Write ||
                            current == Token_Class.Else ||
                            current == Token_Class.ElseIf ||
                            current == Token_Class.Then)
                        {
                            break;
                        }
                        InputPointer++;
                    }
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add($"Parsing Error: Expected '{ExpectedToken}', but reached end of input");
                InputPointer++;
                return null;
            }
        }

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }

        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}