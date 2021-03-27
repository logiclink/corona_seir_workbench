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
            public static (string Country, Record Record) FromString(string s, string sCountryPrevious = default, int iVaccinatedPrevious = 0) {
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
                int iVaccinations = j > 0 ? int.Parse(sp.Slice(i, j)) : 0;
                i += j + 1;

                j = sp[i..].QuotedIndexOf(',');
                int iVaccinated = j > 0 ? (int)Math.Round(double.Parse(sp.Slice(i, j), NumberStyles.Float, CultureInfo.InvariantCulture)) : (iVaccinations != 0 ? iVaccinations : (sCountry == sCountryPrevious ? iVaccinatedPrevious : 0));
                i += j + 1;

                return (sCountry, new Record(dtDate, iVaccinated, sCountry == sCountryPrevious ? iVaccinated - iVaccinatedPrevious : 0));
            }

            /// <summary>
            /// Aligns an enumeration of <see cref="Record"/> objects to a start and end date.
            /// </summary>
            /// <param name="source">Enumeration of <see cref="Record"/> objects with continuous dates.</param>
            /// <param name="dtStart">First date of the sequence of vaccinated people.</param>
            /// <param name="iVaccinatedStart">Value of vaccinated people before the enumeration of <see cref="Record"/> objects starts</param>
            /// <param name="dtEnd">Last date of the sequence of vaccinated people.</param>
            /// <param name="iΔVaccinatedEnd">Delta per day of vaccinated people after the enumeration of <see cref="Record"/> objects ends. Starting value ist the last number of vaccinated people in the Enumeration of <see cref="Record"/> objects.</param>
            /// <returns>enumeration of ints with vaccinated people</returns>
            public static IEnumerable<int> AlignVaccinated(IEnumerable<Record> source, DateTime dtStart, int iVaccinatedStart, DateTime dtEnd, int iΔVaccinatedEnd) {
                if(source == null || source.Count() == 0) {
                    for(DateTime dt = dtStart; dt <= dtEnd; dt = dt.AddDays(1))
                        yield return iVaccinatedStart;
                } else {
                    DateTime dtSourceFirst = source.First().Date;
                    DateTime dtSourceLast = dtEnd;
                    int iSourceVaccinatedLast = iVaccinatedStart;
                    for(DateTime dt = dtStart; dt < dtSourceFirst; dt = dt.AddDays(1))
                        yield return iVaccinatedStart;
                    foreach(Record item in source)
                        if(item.Date >= dtStart && item.Date <= dtEnd) {
                            yield return item.Vaccinated;
                            dtSourceLast = item.Date;
                            iSourceVaccinatedLast = item.Vaccinated;
                        }
                    for(DateTime dt = dtSourceLast.AddDays(1); dt <= dtEnd; dt = dt.AddDays(1))
                        yield return iSourceVaccinatedLast += iΔVaccinatedEnd;
                }
            }

            #region Public readonly properties

            public readonly DateTime Date;          // Date of the record
            public readonly int Vaccinated;         // Total number of vaccinated individuals
            public readonly int DailyVaccinated;    // Number of vaccinated individuals for the day

            #endregion

            #region Constructors

            /// <summary>
            /// Creates and initializes a new record
            /// </summary>
            /// <param name="dtDate">Date of the record</param>
            /// <param name="iVaccinated">Total number of vaccinated individuals</param>
            /// <param name="iDailyVaccinated">Number of vaccinated individuals for the day</param>
            public Record(DateTime dtDate, int iVaccinated, int iDailyVaccinated) {
                Date = dtDate;
                Vaccinated = iVaccinated;
                DailyVaccinated = iDailyVaccinated;
            }

            /// <summary>
            /// Creates and initializes a new record without daily individuals
            /// </summary>
            /// <param name="dtDate">Date of the record</param>
            /// <param name="iVaccinated">Total number of vaccinated individuals</param>
            public Record(DateTime dtDate, int iVaccinated) : this(dtDate, iVaccinated, 0) { }

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

                        // HACK: Sum UK
                        if(!dic.TryGetValue("United Kingdom", out List<Record> lGB))
                            lGB = new List<Record>();
                        if(dic.TryGetValue("Northern Ireland", out List<Record> lNorthernIreland))  
                            foreach(Record r in lNorthernIreland) {
                                int i = lGB.IndexOf(lGB.FirstOrDefault(rr => rr.Date == r.Date));
                                if(i != -1)
                                    lGB[i] = new Record(r.Date, lGB[i].Vaccinated + r.Vaccinated, lGB[i].DailyVaccinated + r.DailyVaccinated);
                                else
                                    lGB.Add(new Record(r.Date, r.Vaccinated, r.DailyVaccinated));
                        }                        
                        if(dic.TryGetValue("Scotland", out List<Record> lScotland))
                            foreach(Record r in lScotland) {
                                int i = lGB.IndexOf(lGB.FirstOrDefault(rr => rr.Date == r.Date));
                                if(i != -1)
                                    lGB[i] = new Record(r.Date, lGB[i].Vaccinated + r.Vaccinated, lGB[i].DailyVaccinated + r.DailyVaccinated);
                                else
                                    lGB.Add(new Record(r.Date, r.Vaccinated, r.DailyVaccinated));
                        }
                        if(dic.TryGetValue("Wales", out List<Record> lWales))
                            foreach(Record r in lWales) {
                                int i = lGB.IndexOf(lGB.FirstOrDefault(rr => rr.Date == r.Date));
                                if(i != -1)
                                    lGB[i] = new Record(r.Date, lGB[i].Vaccinated + r.Vaccinated, lGB[i].DailyVaccinated + r.DailyVaccinated);
                                else
                                    lGB.Add(new Record(r.Date, r.Vaccinated, r.DailyVaccinated));
                        }                            
                        if(dic.TryGetValue("England", out List<Record> lEngland))
                            foreach(Record r in lEngland) {
                                int i = lGB.IndexOf(lGB.FirstOrDefault(rr => rr.Date == r.Date));
                                if(i != -1)
                                    lGB[i] = new Record(r.Date, lGB[i].Vaccinated + r.Vaccinated, lGB[i].DailyVaccinated + r.DailyVaccinated);
                                else
                                    lGB.Add(new Record(r.Date, r.Vaccinated, r.DailyVaccinated));
                        }
                        if(lGB.Count != 0) {
                            lGB.Sort(new LogicLink.Corona.Comparer<Record>((r1, r2) => r1.Date.CompareTo(r2.Date)));
                            dic["United Kingdom"] = lGB;
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
