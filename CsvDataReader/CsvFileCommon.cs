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
    /// Determines how empty lines are interpreted when reading CSV files.
    /// These values do not affect empty lines that occur within quoted fields
    /// or empty lines that appear at the end of the input file.
    /// </summary>
    public enum EmptyLineBehavior
    {
        /// <summary>
        /// Empty lines are interpreted as a line with zero cells.
        /// </summary>
        NoCells,
        /// <summary>
        /// Empty lines are interpreted as a line with a single empty cell.
        /// </summary>
        EmptyCell,
        /// <summary>
        /// Empty lines are skipped over as though they did not exist.
        /// </summary>
        Ignore,
        /// <summary>
        /// An empty line is interpreted as the end of the input file.
        /// </summary>
        EndOfFile,
    }

    /// <summary>
    /// Common base class for CSV reader and writer classes.
    /// </summary>
    public abstract class CsvFileCommon
    {
        public CsvFileCommon(char delimiter = ';', char quote = '"')
        {
            Delimiter = delimiter;
            Quote = quote;
        }

        /// <summary>
        /// These are special characters in CSV files. If a cell contains any
        /// of these characters, the entire cell is wrapped in double quotes.
        /// </summary>
        protected char[] SpecialChars = new char[] { ';', '"', '\r', '\n' };

        // Indexes into SpecialChars for characters with specific meaning
        private const int DelimiterIndex = 0;
        private const int QuoteIndex = 1;

        /// <summary>
        /// Gets/sets the character used for cell delimiters.
        /// </summary>
        public char Delimiter
        {
            get { return SpecialChars[DelimiterIndex]; }
            set { SpecialChars[DelimiterIndex] = value; }
        }

        /// <summary>
        /// Gets/sets the character used for cell quotes.
        /// </summary>
        public char Quote
        {
            get { return SpecialChars[QuoteIndex]; }
            set { SpecialChars[QuoteIndex] = value; }
        }
    }
}
