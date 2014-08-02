using System;

namespace Lucene.Net.Search.Vectorhighlight
{
	/// <summary>
	/// Defines the interface to an object that provides the source text for use in fragment building.
	/// </summary>
	public interface IFragmentSource : IDisposable
	{
		/// <summary>
		/// Gets a value indicating whether this fragment source is empty.
		/// </summary>
		bool IsEmpty { get; }

		/// <summary>
		/// Gets the specified substring of the source text.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		string GetText(int start, ref int length);
	}
}
