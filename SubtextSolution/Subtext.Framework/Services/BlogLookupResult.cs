﻿#region Disclaimer/Info
///////////////////////////////////////////////////////////////////////////////////////////////////
// Subtext WebLog
// 
// Subtext is an open source weblog system that is a fork of the .TEXT
// weblog system.
//
// For updated news and information please visit http://subtextproject.com/
// Subtext is hosted at SourceForge at http://sourceforge.net/projects/subtext
// The development mailing list is at subtext-devs@lists.sourceforge.net 
//
// This project is licensed under the BSD license.  See the License.txt file for more information.
///////////////////////////////////////////////////////////////////////////////////////////////////
#endregion

using System;

namespace Subtext.Framework.Services
{
    public class BlogLookupResult
    {
        public BlogLookupResult(Blog blog, Uri alternateUrl) {
            Blog = blog;
            AlternateUrl = alternateUrl;
        } 

        /// <summary>
        /// The found blog. Null if not found.
        /// </summary>
        public Blog Blog {
            get;
            private set;
        }

        /// <summary>
        /// A blog was found using an alternate host. Try this one instead.
        /// </summary>
        public Uri AlternateUrl {
            get;
            private set;
        }
    }
}
