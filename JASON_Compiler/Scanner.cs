using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public enum Token_Class
{
    Int, Float, String, Read, Write, Repeat, Until, If, ElseIf, Else, Then, Return, Endl,
    PlusOp, MinusOp, MultiplyOp, DivideOp, AssignOp, LessThanOp, GreaterThanOp, EqualOp, NotEqualOp, AndOp, OrOp,
    Identifier, Number,main, StringLiteral, Comment,end,
    Semicolon, Comma, LParanthesis, RParanthesis, LCurly, RCurly
}

namespace JASON_Compiler
{
    public class Token
    {
        public string lex;
        public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Chars = new Dictionary<string, Token_Class>();

        public Scanner()
        {
            ReservedWords.Add("int", Token_Class.Int);
            ReservedWords.Add("float", Token_Class.Float);
            ReservedWords.Add("string", Token_Class.String);
            ReservedWords.Add("read", Token_Class.Read);
            ReservedWords.Add("write", Token_Class.Write);
            ReservedWords.Add("repeat", Token_Class.Repeat);
            ReservedWords.Add("until", Token_Class.Until);
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("elseif", Token_Class.ElseIf);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("then", Token_Class.Then);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("endl", Token_Class.Endl);
            ReservedWords.Add("main", Token_Class.main);
            ReservedWords.Add("end", Token_Class.end);

            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);
            Operators.Add(":=", Token_Class.AssignOp);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("=", Token_Class.EqualOp);
            Operators.Add("<>", Token_Class.NotEqualOp);
            Operators.Add("&&", Token_Class.AndOp);
            Operators.Add("||", Token_Class.OrOp);

            Chars.Add(";", Token_Class.Semicolon);
            Chars.Add(",", Token_Class.Comma);
            Chars.Add("(", Token_Class.LParanthesis);
            Chars.Add(")", Token_Class.RParanthesis);
            Chars.Add("{", Token_Class.LCurly);
            Chars.Add("}", Token_Class.RCurly);

        }

