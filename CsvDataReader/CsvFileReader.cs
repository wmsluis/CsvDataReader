using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Drt.Csv
{
    /// <summary>
    /// Class voor het lezen van comma-separated-value (CSV) files
    /// </summary>
    public class CsvFileReader : CsvFileCommon, IDisposable
    {
        private delegate ToestandsFunctie ToestandsFunctie();

        private bool _disposed = false;
        private readonly StreamReader _reader;
        private readonly EmptyLineBehavior _emptyLineBehavior;

        // data betreffende de toestand
        private string _currLine;
        private int _delimPos;
        private List<string> _values;
        private bool _return;

        /// <summary>
        /// Class voor het lezen van csv files volgens RFC 4180.
        /// Csv file regels worden in cellen opgebroken en teruggegeven.
        /// We doen niet aan datatype conversies: alle velden zijn van type string.
        /// Een eventuele header regel wordt niet anders anders behandeld dan de rest van de csv file.
        /// Tekstvelden mogen tussen een quote karakter gezet worden, 
        /// codeer een quote karakter daarbinnen met behulp van twee quotes achter elkaar.
        /// </summary>
        /// <param name="stream">De invoer stream, zorg zelf voor de juiste encoding en eventuele buffering</param>
        /// <param name="fieldDelimiter">Typisch gesproken een puntkomma, een komma of een tab </param>
        /// <param name="quote">Tekstvelden kunnen tussen quotes zitten, om
        /// zodat speciale karakters als gewone tekst wordt verwerkt.</param>
        /// <param name="emptyLineBehavior">Bepaal hoe je met lege regels in de invoer om wilt gaan</param>
        public CsvFileReader(StreamReader stream, char fieldDelimiter = ';', char quote = '"',
            EmptyLineBehavior emptyLineBehavior = EmptyLineBehavior.NoCells)
            : base(fieldDelimiter, quote)
        {
            _reader = stream;
            _emptyLineBehavior = emptyLineBehavior;
        }

        /// <summary>
        /// Lees een regel van de stream en breek deze op in cellen die we teruggeven in een lijst.
        /// </summary>
        /// <returns>true zolang we nog niet aan het einde zijn, anders false</returns>
        public bool ReadRow()
        {
            ToestandsFunctie toestand = ProcesRegel;
            while (toestand != Klaar)
            {
                // Roep de functie aan waar de toestand pointer naar wijst.
                // De return waarde van die functie call bepaalt de nieuwe toestand.
                toestand = toestand();
            }

            return _return;
        }

        #region Deze functies beschrijven een toestand en parsen ook een gedeelte van een regel.
        private ToestandsFunctie ProcesRegel()
        {
            _delimPos = -1;

            if (_reader.EndOfStream)
                return EindeFile;

            return LeesRegel;
        }

        private ToestandsFunctie EindeFile()
        {
            _return = false;
            _values = null;
            return Klaar;
        }

        private ToestandsFunctie Klaar()
        {
            return null;
        }

        private ToestandsFunctie LeesRegel()
        {
            _currLine = _reader.ReadLine();
            _values = new List<string>();

            if (_currLine.Length == 0)
                return ProcesLegeRegel;

            // Lees de cellen nu één voor één.
            return LeesCel;
        }

        private ToestandsFunctie ProcesLegeRegel()
        {
            if (_emptyLineBehavior == EmptyLineBehavior.EndOfFile)
                return EindeFile;

            if (_emptyLineBehavior == EmptyLineBehavior.Ignore)
                return ProcesRegel;  // ga door naar de volgende regel

            if (_emptyLineBehavior == EmptyLineBehavior.EmptyCell)
                _values.Add(string.Empty);

            return EindeRegel;
        }

        private ToestandsFunctie EindeRegel()
        {
            _return = true;
            return Klaar;
        }

        private ToestandsFunctie LeesCel()
        {
            Debug.Assert(_values != null);
            Debug.Assert(_currLine != null);
            Debug.Assert(_currLine.Length > 0);
            Debug.Assert(_delimPos == -1 || _delimPos == _currLine.Length || _currLine[_delimPos] == Delimiter);

            if (_delimPos == _currLine.Length)
                return EindeRegel;

            if (_currLine[_delimPos + 1] == Quote)
                return ReadQuotedCell;
            else
                return ReadUnquotedCell;
        }

        /// <summary>
        /// Reads a quoted cell by reading from the current line until a
        /// closing quote is found or the end of the file is reached.
        /// On return, the current position points to the delimiter or the end of the last
        /// line in the file. Note: CurrLine may be set to null on return.
        /// </summary>
        private ToestandsFunctie ReadQuotedCell()
        {
            Debug.Assert(_currLine[_delimPos + 1] == Quote);

            var builder = new StringBuilder();
            while (true)
            {
                // iedere iteratie doet een stuk tot de volgende delimiter, of tot een ingebedde dubble quote of een stuk dat op een nieuwe regel staat
                Debug.Assert(_delimPos == -1 || _currLine[_delimPos] == Delimiter || _currLine[_delimPos] == Quote);

                // zoek de eerstvolgende quote
                int quotePos = _currLine.IndexOf(Quote, _delimPos + 2);
                if (quotePos == -1)
                {
                    // quote niet gevonden: ...\r\n
                    builder.Append(Substring(_delimPos + 1, _currLine.Length));
                    // ga door op de volgende regel
                    builder.Append(Environment.NewLine);
                    _currLine = _reader.ReadLine();
                    _delimPos = -1;
                }
                else if (quotePos + 1 == _currLine.Length)
                {
                    // quote aan het einde van de regel: ...."\r\n
                    builder.Append(Substring(_delimPos + 1, quotePos));
                    _delimPos = _currLine.Length;
                    _values.Add(builder.ToString(1, builder.Length - 1));
                    return LeesCel;
                }
                else if (_currLine[quotePos + 1] == Quote)
                {
                    // dubbele quote:  ...""...
                    builder.Append(Substring(_delimPos + 1, quotePos));
                    _delimPos = quotePos; // mss niet helemaal een delimiter ... komt goed in laatste iteratie
                }
                else if (_currLine[quotePos + 1] == Delimiter)
                {
                    // einde deze cel:  ...";...
                    builder.Append(Substring(_delimPos + 1, quotePos + 1));
                    _delimPos = quotePos + 1;
                    _values.Add(builder.ToString(1, builder.Length - 2));
                    return LeesCel;
                }
                else
                {
                    // een losse enkele quote: ..."....
                    // dit zou eigenlijk niet mogen. We doen maar alsof deze tweemaal voorkomt...
                    builder.Append(Substring(_delimPos + 1, quotePos + 1));
                    _delimPos = quotePos;
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
            Debug.Assert(_delimPos == -1 || _currLine[_delimPos] == Delimiter);
            Debug.Assert(_currLine[_delimPos + 1] != Quote);

            int startPos = _delimPos + 1;
            _delimPos = _currLine.IndexOf(Delimiter, startPos);
            if (_delimPos == -1)
                _delimPos = _currLine.Length;
            string cel = Substring(startPos, _delimPos);
            _values.Add(cel);
            return LeesCel;
        }

        #endregion

        public List<string> Values()
        {
            return _values;
        }

        /// <summary>
        /// Stukje _currLine vanaf start tot eind (exclusief)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="eind"></param>
        /// <returns></returns>
        private string Substring(int start, int eind)
        {
            return _currLine.Substring(start, eind - start);
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
