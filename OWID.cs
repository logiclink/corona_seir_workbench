using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LogicLink.Corona {

    /// <summary>
    /// SARS-COV-2 vaccinations per date and country 
    /// from Our World in Data (Oxford MArtin School & Oxford University)
    /// </summary>
    public class OWID {

        private static Dictionary<string, List<Record>> _dic;                   // Memory Cache
        private static readonly SemaphoreSlim _sms = new SemaphoreSlim(1, 1);   // Semaphore blocking multiple LoadAsync calls and GetDataAsync before LoadAsync finishes

        // Remarks: https://github.com/owid/covid-19-data/blob/master/public/data/vaccinations/ has vaccination data from various sources
        //          See https://ourworldindata.org/covid-vaccinations for explainations.
        private const string OWID_URL = "https://raw.githubusercontent.com/owid/covid-19-data/master/public/data/vaccinations/vaccinations.csv";

        /// <summary>
        /// Fills missing dates with the values of the Record from the last known date in the list.
        /// </summary>
        /// <param name="l">List of Records, must have one element at least.</param>
        /// <param name="r">Record</param>
        private void FillAndAdd(List<Record> l, Record r) {
            Record rLast = l[^1];
            for(DateTime dt = rLast.Date.AddDays(1); dt < r.Date; dt = dt.AddDays(1))
                l.Add(new Record(dt, rLast.Vaccinated));
            l.Add(r);
        }

        /// <summary>
        /// Structure for a record of a CSV file row
        /// </summary>
        public struct Record {

            /// <summary>
            /// Factory method for parsing a row into a record.
            /// </summary>
            /// <param name="s">Row of the CSV file</param>
            /// <returns>Tuple of country string and record</returns>
            public static (string Country, Record Record) FromString(string s, string sCountryPrevious = default, double dVaccinatedPrevious = 0) {
                ReadOnlySpan<char> sp = s;
                int i = 0;

                int j = sp.QuotedIndexOf(',');
                string sCountry = sp[i] == '"'
                                    ? sp[i + j - 1] == '"'
                                    ? new string(sp.Slice(i + 1, j - 2))
                                    : new string(sp.Slice(i + 1, j - 1))
                                    : new string(sp.Slice(i, j));
                i += j + 1;

                switch(sCountry) {
                    case "United States":
                        sCountry = "US";
                        break;
                }

                // Ignore second column with 3-letter code
                j = sp[i..].QuotedIndexOf(',');
                i += j + 1;

                j = sp[i..].QuotedIndexOf(',');
                DateTime dtDate = DateTime.Parse(sp.Slice(i, j), CultureInfo.InvariantCulture);
                i += j + 1;

                j = sp[i..].QuotedIndexOf(',');
                double dVaccinations = j > 0 ? double.Parse(sp.Slice(i, j), NumberStyles.Float, CultureInfo.InvariantCulture) : 0;
                i += j + 1;

                j = sp[i..].QuotedIndexOf(',');
                double dVaccinated = j > 0 ? double.Parse(sp.Slice(i, j), NumberStyles.Float, CultureInfo.InvariantCulture) : (dVaccinations != 0 ? dVaccinations : (sCountry == sCountryPrevious ? dVaccinatedPrevious : 0));
                i += j + 1;

                return (sCountry, new Record(dtDate, dVaccinated, sCountry == sCountryPrevious ? (int)(dVaccinated - dVaccinatedPrevious) : 0));
            }

            /// <summary>
            /// Aligns an enumeration of <see cref="Record"/> objects to a start and end date.
            /// </summary>
            /// <param name="source">Enumeration of <see cref="Record"/> objects with continuous dates.</param>
            /// <param name="dtStart">First date of the sequence of vaccinated people.</param>
            /// <param name="dVaccinatedStart">Value of vaccinated people before the enumeration of <see cref="Record"/> objects starts</param>
            /// <param name="dtEnd">Last date of the sequence of vaccinated people.</param>
            /// <param name="dΔVaccinatedEnd">Delta per day of vaccinated people after the enumeration of <see cref="Record"/> objects ends. Starting value ist the last number of vaccinated people in the Enumeration of <see cref="Record"/> objects.</param>
            /// <returns>enumeration of ints with vaccinated people</returns>
            public static IEnumerable<double> AlignVaccinated(IEnumerable<Record> source, DateTime dtStart, double dVaccinatedStart, DateTime dtEnd, double dΔVaccinatedEnd) {
                if(source == null || source.Count() == 0) {
                    for(DateTime dt = dtStart; dt <= dtEnd; dt = dt.AddDays(1))
                        yield return dVaccinatedStart;
                } else {
                    DateTime dtSourceFirst = source.First().Date;
                    DateTime dtSourceLast = dtEnd;
                    double dSourceVaccinatedLast = dVaccinatedStart;
                    for(DateTime dt = dtStart; dt < dtSourceFirst; dt = dt.AddDays(1))
                        yield return dVaccinatedStart;
                    foreach(Record item in source)
                        if(item.Date >= dtStart && item.Date <= dtEnd) {
                            yield return item.Vaccinated;
                            dtSourceLast = item.Date;
                            dSourceVaccinatedLast = item.Vaccinated;
                        }
                    for(DateTime dt = dtSourceLast.AddDays(1); dt <= dtEnd; dt = dt.AddDays(1))
                        yield return dSourceVaccinatedLast += dΔVaccinatedEnd;
                }
            }

            #region Public readonly properties

            public readonly DateTime Date;          // Date of the record
            public readonly double Vaccinated;      // Total number of vaccinated individuals. For "World" this exceeds Int32.MaxValue (2.147.483.647)
            public readonly int DailyVaccinated;    // Number of vaccinated individuals for the day

            #endregion

            #region Constructors

            /// <summary>
            /// Creates and initializes a new record
            /// </summary>
            /// <param name="dtDate">Date of the record</param>
            /// <param name="dVaccinated">Total number of vaccinated individuals</param>
            /// <param name="iDailyVaccinated">Number of vaccinated individuals for the day</param>
            public Record(DateTime dtDate, double dVaccinated, int iDailyVaccinated) {
                Date = dtDate;
                Vaccinated = dVaccinated;
                DailyVaccinated = iDailyVaccinated;
            }

            /// <summary>
            /// Creates and initializes a new record without daily individuals
            /// </summary>
            /// <param name="dtDate">Date of the record</param>
            /// <param name="dVaccinated">Total number of vaccinated individuals</param>
            public Record(DateTime dtDate, double dVaccinated) : this(dtDate, dVaccinated, 0) { }

            #endregion

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString() => $"{Date} \t{Vaccinated}";
        }

        /// <summary>
        /// Loads the current Our World in Data vaccination data in a separate thread
        /// </summary>
        /// <returns>awaitable Task</returns>
        public async Task LoadAsync() {
            await Task.Run(async () => {
                _sms.Wait();
                try {
                    if(_dic != null) return;
                    Dictionary<string, List<Record>> dic = new Dictionary<string, List<Record>>();
                    using(FileStream fs = new FileStream(await Download.GetCachedAsync(OWID_URL), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using(StreamReader rd = new StreamReader(fs)) {
                            await rd.ReadLineAsync();
                            string s = default;
                            Record r = default;
                            while(!rd.EndOfStream) {
                                (s, r) = Record.FromString(await rd.ReadLineAsync(), s, r.Vaccinated);
                                if(dic.TryGetValue(s, out List<Record> l)) {
                                    Record rLast = l[^1];
                                    for(DateTime dt = rLast.Date.AddDays(1); dt < r.Date; dt = dt.AddDays(1))
                                        l.Add(new Record(dt, rLast.Vaccinated));
                                    l.Add(r);
                                } else
                                    dic[s] = new List<Record> { r };
                            }
                        }
                        _dic = dic;
                } finally {
                    _sms.Release();
                }
            });
        }

        /// <summary>
        /// Returns Our World in Data vaccination data for a country
        /// </summary>
        /// <param name="sCountry">Name of the Country</param>
        /// <returns>Asynchronous enumerator</returns>
        public async IAsyncEnumerable<Record> GetDataAsync(string sCountry) {
            if(_dic == null)
                await LoadAsync();
            
            if(!_dic.ContainsKey(sCountry))
                yield break;

            foreach(Record r in _dic[sCountry])
                yield return r;
        }
    }
}