        public void StartScanning(string SourceCode)
        {
            for (int i = 0; i < SourceCode.Length; i++)
            {

                int j = i;
                char CurrentChar = SourceCode[i];
                string CurrentLexeme = CurrentChar.ToString();

                if (CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\n' || CurrentChar == '\t')
                    continue;


                if ((CurrentChar >= 'A' && CurrentChar <= 'Z') || (CurrentChar >= 'a' && CurrentChar <= 'z'))
                {
                    j++;
                    while (j < SourceCode.Length)
                    {
                        string curr = SourceCode[j].ToString();
                        string next = (j + 1 < SourceCode.Length) ? curr + SourceCode[j + 1].ToString() : "";


                        if (Operators.ContainsKey(curr) ||
                            Operators.ContainsKey(next) ||
                            Chars.ContainsKey(curr) ||
                            char.IsWhiteSpace(SourceCode[j]))
                        {
                            break;
                        }

                        CurrentLexeme += SourceCode[j];
                        j++;
                    }

                    FindTokenClass(CurrentLexeme);
                    i = j - 1;
                }

                // khaly .x yb2a 0.x
                else if ((CurrentChar >= '0' && CurrentChar <= '9') || CurrentChar == '.')
                {
                    if (CurrentChar == '.')
                    {

                        if (j + 1 >= SourceCode.Length || !isNumber(SourceCode[j + 1].ToString()))
                        {
                            j++;
                            while (j < SourceCode.Length)
                            {
                                string curr = SourceCode[j].ToString();
                                string next = (j + 1 < SourceCode.Length) ? curr + SourceCode[j + 1].ToString() : "";


                                if (Operators.ContainsKey(curr) ||
                                    Operators.ContainsKey(next) ||
                                    Chars.ContainsKey(curr) ||
                                    char.IsWhiteSpace(SourceCode[j]))
                                {
                                    break;
                                }

                                CurrentLexeme += SourceCode[j];
                                j++;
                            }
                            i = j - 1;

                            FindTokenClass(CurrentLexeme);
                        }
                        else
                        {

                            //char dot = CurrentLexeme[0];
                            //char zero = '0';
                            //CurrentLexeme = zero.ToString();
                            //CurrentLexeme += dot;



                            j++;
                            while (j < SourceCode.Length && ((SourceCode[j] >= '0' && SourceCode[j] <= '9' || SourceCode[j] == '.')))
                            {
                                CurrentLexeme += SourceCode[j];
                                j++;
                            }


                            // error zay 3a3
                            if (j < SourceCode.Length && ((SourceCode[j] >= 'A' && SourceCode[j] <= 'Z') || (SourceCode[j] >= 'a' && SourceCode[j] <= 'z')))
                            {
                                while (j < SourceCode.Length)
                                {
                                    string curr = SourceCode[j].ToString();
                                    string next = (j + 1 < SourceCode.Length) ? curr + SourceCode[j + 1].ToString() : "";


                                    if (Operators.ContainsKey(curr) ||
                                        Operators.ContainsKey(next) ||
                                        Chars.ContainsKey(curr) ||
                                        char.IsWhiteSpace(SourceCode[j]))
                                    {
                                        break;
                                    }

                                    CurrentLexeme += SourceCode[j];
                                    j++;
                                }
                                Errors.Error_List.Add("Invalid identifier format: " + CurrentLexeme);
                            }
                            else
                            {
                                FindTokenClass(CurrentLexeme);
                            }
                            i = j - 1;
                        }
                    }

                    else
                    {
                        j++;
                        if (j < SourceCode.Length)
                        {
                            while (((SourceCode[j] >= '0' && SourceCode[j] <= '9') || SourceCode[j] == '.'))
                            {
                                CurrentLexeme += SourceCode[j];
                                j++;
                                if (j >= SourceCode.Length) break;
                            }
                        }
                        if (j < SourceCode.Length && ((SourceCode[j] >= 'A' && SourceCode[j] <= 'Z') || (SourceCode[j] >= 'a' && SourceCode[j] <= 'z')))
                        {
                            while (j < SourceCode.Length)
                            {
                                string curr = SourceCode[j].ToString();
                                string next = (j + 1 < SourceCode.Length) ? curr + SourceCode[j + 1].ToString() : "";


                                if (Operators.ContainsKey(curr) ||
                                    Operators.ContainsKey(next) ||
                                    Chars.ContainsKey(curr) ||
                                    char.IsWhiteSpace(SourceCode[j]))
                                {
                                    break;
                                }

                                CurrentLexeme += SourceCode[j];
                                j++;
                            }

                            Errors.Error_List.Add("Invalid identifier format: " + CurrentLexeme);
                        }
                        else
                        {
                            FindTokenClass(CurrentLexeme);
                        }
                        i = j - 1;
                    }

                    i = j - 1;
                }
                // el String
                else if (CurrentChar == '"')
                {
                    j++;
                    while (j < SourceCode.Length && SourceCode[j] != '"')
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                    if (j < SourceCode.Length)
                    {
                        CurrentLexeme += SourceCode[j]; // 3lshan ye2fel
                        j++;
                    }
                    FindTokenClass(CurrentLexeme);
                    i = j - 1;
                }
                else if (CurrentChar == '/' && i + 1 < SourceCode.Length && SourceCode[i + 1] == '*')
                {
                    CurrentLexeme = "/*";
                    j += 2;

                    while (j + 1 < SourceCode.Length && !(SourceCode[j] == '*' && SourceCode[j + 1] == '/'))
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }

                    if (j + 1 < SourceCode.Length)
                    {

                        CurrentLexeme += "*";
                        CurrentLexeme += "/";
                        j += 2;
                    }
                    else
                    {

                        while (j < SourceCode.Length)
                        {
                            CurrentLexeme += SourceCode[j];
                            j++;
                        }
                    }

                    FindTokenClass(CurrentLexeme);
                    i = j - 1;
                }


                // law kaza haga gamb ba3D
                else if (i + 1 < SourceCode.Length && Operators.ContainsKey(CurrentChar.ToString() + SourceCode[i + 1].ToString()))
                {
                    CurrentLexeme = CurrentChar.ToString() + SourceCode[i + 1].ToString();
                    i++;
                    FindTokenClass(CurrentLexeme);
                }


                else if (Operators.ContainsKey(CurrentChar.ToString()) || Chars.ContainsKey(CurrentChar.ToString()))
                {
                    FindTokenClass(CurrentLexeme);
                }






                else
                {
                    j++;
                    while (j < SourceCode.Length)
                    {
                        string curr = SourceCode[j].ToString();
                        string next = (j + 1 < SourceCode.Length) ? curr + SourceCode[j + 1].ToString() : "";


                        if (Operators.ContainsKey(curr) ||
                            Operators.ContainsKey(next) ||
                            Chars.ContainsKey(curr) ||
                            char.IsWhiteSpace(SourceCode[j]))
                        {
                            break;
                        }

                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                    i = j - 1;

                    FindTokenClass(CurrentLexeme);
                }
            }
        }

        void FindTokenClass(string Lex)
        {
            Token_Class TC;
            Token Tok = new Token { lex = Lex };
            if (string.IsNullOrWhiteSpace(Lex))
                return;
            if (ReservedWords.TryGetValue(Tok.lex, out TC))
            {
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }
            else if (isIdentifier(Lex))
            {
                Tok.token_type = Token_Class.Identifier;
                Tokens.Add(Tok);
            }
            else if (isNumber(Lex))
            {
                Tok.token_type = Token_Class.Number;
                Tokens.Add(Tok);
            }
            else if (Operators.TryGetValue(Tok.lex, out TC) || Chars.TryGetValue(Tok.lex, out TC))
            {
                Tok.token_type = TC;
                Tokens.Add(Tok);
            }
            else if (isString(Lex))
            {
                Tok.token_type = Token_Class.StringLiteral;
                Tokens.Add(Tok);
            }
            else if (Lex[0] == '/' && Lex[1] == '*' && Lex[Lex.Length - 1] == '/' && Lex[Lex.Length - 2] == '*')
            {
                //Tok.token_type = Token_Class.Comment;
                //Tokens.Add(Tok);
                return;
            }
            else
            {
                Errors.Error_List.Add("Unrecognized token: " + Lex);
            }
        }

        bool isIdentifier(string lex)
        {
            bool isValid = false;

            Regex id_regex = new Regex(@"^[a-zA-Z][a-zA-Z0-9]*$");
            if (id_regex.IsMatch(lex))
            {
                return true;
            }
            return isValid;
        }
        bool isNumber(string lex)
        {
            bool isValid = false;

            Regex constant_regex = new Regex(@"^([0-9]+(\.[0-9]+)?|\.[0-9]+)$");
            if (constant_regex.IsMatch(lex))
            {
                isValid = true;
            }
            return isValid;
        }

        bool isString(string lex)
        {
            if ((lex[0] == '"' && lex[lex.Length - 1] == '"') && lex.Length > 1)
                return true;
            return false;
        }
    }

}

