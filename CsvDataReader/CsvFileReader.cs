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

        private readonly StreamReader _reader;
        private readonly EmptyLineBehavior _emptyLineBehavior;

        // data betreffende de toestand
        private delegate ToestandsFunctie ToestandsFunctie();
        private string _currLine;
        private int _currPos;
        private List<string> _cells;

        /// <summary>
        /// Class voor het lezen van csv files.
        /// Csv file regels worden in cellen opgebroken en teruggegeven. 
        /// We doen niet aan datatype conversies: alle velden zijn van type string.
        /// Een eventuele header regel wordt niet anders anders behandeld dan de rest van de csv file.
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
            ToestandsFunctie toestand = ProcesRegel;
            while (toestand != null)
            {
                // Roep de functie aan waar toestand pointer naar wijst. 
                // De return waarde van die functie call bepaalt de nieuwe toestand.
                toestand = toestand();
            }

            return _cells;
        }

        #region Deze functies beschrijven ook een toestand van het parsen van een regel weer
        private ToestandsFunctie ProcesRegel()
        {
            _cells = null;
            _currPos = -1;

            // Test voor einde van de file
            if (_reader.EndOfStream)
                return null;

            // lees de volgende regel
            _currLine = _reader.ReadLine();
            if (_currLine.Length == 0)
                return ProcesLegeRegel;

            return ProcesNietLegeRegel;
        }

        private ToestandsFunctie ProcesLegeRegel()
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
                    return ProcesRegel;  // ga door naar de volgende regel
                case EmptyLineBehavior.EndOfFile:
                    return null;
                default:
                    return null;  // hier komen we nooit
            }
        }

        private ToestandsFunctie ProcesNietLegeRegel()
        {
            Debug.Assert(_currPos == -1);
            Debug.Assert(_currLine != null && _currLine.Length > 0);

            _cells = new List<string>();

            return LeesCel;
        }

        private ToestandsFunctie LeesCel()
        {
            Debug.Assert(_cells != null);
            Debug.Assert(_currLine != null && _currLine.Length > 0);

            // _currpos = -1, zit op een delimiter, of line length

            // Break if we reached the end of the line
            if (_currPos == _currLine.Length)
                return null;

            Debug.Assert(_currPos == -1 || _currLine[_currPos] == Delimiter);

            _currPos++;

            if (_currLine[_currPos] == Quote)
                return ReadQuotedCell;
            else
                return ReadUnquotedCell;
        }

        #endregion

        /// <summary>
        /// Reads a quoted cell by reading from the current line until a
        /// closing quote is found or the end of the file is reached. On return,
        /// the current position points to the delimiter or the end of the last
        /// line in the file. Note: CurrLine may be set to null on return.
        /// </summary>
        private ToestandsFunctie ReadQuotedCell()
        {
            // Skip opening quote character
            Debug.Assert(_currPos == 0 || _currLine[_currPos - 1] == Delimiter);
            Debug.Assert(_currLine[_currPos] == Quote);
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
                    _currLine = _reader.ReadLine();
                    _currPos = 0;
                }
                else if (quotePos + 1 == _currLine.Length)
                {
                    // quote aan het einde van de regel: ...."\r\n
                    string s = _currLine.Substring(_currPos, quotePos - _currPos);
                    builder.Append(s);
                    _currPos = _currLine.Length;
                    string cel = builder.ToString();
                    _cells.Add(cel);
                    return LeesCel;
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
                    string cel = builder.ToString();
                    _cells.Add(cel);
                    return LeesCel;
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
        private ToestandsFunctie ReadUnquotedCell()
        {
            Debug.Assert(_currPos == 0 || _currLine[_currPos - 1] == Delimiter);
            Debug.Assert(_currLine[_currPos] != Quote);

            int startPos = _currPos;
            _currPos = _currLine.IndexOf(Delimiter, _currPos);
            if (_currPos == -1)
                _currPos = _currLine.Length;
            string cel = _currLine.Substring(startPos, _currPos - startPos);
            _cells.Add(cel);
            return LeesCel;
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
