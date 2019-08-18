﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drt.Csv
{
    /// <summary>
    /// Class for reading from comma-separated-value (CSV) files
    /// </summary>
    public class CsvFileReader : CsvFileCommon, IDisposable
    {
        private bool _disposed = false;

        private delegate ToestandsFunctie ToestandsFunctie();

        private readonly StreamReader _reader;
        private string _currLine;
        private int _currPos;
        private readonly EmptyLineBehavior _emptyLineBehavior;

        // gebruikt door toestandsfuncties
        private List<string> _cells;
        private int _numCells;

        /// <summary>
        /// class voor het lezen van csv files.
        /// Een eventuele header regel wordt niet anders anders behandeld dan de rest van de csvfile.
        /// Tekstvelden mogen tussen een quote karakter gezet worden, codeer een quote karakter daarbinnen met behulp van twee quotes achter elkaar.
        /// </summary>
        /// <param name="stream">De invoer stream</param>
        /// <param name="fieldDelimiter">Typisch gesproken een puntkomma, een komma of een tab </param>
        /// <param name="quote">Wordt gebruikt om een tekstveld mee te omgeven, zodat speciale karakters gewoon als tekst worden geinterpretterd.</param>
        /// <param name="emptyLineBehavior">Bepaalt hoe je met lege regels in de invoer moet omgaan</param>
        public CsvFileReader(StreamReader stream, char fieldDelimiter = ';', char quote = '"', EmptyLineBehavior emptyLineBehavior = EmptyLineBehavior.NoCells) :
            base(fieldDelimiter, quote)
        {
            _reader = stream;
            _emptyLineBehavior = emptyLineBehavior;
        }

        /// <summary>
        /// Lees een regel van de stream en breek deze op in cellen die we teruggeven
        /// Let op: aan het einde van de file geven we een null collectie terug.
        /// </summary>
        public List<string> ReadRow()
        {
            ToestandsFunctie toestand = LeesRegel;
            while (toestand != null)
            {
                toestand = toestand();
            }

            return _cells;
        }

        private ToestandsFunctie LeesRegel()
        {
            // Read next line from the file
            _cells = null;
            _currLine = _reader.ReadLine();

            // Test for end of file
            if (_currLine == null)
                return null;

            // Test for empty line
            if (_currLine.Length == 0)
            {
                switch (_emptyLineBehavior)
                {
                    case EmptyLineBehavior.NoCells:
                        _cells = new List<string>();
                        return null;

                    case EmptyLineBehavior.EmptyCell:
                        _cells = new List<string> { string.Empty };
                        return null;

                    case EmptyLineBehavior.Ignore:
                        return LeesRegel;

                    case EmptyLineBehavior.EndOfFile:
                        return null;
                }
            }

            return LeesCellenStart;
        }

        private ToestandsFunctie LeesCellenStart()
        {
            _cells = new List<string>();
            _numCells = 0;
            _currPos = 0;

            return LeesCel;
        }

        private ToestandsFunctie LeesCellenEinde()
        {
            // Remove any unused cells from collection
            if (_numCells < _cells.Count)
                _cells.RemoveRange(_numCells, _cells.Count - _numCells);

            // klaar
            return null;
        }

        private ToestandsFunctie LeesCel()
        {
            string cel;
            // Read next cell
            if (_currPos < _currLine.Length && _currLine[_currPos] == Quote)
                cel = ReadQuotedCell();
            else
                cel = ReadUnquotedCell();

            // Add cell to list
            if (_numCells < _cells.Count)
                _cells[_numCells] = cel;
            else
                _cells.Add(cel);
            _numCells++;

            // Break if we reached the end of the line
            if (_currLine == null || _currPos == _currLine.Length)
                return LeesCellenEinde;

            // Otherwise skip delimiter
            Debug.Assert(_currLine[_currPos] == Delimiter);
            _currPos++;

            return LeesCel;
        }

        private ToestandsFunctie LeesUnquoted()
        {

            return LeesCellenStart;
        }

        private ToestandsFunctie LeesQuoted()
        {

            return LeesCellenStart;
        }


        /// <summary>
        /// Reads a quoted cell by reading from the current line until a
        /// closing quote is found or the end of the file is reached. On return,
        /// the current position points to the delimiter or the end of the last
        /// line in the file. Note: CurrLine may be set to null on return.
        /// </summary>
        private string ReadQuotedCell()
        {
            // Skip opening quote character
            Debug.Assert(_currPos < _currLine.Length && _currLine[_currPos] == Quote);
            _currPos++;

            // Parse cell
            StringBuilder builder = new StringBuilder();
            while (true)
            {
                if (_currPos == _currLine.Length)
                {
                    // End of line so attempt to read the next line
                    _currLine = _reader.ReadLine();
                    _currPos = 0;
                    // Done if we reached the end of the file
                    if (_currLine == null)
                        return builder.ToString();
                    // Otherwise, treat as a multi-line field
                    builder.Append(Environment.NewLine);
                }

                // Test for quote character
                if (_currLine[_currPos] == Quote)
                {
                    // If two quotes, skip first and treat second as literal
                    int nextPos = _currPos + 1;
                    if (nextPos < _currLine.Length && _currLine[nextPos] == Quote)
                        _currPos++;
                    else
                        break;  // Single quote ends quoted sequence
                }
                // Add current character to the cell
                builder.Append(_currLine[_currPos++]);
            }

            if (_currPos < _currLine.Length)
            {
                // Consume closing quote
                Debug.Assert(_currLine[_currPos] == Quote);
                _currPos++;
                // Append any additional characters appearing before next delimiter
                builder.Append(ReadUnquotedCell());
            }
            // Return cell value
            return builder.ToString();
        }

        /// <summary>
        /// Reads an unquoted cell by reading from the current line until a
        /// delimiter is found or the end of the line is reached. On return, the
        /// current position points to the delimiter or the end of the current
        /// line.
        /// </summary>
        private string ReadUnquotedCell()
        {
            int startPos = _currPos;
            _currPos = _currLine.IndexOf(Delimiter, _currPos);
            if (_currPos == -1)
                _currPos = _currLine.Length;
            return _currLine.Substring(startPos, _currPos - startPos);
        }

        public void Dispose()
        {
            this.Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _reader.Dispose();

            _disposed = true;
        }
    }

}
