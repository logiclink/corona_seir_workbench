using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LogicLink.Corona {

    /// <summary>
    /// Confirmed, recovered and deaths SARS-COV-2 cases per date and country 
    /// from the Johns Hopkins University Center for Systems Science and 
    /// Engineering (CSSE) data
    /// </summary>
    public class JHU {

        private static Dictionary<string, List<Record>> _dic;                   // Memory Cache
        private static readonly SemaphoreSlim _sms = new SemaphoreSlim(1, 1);   // Semaphore blocking multiple LoadAsync calls and GetDataAsync before LoadAsync finishes

        // Remarks: https://datahub.io/core/covid-19 aggregates Johns Hopkins University Center for Systems Science and Engineering (CSSE) data
        private const string JHU_URL = "https://datahub.io/core/covid-19/r/countries-aggregated.csv";

        /// <summary>
        /// Structure for a record of a CSV file row
        /// </summary>
        public struct Record {

            /// <summary>
            /// Factory method for parsing a row into a record.
            /// </summary>
            /// <param name="s">Row of teh CSV file</param>
            /// <param name="sCountryPrevious">Country of the previous row for decission if a new country starts.</param>
            /// <param name="iConfirmedPrevious">Number of confirmed cases of the previous row.</param>
            /// <returns>Tuple of country string and record</returns>
            public static (string Country, Record Record) FromString(string s, string sCountryPrevious = default, int iConfirmedPrevious = 0) {
                ReadOnlySpan<char> sp = s;
                int i = 0;

                int j = sp.QuotedIndexOf(',');
                DateTime dtDate = DateTime.Parse(sp.Slice(i, j), CultureInfo.InvariantCulture);
                i += j + 1;

                j = sp.Slice(i).QuotedIndexOf(',');
                string sCountry = sp[i] == '"'
                                  ? sp[i + j - 1] == '"'
                                    ? new string(sp.Slice(i + 1, j - 2))
                                    : new string(sp.Slice(i + 1, j - 1))
                                  : new string(sp.Slice(i, j));
                i += j + 1;

                j = sp.Slice(i).QuotedIndexOf(',');
                int iConfirmed = int.Parse(sp.Slice(i, j));
                i += j + 1;

                j = sp.Slice(i).QuotedIndexOf(',');
                int iRecovered = int.Parse(sp.Slice(i, j));
                i += j + 1;

                int iDeaths = int.Parse(sp.Slice(i));

                return (sCountry, new Record(dtDate, iConfirmed, sCountry == sCountryPrevious ? iConfirmed - iConfirmedPrevious : 0, iRecovered, iDeaths));
            }

            #region Public readonly properties

            public readonly DateTime Date;          // Date of the record
            public readonly int Confirmed;          // Total number of confirmed infected individuals
            public readonly int DailyConfirmed;     // Number of confirmed infected individuals for the day
            public readonly int Recovered;          // Total number of recovered individuals
            public readonly int Deaths;             // Total number of deaths of infected individuals

            #endregion

            #region Constructors

            /// <summary>
            /// Creates and initializes a new record
            /// </summary>
            /// <param name="dtDate">Date of the record</param>
            /// <param name="iConfirmed">Total number of confirmed infected individuals</param>
            /// <param name="iDailyConfirmed">Number of confirmed infected individuals for the day</param>
            /// <param name="iRecovered">Total number of recovered individuals</param>
            /// <param name="iDeaths">Total number of deaths of infected individuals</param>
            public Record(DateTime dtDate, int iConfirmed, int iDailyConfirmed, int iRecovered, int iDeaths) {
                Date = dtDate;
                Confirmed = iConfirmed;
                DailyConfirmed = iDailyConfirmed;
                Recovered = iRecovered;
                Deaths = iDeaths;
            }

            /// <summary>
            /// Creates and initializes a new record without daily individuals
            /// </summary>
            /// <param name="dtDate">Date of the record</param>
            /// <param name="iConfirmed">Total number of confirmed infected individuals</param>
            /// <param name="iRecovered">Total number of recovered individuals</param>
            /// <param name="iDeaths">Total number of deaths of infected individuals</param>
            public Record(DateTime dtDate, int iConfirmed, int iRecovered, int iDeaths) : this(dtDate, iConfirmed, 0, iRecovered, iDeaths) { }

            #endregion

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString() => $"{Date}\t{Confirmed}\t{Recovered}\t{Deaths}";
        }

        /// <summary>
        /// Loads the current Johns Hopkins University cornona data in a separate thread
        /// </summary>
        /// <returns>awaitable Task</returns>
        public async Task LoadAsync() {
            await Task.Run(async () => {
                _sms.Wait();
                try {
                    if(_dic != null) return;
                    Dictionary<string, List<Record>> dic = new Dictionary<string, List<Record>>();
                    using(FileStream fs = new FileStream(await Download.GetCachedAsync(JHU_URL), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using(StreamReader rd = new StreamReader(fs)) {
                            await rd.ReadLineAsync();
                            string s = default;
                            Record r = default;
                            while(!rd.EndOfStream) {
                                (s, r) = Record.FromString(await rd.ReadLineAsync(), s, r.Confirmed);
                                if(dic.TryGetValue(s, out List<Record> l))
                                    l.Add(r);
                                else {
                                    l = new List<Record>();
                                    l.Add(r);
                                    dic[s] = l;
                                }
                            }
                        }
                        _dic = dic;
                    } finally {
                        _sms.Release();
                    }
            });
        }

        /// <summary>
        /// Returns Johns-Hopkins-University cornona data for a country
        /// </summary>
        /// <param name="sCountry">Name of the Country</param>
        /// <returns>Asynchronous enumerator</returns>
        public async IAsyncEnumerable<Record> GetDataAsync(string sCountry) {
            if(_dic == null)
                await LoadAsync();
            foreach(Record r in _dic[sCountry])
                yield return r;
        }
    }
}
