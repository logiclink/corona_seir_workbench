using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private static Dictionary<string, Dictionary<DateTime, Record>> _dic;   // Memory Cache
        private static readonly SemaphoreSlim _sms = new SemaphoreSlim(1, 1);   // Semaphore blocking multiple LoadAsync calls and GetDataAsync before LoadAsync finishes

        private const string JHU_CONFIRMED_URL = "https://github.com/CSSEGISandData/COVID-19/raw/master/csse_covid_19_data/csse_covid_19_time_series/time_series_covid19_confirmed_global.csv";
        private const string JHU_RECOVERED_URL = "https://github.com/CSSEGISandData/COVID-19/raw/master/csse_covid_19_data/csse_covid_19_time_series/time_series_covid19_recovered_global.csv";
        private const string JHU_DEATHS_URL = "https://github.com/CSSEGISandData/COVID-19/raw/master/csse_covid_19_data/csse_covid_19_time_series/time_series_covid19_deaths_global.csv";

        /// <summary>
        /// Structure for a record of a Day
        /// </summary>
        public struct Record {

            #region Public readonly fields

            public readonly DateTime Date;  // Date of the record
            public readonly int Confirmed;           // Total number of confirmed infected individuals
            public readonly int DailyConfirmed;      // Number of confirmed infected individuals for the day
            public readonly int Recovered;           // Total number of recovered individuals
            public readonly int Deaths;              // Total number of deaths of infected individuals

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

        private List<DateTime> GetDatesFromString(string s) {
            ReadOnlySpan<char> sp = s;
            List<DateTime> l = new List<DateTime>();
            int i = 0;
            int j = 0;

            // Ignore first four columns with Province/State, Country/Region, Lat & Long
            for(int h = 0; h < 4; h++) {
                j = sp[i..].QuotedIndexOf(',');
                i += j + 1;
            }

            // Parse all date columns until the last one
            j = sp[i..].QuotedIndexOf(',');
            while( j != -1) {
                l.Add(DateTime.Parse(sp.Slice(i, j), CultureInfo.InvariantCulture));
                i += j + 1;
                j = sp[i..].QuotedIndexOf(',');
            }

            // Parse last date column
            l.Add(DateTime.Parse(sp[i..], CultureInfo.InvariantCulture));

            return l;
        }

        private (string Country, List<int> Values) GetValuesFromString(string s) {
            ReadOnlySpan<char> sp = s;
            List<int> l = new List<int>();
            int i = 0;

            // Ignore Province/State column
            int j = sp[i..].QuotedIndexOf(',');
            i += j + 1;

            // Get Country/Region
            j = sp[i..].QuotedIndexOf(',');
            string sCountry = new string(sp.Slice(i, j));
            i += j + 1;

            // Ignore Lat & Long column
            for(int h = 0; h < 2; h++) {
                j = sp[i..].QuotedIndexOf(',');
                i += j + 1;
            }

            // Parse all number columns until the last one
            int k = 0;
            j = sp[i..].QuotedIndexOf(',');
            while(j != -1) {
                l.Add(int.Parse(sp.Slice(i, j)));
                i += j + 1;
                j = sp[i..].QuotedIndexOf(',');
            }

            // Parse last number column
            l.Add(int.Parse(sp[i..]));

            return (sCountry, l);
        }

        /// <summary>
        /// Loads the current Johns Hopkins University cornona data in a separate thread
        /// </summary>
        /// <returns>awaitable Task</returns>
        public async Task LoadAsync() 
        {
            if (_dic != null)
            {
                return;
            }

            await _sms.WaitAsync();
            try
            {
                _dic ??= await LoadInternalAsync();
            } 
            finally 
            {
                _sms.Release();
            }
        }

        private async Task<Dictionary<string, Dictionary<DateTime, Record>>> LoadInternalAsync()
        {
            Dictionary<string, Dictionary<DateTime, Record>> dic = new Dictionary<string, Dictionary<DateTime, Record>>();

            // Confirmed data
            await using (FileStream fs = new FileStream(await Download.GetCachedAsync(JHU_CONFIRMED_URL), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader rd = new StreamReader(fs))
            {
                List<DateTime> lDates = GetDatesFromString(await rd.ReadLineAsync());
                string sCountry;
                List<int> lValues;
                while (!rd.EndOfStream)
                {
                    (sCountry, lValues) = GetValuesFromString(await rd.ReadLineAsync());

                    if (lDates.Count != lValues.Count)
                        Debug.WriteLine("ERROR IN DATA !!!");

                    if (!dic.TryGetValue(sCountry, out Dictionary<DateTime, Record> l))
                    {
                        l = new Dictionary<DateTime, Record>();
                        dic[sCountry] = l;
                    }

                    for (int i = 0; i < (int) Math.Min(lDates.Count, lValues.Count); i++)
                    {
                        if (l.TryGetValue(lDates[i], out Record r))
                        {
                            l[lDates[i]] = new Record(lDates[i], r.Confirmed + lValues[i],
                                i != 0 ? r.DailyConfirmed + lValues[i] - lValues[i - 1] : r.DailyConfirmed, r.Recovered,
                                r.Deaths);
                        }
                        else
                        {
                            l[lDates[i]] = new Record(lDates[i], lValues[i], i != 0 ? lValues[i] - lValues[i - 1] : 0, 0, 0);
                        }
                    }
                }
            }

            // Recovered data
            await using (FileStream fs = new FileStream(await Download.GetCachedAsync(JHU_RECOVERED_URL), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader rd = new StreamReader(fs))
            {
                List<DateTime> lDates = GetDatesFromString(await rd.ReadLineAsync());
                string sCountry;
                List<int> lValues;
                while (!rd.EndOfStream)
                {
                    (sCountry, lValues) = GetValuesFromString(await rd.ReadLineAsync());

                    if (lDates.Count != lValues.Count)
                        Debug.WriteLine("ERROR IN DATA !!!");

                    if (!dic.TryGetValue(sCountry, out Dictionary<DateTime, Record> l))
                    {
                        l = new Dictionary<DateTime, Record>();
                        dic[sCountry] = l;
                    }

                    for (int i = 0; i < (int) Math.Min(lDates.Count, lValues.Count); i++)
                    {
                        if (l.TryGetValue(lDates[i], out Record r))
                        {
                            l[lDates[i]] = new Record(lDates[i], r.Confirmed, r.DailyConfirmed,
                                r.Recovered + lValues[i], r.Deaths);
                        }
                        else
                        {
                            l[lDates[i]] = new Record(lDates[i], 0, 0, lValues[i], 0);
                        }
                    }
                }
            }

            // Deaths data
            await using (FileStream fs = new FileStream(await Download.GetCachedAsync(JHU_DEATHS_URL), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader rd = new StreamReader(fs))
            {
                List<DateTime> lDates = GetDatesFromString(await rd.ReadLineAsync());
                string sCountry;
                List<int> lValues;
                while (!rd.EndOfStream)
                {
                    (sCountry, lValues) = GetValuesFromString(await rd.ReadLineAsync());

                    if (lDates.Count != lValues.Count)
                        Debug.WriteLine("ERROR IN DATA !!!");

                    if (!dic.TryGetValue(sCountry, out Dictionary<DateTime, Record> l))
                    {
                        l = new Dictionary<DateTime, Record>();
                        dic[sCountry] = l;
                    }

                    for (int i = 0; i < (int) Math.Min(lDates.Count, lValues.Count); i++)
                    {
                        if (l.TryGetValue(lDates[i], out Record r))
                            l[lDates[i]] = new Record(lDates[i], r.Confirmed, r.DailyConfirmed, r.Recovered,
                                r.Deaths + lValues[i]);
                        else
                            l[lDates[i]] = new Record(lDates[i], 0, 0, 0, lValues[i]);
                    }
                }
            }

            return dic;
        }

        /// <summary>
        /// Returns Johns-Hopkins-University cornona data for a country
        /// </summary>
        /// <param name="sCountry">Name of the Country</param>
        /// <returns>Asynchronous enumerator</returns>
        public async IAsyncEnumerable<Record> GetDataAsync(string sCountry) {
            if(_dic == null)
                await LoadAsync();

            if(!_dic.ContainsKey(sCountry))
                yield break;

            foreach(Record r in _dic[sCountry].Values)
                yield return r;
        }
    }
}
