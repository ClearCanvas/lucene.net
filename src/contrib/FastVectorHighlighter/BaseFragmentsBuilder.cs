/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Index;

using WeightedFragInfo = Lucene.Net.Search.Vectorhighlight.FieldFragList.WeightedFragInfo;
using SubInfo = Lucene.Net.Search.Vectorhighlight.FieldFragList.WeightedFragInfo.SubInfo;
using Toffs = Lucene.Net.Search.Vectorhighlight.FieldPhraseList.WeightedPhraseInfo.Toffs;

namespace Lucene.Net.Search.Vectorhighlight
{
    public abstract class BaseFragmentsBuilder : FragmentsBuilder
    {
		class Fragment
		{
			public Fragment(WeightedFragInfo fragInfo)
			{
				FragInfo = fragInfo;
			}

			public readonly WeightedFragInfo FragInfo;
			public string Text;
		}

		protected String[] preTags, postTags;
        public static String[] COLORED_PRE_TAGS = {
            "<b style=\"background:yellow\">", "<b style=\"background:lawngreen\">", "<b style=\"background:aquamarine\">",
            "<b style=\"background:magenta\">", "<b style=\"background:palegreen\">", "<b style=\"background:coral\">",
            "<b style=\"background:wheat\">", "<b style=\"background:khaki\">", "<b style=\"background:lime\">",
            "<b style=\"background:deepskyblue\">", "<b style=\"background:deeppink\">", "<b style=\"background:salmon\">",
            "<b style=\"background:peachpuff\">", "<b style=\"background:violet\">", "<b style=\"background:mediumpurple\">",
            "<b style=\"background:palegoldenrod\">", "<b style=\"background:darkkhaki\">", "<b style=\"background:springgreen\">",
            "<b style=\"background:turquoise\">", "<b style=\"background:powderblue\">"
        };

        public static String[] COLORED_POST_TAGS = { "</b>" };

    	private IFragmentSourceProvider _fragmentSourceProvider = new StoredFieldFragmentSourceProvider();

        protected BaseFragmentsBuilder()
            : this(new String[] { "<b>" }, new String[] { "</b>" })
        {

        }

        protected BaseFragmentsBuilder(String[] preTags, String[] postTags)
        {
            this.preTags = preTags;
            this.postTags = postTags;
        }

        static Object CheckTagsArgument(Object tags)
        {
            if (tags is String) return tags;
            else if (tags is String[]) return tags;
            throw new ArgumentException("type of preTags/postTags must be a String or String[]");
        }

    	public IFragmentSourceProvider FragmentSourceProvider
    	{
			get { return _fragmentSourceProvider; }
			set { _fragmentSourceProvider = value ?? new StoredFieldFragmentSourceProvider(); }
    	}

        public abstract List<WeightedFragInfo> GetWeightedFragInfoList(List<WeightedFragInfo> src);

        public virtual String CreateFragment(IndexReader reader, int docId, String fieldName, FieldFragList fieldFragList)
        {
            String[] fragments = CreateFragments(reader, docId, fieldName, fieldFragList, 1);
            if (fragments == null || fragments.Length == 0) return null;
            return fragments[0];
        }

        public virtual String[] CreateFragments(IndexReader reader, int docId, String fieldName, FieldFragList fieldFragList, int maxNumFragments)
        {
            if (maxNumFragments < 0)
                throw new ArgumentException("maxNumFragments(" + maxNumFragments + ") must be positive number.");

        	using(var fragmentSource = _fragmentSourceProvider.GetFragmentSource(reader, docId, fieldName))
        	{
				if (fragmentSource.IsEmpty)
					return null;

				// determine the set of fragments to be created
				var fragments = GetWeightedFragInfoList(fieldFragList.fragInfos)
					.Select(fragInfo => new Fragment(fragInfo))
					.Take(maxNumFragments)
					.ToArray();

				// process the fragments in order of StartOffset (as opposed to weight),
				// so that the fragmentSource implementation can be forward-only
				foreach (var fragment in fragments.OrderBy(f => f.FragInfo.StartOffset))
				{
					fragment.Text = MakeFragment(fragmentSource, fragment.FragInfo);
				}

				return fragments.Select(f => f.Text).ToArray();
			}
        }

		protected virtual String MakeFragment(IFragmentSource fragmentSource, WeightedFragInfo fragInfo)
		{
			var s = fragInfo.startOffset;
			return MakeFragment(fragInfo, GetFragmentSource(fragmentSource, s, fragInfo.endOffset), s);
		}

		protected virtual String MakeFragment(WeightedFragInfo fragInfo, String src, int s)
        {
            StringBuilder fragment = new StringBuilder();
            int srcIndex = 0;
            foreach (SubInfo subInfo in fragInfo.subInfos)
            {
                foreach (Toffs to in subInfo.termsOffsets)
                {
                    fragment.Append(src.Substring(srcIndex, to.startOffset - s - srcIndex)).Append(GetPreTag(subInfo.seqnum))
                      .Append(src.Substring(to.startOffset - s, to.endOffset - s - (to.startOffset - s))).Append(GetPostTag(subInfo.seqnum));
                    srcIndex = to.endOffset - s;
                }
            }
            fragment.Append(src.Substring(srcIndex));
            return fragment.ToString();
        }

		protected virtual String GetFragmentSource(IFragmentSource fragmentSource, int startOffset, int endOffset)
		{
			var len = endOffset - startOffset;
			return fragmentSource.GetText(startOffset, ref len);
		}

        protected virtual String GetPreTag(int num)
        {
            int n = num % preTags.Length;
            return preTags[n];
        }

        protected virtual String GetPostTag(int num)
        {
            int n = num % postTags.Length;
            return postTags[n];
        }
    }
}
