using System;
using System.Text;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace Lucene.Net.Search.Vectorhighlight
{
	/// <summary>
	/// Implementation of <see cref="IFragmentSourceProvider"/> that loads the text from fields
	/// stored in the index.
	/// </summary>
	/// <remarks>
	/// This is the default implementation of <see cref="IFragmentSourceProvider"/>, implemented
	/// identically to the original algorithm in FastVectorHighlighter. It assumes the source
	/// text for the field is stored in the index, and reads the entirety of the text into a
	/// buffer.
	/// </remarks>
	public class StoredFieldFragmentSourceProvider : IFragmentSourceProvider
	{
		class StoredFieldFragmentSource : IFragmentSource
		{
			private readonly Field[] _values;
			private readonly StringBuilder _buffer = new StringBuilder();
			private int _index;

			public StoredFieldFragmentSource(Field[] values)
			{
				_values = values;
				_index = 0;
			}

			public bool IsEmpty
			{
				get { return _values.Length == 0; }
			}

			public string GetText(int start, ref int length)
			{
				var endOffset = start + length;
				while (_buffer.Length < endOffset && _index < _values.Length)
				{
					_buffer.Append(_values[_index].StringValue);
					if (_values[_index].IsTokenized && _values[_index].StringValue.Length > 0 && _index + 1 < _values.Length)
						_buffer.Append(' ');
					_index++;
				}
				var s = _buffer.ToString().Substring(start, Math.Min(length, _buffer.Length - start));
				length = s.Length;
				return s;
			}

			public void Dispose()
			{
				
			}
		}

		public IFragmentSource GetFragmentSource(IndexReader reader, int doc, string field)
		{
			return new StoredFieldFragmentSource(GetFields(reader, doc, field));
		}

		private static Field[] GetFields(IndexReader reader, int docId, String fieldName)
		{
			// according to javadoc, doc.getFields(fieldName) cannot be used with lazy loaded field???
			var doc = reader.Document(docId, new MapFieldSelector(new[] { fieldName }));
			return doc.GetFields(fieldName); // according to Document class javadoc, this never returns null
		}
	}
}
