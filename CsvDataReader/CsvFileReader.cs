using System;
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
        private int _linenr;

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
            _linenr = 0;
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

        private string ReadLine()
        {
            _linenr++;
            return _reader.ReadLine();
        }

        private ToestandsFunctie LeesRegel()
        {
            // Read next line from the file
            _cells = null;
            _currLine = ReadLine();

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
            _currPos = 0;

            return LeesCel;
        }

        private ToestandsFunctie LeesCel()
        {
            string cel;
            // Read next cell
            if (_currPos < _currLine.Length && _currLine[_currPos] == Quote)
                cel = ReadQuotedCell();
            else
                cel = ReadUnquotedCell();
            _cells.Add(cel);

            // Break if we reached the end of the line
            if (_currLine == null || _currPos == _currLine.Length)
                return null;

            // Otherwise skip delimiter
            Debug.Assert(_currLine[_currPos] == Delimiter);
            _currPos++;

            return LeesCel;
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
            var builder = new StringBuilder();
            while (true)
            {
                // zoek de eerstvolgende quote
                int quotePos = _currLine.IndexOf(Quote, _currPos);
                if (quotePos == -1)
                {
                    // quote niet gevonden: ...\r\n
                    string s = _currLine.Substring(_currPos, _currLine.Length - _currPos);
                    builder.Append(s);
                    builder.Append(Environment.NewLine);
                    _currLine = ReadLine();
                    _currPos = 0;
                }
                else if (quotePos + 1 == _currLine.Length)
                {
                    // quote aan het einde van de regel: ...."\r\n
                    string s = _currLine.Substring(_currPos, quotePos - _currPos);
                    builder.Append(s);
                    _currPos = _currLine.Length;
                    return builder.ToString();
                }
                else if (_currLine[quotePos + 1] == Quote)
                {
                    // dubbele quote:  ...""...
                    quotePos++;
                    string s = _currLine.Substring(_currPos, quotePos - _currPos);
                    builder.Append(s);
                    _currPos = quotePos + 1;
                }
                else if (_currLine[quotePos + 1] == Delimiter)
                {
                    // einde deze cel:  ...";...
                    string s = _currLine.Substring(_currPos, quotePos - _currPos);
                    builder.Append(s);
                    _currPos = quotePos + 1;
                    return builder.ToString();
                }
                else
                {
                    // een losse enkele quote: ..."....
                    // dit zou eigenlijk niet mogen. We doen maar alsof deze tweemaal voorkomt...
                    quotePos++;
                    string s = _currLine.Substring(_currPos, quotePos - _currPos);
                    builder.Append(s);
                    _currPos = quotePos;
                }
            }
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

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _reader.Dispose();

            _disposed = true;
        }
    }

}
