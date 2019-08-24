using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Drt.Csv
{
    /// <summary>
    /// Class for writing to comma-separated-value (CSV) files.
    /// </summary>
    public class CsvFileWriter : CsvFileCommon, IDisposable
    {
        private bool _disposed = false;

        // Private members
        private StreamWriter _writer;
        private string OneQuote = null;
        private string TwoQuotes = null;
        private string QuotedFormat = null;

        /// <summary>
        /// Initializes a new instance of the CsvFileWriter class for the
        /// specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        public CsvFileWriter(StreamWriter stream, char fieldDelimiter = ';', char quote = '"') :
            base(fieldDelimiter, quote)
        {
            _writer = stream;
        }

        /// <summary>
        /// Writes a row of cells to the current CSV file.
        /// </summary>
        /// <param name="cells">The list of cells to write</param>
        public void WriteRow(List<string> cells)
        {
            // Verify required argument
            if (cells == null)
                throw new ArgumentNullException("cells");

            // Ensure we're using current quote character
            if (OneQuote == null || OneQuote[0] != Quote)
            {
                OneQuote = String.Format("{0}", Quote);
                TwoQuotes = String.Format("{0}{0}", Quote);
                QuotedFormat = String.Format("{0}{{0}}{0}", Quote);
            }

            // Write each cells
            for (int i = 0; i < cells.Count; i++)
            {
                // Add delimiter if this isn't the first cell
                if (i > 0)
                    _writer.Write(Delimiter);
                // Write this cell
                if (cells[i].IndexOfAny(SpecialChars) == -1)
                    _writer.Write(cells[i]);
                else
                    _writer.Write(QuotedFormat, cells[i].Replace(OneQuote, TwoQuotes));
            }
            _writer.WriteLine();
        }

        public void Dispose()
        {
            this.Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        // Propagate Dispose to StreamWriter
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _writer.Dispose();

            _disposed = true;
        }
    }
}
