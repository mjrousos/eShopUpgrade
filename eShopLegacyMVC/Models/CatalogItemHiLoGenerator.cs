using System;
using System.Linq;

namespace eShopLegacyMVC.Models
{
    /// <summary>
    /// Hi-Lo sequence generator for creating unique catalog item identifiers.
    /// Implements the Hi-Lo pattern to provide high-performance unique ID generation
    /// by batching sequence values from the database to minimize round trips.
    /// Thread-safe implementation supports concurrent access across multiple requests.
    /// </summary>
    public class CatalogItemHiLoGenerator
    {
        /// <summary>
        /// Number of consecutive IDs to generate from a single database sequence call.
        /// Higher values improve performance but may leave unused ID gaps.
        /// </summary>
        private const int HiLoIncrement = 10;
        
        /// <summary>
        /// Current high sequence value obtained from the database
        /// </summary>
        private int sequenceId = -1;
        
        /// <summary>
        /// Number of low sequence values remaining before requiring a new database call
        /// </summary>
        private int remainningLoIds = 0;
        
        /// <summary>
        /// Lock object to ensure thread-safe access to sequence generation
        /// </summary>
        private object sequenceLock = new object();

        /// <summary>
        /// Generates the next unique sequence value using the Hi-Lo pattern.
        /// Fetches new sequence batch from database when current batch is exhausted.
        /// Thread-safe operation that minimizes database round trips for better performance.
        /// </summary>
        /// <param name="db">Database context for executing sequence queries</param>
        /// <returns>Next unique identifier for catalog item</returns>
        public int GetNextSequenceValue(CatalogDBContext db)
        {
            lock (sequenceLock)
            {
                if (remainningLoIds == 0)
                {
                    // Fetch new "Hi" value from database sequence
                    var rawQuery = db.Database.SqlQuery<Int64>("SELECT NEXT VALUE FOR catalog_hilo;");
                    sequenceId = (int)rawQuery.Single();
                    remainningLoIds = HiLoIncrement - 1;
                    return sequenceId;
                }
                else
                {
                    // Use next "Lo" value from current batch
                    remainningLoIds--;
                    return ++sequenceId;
                }
            }
        }
    }
}