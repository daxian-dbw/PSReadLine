/********************************************************************++
Copyright (c) Microsoft Corporation.  All rights reserved.
--********************************************************************/

using System;
using System.Text;

namespace Microsoft.PowerShell
{
    public partial class PSConsoleReadLine
    {
        private void WriteBlankLines(int count)
        {
            _console.BlankRestOfLine();
            for (int i = 1; i < count; i++)
            {
                _console.Write("\n");
                _console.BlankRestOfLine();
            }
        }

        private void WriteBlankLines(int top, int count)
        {
            var savedCursorLeft = _console.CursorLeft;
            var savedCursorTop = _console.CursorTop;

            _console.SetCursorPosition(left: 0, top);
            WriteBlankLines(count);
            _console.SetCursorPosition(savedCursorLeft, savedCursorTop);
        }

        private void WriteBlankRestOfLine(int left, int top)
        {
            var savedCursorLeft = _console.CursorLeft;
            var savedCursorTop = _console.CursorTop;

            _console.SetCursorPosition(left, top);
            _console.BlankRestOfLine();
            _console.SetCursorPosition(savedCursorLeft, savedCursorTop);
        }

        private static string Spaces(int cnt)
        {
            return cnt < _spaces.Length
                ? (_spaces[cnt] ?? (_spaces[cnt] = new string(' ', cnt)))
                : new string(' ', cnt);
        }

        internal static int LengthInBufferCells(string str)
        {
            return LengthInBufferCells(str, 0, str.Length);
        }

        internal static int LengthInBufferCells(string str, int start, int end)
        {
            var sum = 0;
            for (var i = start; i < end; i++)
            {
                var c = str[i];
                if (c == 0x1b && (i+1) < end && str[i+1] == '[')
                {
                    // Simple escape sequence skipping
                    i += 2;
                    while (i < end && str[i] != 'm')
                        i++;

                    continue;
                }
                sum += LengthInBufferCells(c);
            }
            return sum;
        }

        internal static int LengthInBufferCells(StringBuilder sb, int start, int end)
        {
            var sum = 0;
            for (var i = start; i < end; i++)
            {
                var c = sb[i];
                if (c == 0x1b && (i + 1) < end && sb[i + 1] == '[')
                {
                    // Simple escape sequence skipping
                    i += 2;
                    while (i < end && sb[i] != 'm')
                        i++;

                    continue;
                }
                sum += LengthInBufferCells(c);
            }
            return sum;
        }

        internal static int LengthInBufferCells(char c)
        {
            if (c < 256)
            {
                // We render ^C for Ctrl+C, so return 2 for control characters
                return char.IsControl(c) ? 2 : 1;
            }

            return Wcwidth.UnicodeCalculator.GetWidth(c);
        }

        private static string SubstringByCells(string text, int countOfCells)
        {
            return SubstringByCells(text, 0, countOfCells);
        }

        private static string SubstringByCells(string text, int start, int countOfCells)
        {
            int length = SubstringLengthByCells(text, start, countOfCells);
            return length == 0 ? string.Empty : text.Substring(start, length);
        }

        private static int SubstringLengthByCells(string text, int countOfCells)
        {
            return SubstringLengthByCells(text, 0, countOfCells);
        }

        private static int SubstringLengthByCells(string text, int start, int countOfCells)
        {
            int cellLength = 0;
            int charLength = 0;

            for (int i = start; i < text.Length; i++)
            {
                cellLength += LengthInBufferCells(text[i]);

                if (cellLength > countOfCells)
                {
                    return charLength;
                }

                charLength++;

                if (cellLength == countOfCells)
                {
                    return charLength;
                }
            }

            return charLength;
        }

        private static int SubstringLengthByCellsFromEnd(string text, int countOfCells)
        {
            return SubstringLengthByCellsFromEnd(text, text.Length - 1, countOfCells);
        }

        private static int SubstringLengthByCellsFromEnd(string text, int start, int countOfCells)
        {
            int cellLength = 0;
            int charLength = 0;

            for (int i = start; i >= 0; i--)
            {
                cellLength += LengthInBufferCells(text[i]);

                if (cellLength > countOfCells)
                {
                    return charLength;
                }

                charLength++;

                if (cellLength == countOfCells)
                {
                    return charLength;
                }
            }

            return charLength;
        }

        private static (int newStart, int newEnd) TrimSubstringInPlace(string text, int start, int end)
        {
            int newStart = start;
            int newEnd = end;

            for (; newStart <= end; newStart++)
            {
                if (!char.IsWhiteSpace(text[newStart]))
                {
                    break;
                }
            }

            for (; newEnd > newStart; newEnd--)
            {
                if (!char.IsWhiteSpace(text[newEnd]))
                {
                    break;
                }
            }

            // Return the new start/end after triming, or (-1, -1) if the substring only consists of whitespaces.
            return newStart > newEnd ? (-1, -1) : (newStart, newEnd);
        }
    }
}
