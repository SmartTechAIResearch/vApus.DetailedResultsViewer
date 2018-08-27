/*
 * 2014 Sizing Servers Lab, affiliated with IT bachelor degree NMCT
 * University College of West-Flanders, Department GKG (www.sizingservers.be, www.nmct.be, www.howest.be/en)
 * 
 * Author(s):
 *    Dieter Vandroemme
 */
using SizingServers.Util;
using SizingServers.Log;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using vApus.Util;

namespace vApus.Results {
    internal abstract class BaseResultSetCalculator {
        public abstract DataTable Get(DatabaseActions databaseActions, CancellationToken cancellationToken, FunctionOutputCache functionOutputCache, params int[] stressTestIds);
        protected DataTable CreateEmptyDataTable(string name, params string[] columnNames) {
            var objectType = typeof(object);
            var dataTable = new DataTable(name);
            foreach (string columnName in columnNames) dataTable.Columns.Add(columnName, objectType);
            return dataTable;
        }

        /// <summary>
        /// All tables needed must be gattered here.
        /// </summary>
        /// <param name="databaseActions"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="stressTestIds"></param>
        /// <returns>Label and data table</returns>
        protected abstract ConcurrentDictionary<string, DataTable> GetData(DatabaseActions databaseActions, CancellationToken cancellationToken, FunctionOutputCache functionOutputCache, params int[] stressTestIds);
        /// <summary>
        /// Get data tables per run. It is possible that one data table has requests with different run result ids, because of combining homogeneous results. Take this into account!
        /// </summary>
        /// <param name="databaseActions"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="runResults"></param>
        /// <param name="threads">The number of threads that should be used to query the database.</param>
        /// <param name="columns"></param>
        /// <returns></returns>
        protected DataTable[] GetRequestResultsPerRunThreaded(DatabaseActions databaseActions, CancellationToken cancellationToken, FunctionOutputCache functionOutputCache, DataTable runResults, params string[] columns) {
            var cacheEntry = functionOutputCache.GetOrAdd(MethodInfo.GetCurrentMethod(), columns);
            var cacheEntryDt = cacheEntry.ReturnValue as DataTable[];
            if (cacheEntryDt != null) return cacheEntryDt;

            int runCount = runResults.Rows.Count;

            //Adaptive parallelization, trying not to cripple the machine.
            int threads = 8;

            if (threads > Environment.ProcessorCount - 1) threads = Environment.ProcessorCount - 1;
            if (threads > runCount) threads = runCount;
            if (threads < 1) threads = 1;

            int partRange = runCount / threads;
            int remainder = runCount % threads;

            int[][] runResultIds = new int[threads][];

            int inclLower = 0;
            for (int thread = 0; thread != threads; thread++) {
                int exclUpper = inclLower + partRange;
                if (remainder != 0) {
                    ++exclUpper;
                    --remainder;
                }

                runResultIds[thread] = new int[exclUpper - inclLower];
                for (int i = inclLower; i != exclUpper; i++)
                    runResultIds[thread][i - inclLower] = (int)runResults.Rows[i][0];

                inclLower = exclUpper;
            }

            cacheEntryDt = new DataTable[runResultIds.Length];
            Parallel.For(0, runResultIds.Length, (i, loopState) => {
                // for (int i = 0; i != runResultIds.Length; i++)
                using (var dba = new DatabaseActions() { ConnectionString = databaseActions.ConnectionString, CommandTimeout = 600 }) {
                    if (cancellationToken.IsCancellationRequested) loopState.Break();
                    try {
                        cacheEntryDt[i] = ReaderAndCombiner.GetRequestResults(cancellationToken, dba, runResultIds[i], columns);
                    }
                    catch (Exception ex) {
                        Loggers.Log(Level.Error, "Failed at getting a run result part.", ex, new object[] { threads, columns });
                    }
                }
            });

            cacheEntry.ReturnValue = cacheEntryDt;

            return cacheEntryDt;
        }
    }
}
