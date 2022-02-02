using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RUCPs.Console
{
    public class ConsoleTable
    {
        private string[] m_columns;
        private List<string[]> m_rows = new List<string[]>();

        public int ColumnsCount => m_columns.Length;

        public ConsoleTable(params string[] columns)
        {
            m_columns = columns;
        }

        public void AddRow(params string[] row)
        {
            if (row == null || row.Length != m_columns.Length)
            {
                string[] new_row = new string[m_columns.Length];
                for (int i = 0; i < new_row.Length; i++)
                {
                    new_row[i] = (row != null && i < row.Length) ? row[i] : "-";
                }
                m_rows.Add(new_row);
            }
            else { m_rows.Add(row); }
        }

        /// <summary>
        /// Sorts the table by value from the column with the specified name
        /// Returns TRUE if sorting was successful
        /// </summary>
        /// <param name="colum"></param>
        public bool SortingBy(string columnName)
        {
            int columnIndex = Array.FindIndex(m_columns, (c) => c.Equals(columnName));
            if (columnIndex < 0) return false;

            m_rows.Sort((row_a, row_b) => CompareNumber(row_a[columnIndex], row_b[columnIndex]));
            return true;
        }

        public void Draw()
        {
            string[] table = GetText();
            foreach(string row in table)
            {
                System.Console.WriteLine(row);
            }
        }

        public string[] GetText()
        {
            List<string> resultTable = new List<string>();

            //Максимальная ширина ячеек в строках
            int[] widths = new int[m_columns.Length];
            for (int i = 0; i < widths.Length; i++)
            {
                //Поиск строки с наибольшим размером ячейки в столбце i
                string[] row = m_rows.Aggregate((a, b) => (a[i].Length > b[i].Length) ? a : b);

                //Если размер заголовка больше чем ячейки
                widths[i] = m_columns[i].Length > row[i].Length ? m_columns[i].Length : row[i].Length;
            }

            string banner = String.Format(GetFormat(m_columns, widths), m_columns);
            resultTable.Add(banner);
            resultTable.Add(new string('-', banner.Length));
            foreach (string[] row in m_rows)
            {
                resultTable.Add(String.Format(GetFormat(row, widths), row));
            }
            return resultTable.ToArray();
        }

        private string GetFormat(string[] row, int[] widths)
        {
            string format = "|";
            for (int i = 0; i < row.Length; i++)
            {
                format += $"{{{i},{widths[i] + 1}}} |";
            }
            return format;
        }

        /// <summary>
        /// Сравнивает две строки, если в строках пресутсвуют числа сравнение идет по 1 найденному числу
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int CompareNumber(string a, string b)
        {
            //Поиск числа в строке А
            string resultA = Regex.Match(a.Replace('.', ','), @"[-+]?[0-9]*\,?[0-9]+").Value;
            //Если число не найдено сортировка по string
            if (string.IsNullOrEmpty(resultA)) return string.Compare(a, b);

            // Поиск числа в строке B
            string resultB = Regex.Match(b.Replace('.', ','), @"[-+]?[0-9]*\,?[0-9]+").Value;
            //Если число не найдено сортировка по string
            if (string.IsNullOrEmpty(resultB)) return string.Compare(a, b);

            float floatA, floatB;

            if (int.TryParse(resultA, out int intValueA)) { floatA = intValueA; }
            else if (float.TryParse(resultA, out floatA)) { }

            if (int.TryParse(resultB, out int intValueB)) { floatB = intValueB; }
            else if (float.TryParse(resultB, out floatB)) { }

            if (floatA < floatB) return -1;
            if (floatA > floatB) return 1;
            return 0;
        }
    }
}
