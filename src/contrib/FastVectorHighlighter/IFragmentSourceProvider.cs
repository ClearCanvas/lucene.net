using Lucene.Net.Index;

namespace Lucene.Net.Search.Vectorhighlight
{
	/// <summary>
	/// Defines the interface to an object that provides <see cref="IFragmentSource"/> objects.
	/// </summary>
	/// <remarks>
	/// The original FastVectorHighlighter implementation assumes that the source field text is stored
	/// in the index, and furthermore loads that text in its entirety into memory without regard for 
	/// the size of the text.
	/// 
	/// The purpose of this interface is to abstract the source of the text used in fragment building,
	/// which provides flexibility in the case where the text is not stored in the index, or when it
	/// is not desirable to load the entire text into memory.
	/// </remarks>
	public interface IFragmentSourceProvider
	{
		/// <summary>
		/// Obtains a <see cref="IFragmentSource"/> for the specified document and field.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="doc"></param>
		/// <param name="field"></param>
		/// <returns></returns>
		IFragmentSource GetFragmentSource(IndexReader reader, int doc, string field);
	}
}
