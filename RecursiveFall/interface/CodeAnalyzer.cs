using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace @interface
{
    internal class CodeAnalyzer
    {
        string pattern = @"(\+)|(\-)|(L)|(l)|(\d)";
        private AnalyzerState AnalyzerState = AnalyzerState.FIRST_SYMBOL;
        private int ColumnCount = 0;
        RichTextBox richTextBox;
        bool firstDigit = false;
        private string lexems = "";
        private string result = "";


        public CodeAnalyzer(string text, RichTextBox richTextBox)
        {
            this.richTextBox = richTextBox;
            lexems = text;
            Parse();
            richTextBox.ResetText();
            result = result.Remove(result.Length - 3);
            richTextBox.AppendText(result);
        }

        private void AppendInfo(string info = "")
        {
            result += $"{info} - ";
        }

        private string Parse() 
        {
            string result = Expression();
            if (ColumnCount != lexems.Length)
            {
                throw new ArgumentOutOfRangeException("Ошибка в выражении " + lexems[ColumnCount]);
            }
            return result;
        }

        private string Expression(bool innerExpression = false) 
        {
            if (!innerExpression) 
            {
                AppendInfo("E");
            }
            string first = Term();

            while (ColumnCount < lexems.Length)
            {
                char op = lexems[ColumnCount];
                if (!op.Equals('+') && !op.Equals('-')) 
                {
                    break;
                } 
                else
                {
                    AppendInfo(op.ToString());
                    ColumnCount++;
                }

                // находим второе слагаемое (вычитаемое)
                string second = Term();
            }
            return first;
        }

        private string Term()
        {
            AppendInfo("T");
            string first = Factor();

            while (ColumnCount < lexems.Length)
            {
                char op = lexems[ColumnCount];
                if (!op.Equals('*') && !op.Equals('/')) 
                {
                    break;
                } 
                else
                {
                    AppendInfo(op.ToString());
                    ColumnCount++;
                }

                // находим второй множитель (делитель)
                string second = Factor();
            }
            return first;
        }

        private string Factor()
        {
            AppendInfo("F");
            string first = Verb();

            while (ColumnCount < lexems.Length)
            {
                char op = lexems[ColumnCount];
                if (!op.Equals('^'))
                {
                    break;
                }
                else
                {
                    AppendInfo("^");
                    ColumnCount++;
                }

                // находим второй множитель (делитель)
                string second = Verb();
            }
            return first;
        }

        private string Verb()
        {
            AppendInfo("V");
            char next = lexems[ColumnCount];
            string result;
            if (next.Equals('('))
            {
                AppendInfo("(E)");
                ColumnCount++;
                // если выражение в скобках, то рекурсивно переходим на обработку подвыражения типа Е
                result = Expression(true);
                char closingBracket;
                if (ColumnCount < lexems.Length)
                {
                    closingBracket = lexems[ColumnCount];
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Неожиданный конец выражения");
                }
                if (ColumnCount < lexems.Length && closingBracket.Equals(')'))
                {
                    ColumnCount++;
                    return result;
                }
                throw new ArgumentOutOfRangeException("')' отсутствует, но " + closingBracket + "найдена");
            }
            else if (char.IsDigit(next))
            {
                return Number();
            }
            else if (Regex.IsMatch(next.ToString(), "[a-zA-Z]"))
            {
                return Id();
            }
            else 
            {
                AppendInfo("ε");
                if (ColumnCount < lexems.Length)
                {
                    ColumnCount++;
                }
                return "";
            }
            // в противном случае токен должен быть числом

        }

        private string Id()
        {
            AppendInfo("id");
            while (ColumnCount < lexems.Length && (char.IsDigit(lexems[ColumnCount]) || Regex.IsMatch(lexems[ColumnCount].ToString(), "[a-zA-Z]")))
            {
                ColumnCount++;
            }
            return "";
        }

        private string Number()
        {
            AppendInfo("number");
            while (ColumnCount < lexems.Length && char.IsDigit(lexems[ColumnCount])) 
            {
                ColumnCount++;
            }
            return "";
        }
    }
}
