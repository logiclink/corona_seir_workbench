using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LogicLink.Corona {

	/// <summary>
	/// Extension methods for IAsyncEnumerable<T>
	/// </summary>
	public static class IAsyncEnumerableExtensions {

		/// <summary>
		/// Converts an async enumerable into a list.
		/// </summary>
		/// <typeparam name="T">The type of the objects to iterate.</typeparam>
		/// <param name="source">The source enumerable to iterate.</param>
		/// <param name="cancellationToken">The cancellation token to use.</param>
		/// <returns>List of T</returns>
		public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default) {
			if(source == null) throw new ArgumentNullException(nameof(source));

			List<T> l = new List<T>();
			await using(IAsyncEnumerator<T> e = source.GetAsyncEnumerator(cancellationToken))
				while(await e.MoveNextAsync())
					l.Add(e.Current);

			return l;
		}

		/// <summary>
		/// Converts an async enumerable into a dictionary.
		/// </summary>
		/// <typeparam name="T">The type of the objects to iterate.</typeparam>
		/// <typeparam name="TKey">The type of keys</typeparam>
		/// <param name="source">The source enumerable to iterate.</param>
		/// <param name="keySelector">Function of T which creates a key</param>
		/// <param name="cancellationToken">The cancellation token to use.</param>
		/// <returns>Dictionary of T with TKey keys</returns>
		public static async Task<Dictionary<TKey, T>> ToDictionaryAsync<T, TKey>(this IAsyncEnumerable<T> source, Func<T, TKey> keySelector, CancellationToken cancellationToken = default) {
			if(source == null) throw new ArgumentNullException(nameof(source));

			Dictionary<TKey, T> dic = new Dictionary<TKey, T>();
			await using(IAsyncEnumerator<T> e = source.GetAsyncEnumerator(cancellationToken))
				while(await e.MoveNextAsync())
					dic.Add(keySelector(e.Current), e.Current);

			return dic;
		}
	}
}
