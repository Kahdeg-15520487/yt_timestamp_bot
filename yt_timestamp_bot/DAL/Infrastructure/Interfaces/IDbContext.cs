using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace discordbot.DAL.Infrastructure.Interfaces
{
    public interface TValue
    {
        string Key { get; }
        object Value { get; }
    }
    public interface IDbContext : IDisposable
    {
        /// <summary>
        /// Get all values from the database.
        /// </summary>
        /// <returns>list of all values.</returns>
        Task<IEnumerable<T>> GetAll<T>() where T : TValue;

        /// <summary>
        /// Get a value based on supplied key.
        /// </summary>
        /// <param name="key">key of value.</param>
        /// <returns>value.</returns>
        Task<T> Get<T>(string key) where T : TValue;

        /// <summary>
        /// Get many value based on supplied list of keys.
        /// </summary>
        /// <param name="keys">list of keys.</param>
        /// <returns>values.</returns>
        Task<IEnumerable<T>> Gets<T>(IEnumerable<string> keys) where T : TValue;

        /// <summary>
        /// Set a key-value into the database.
        /// </summary>
        /// <param name="value">value to be set.</param>
        /// <returns>whether the operation is successful or not.</returns>
        Task<bool> Set<T>(TValue value) where T : TValue;

        /// <summary>
        /// Set a list of key-value into the databse.
        /// </summary>
        /// <param name="values">list of values to be set.</param>
        /// <returns>return count of successful operation.</returns>
        Task<int> Sets<T>(IEnumerable<TValue> values) where T : TValue;

        /// <summary>
        /// Delete a key from the database.
        /// </summary>
        /// <param name="value">value to be deleted.</param>
        /// <returns>whether the operation is successful or not</returns>
        Task<bool> Delete(TValue document);
    }
}
