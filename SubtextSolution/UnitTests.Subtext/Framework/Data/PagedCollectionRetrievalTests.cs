using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using MbUnit.Framework;
using Microsoft.ApplicationBlocks.Data;
using Subtext.Extensibility;
using Subtext.Framework;
using Subtext.Framework.Components;
using Subtext.Framework.Configuration;
using Subtext.Framework.Logging;
using Subtext.Framework.Util;
using UnitTests.Subtext;
using UnitTests.Subtext.Framework.Data;

namespace UnitTests.Subtext.Framework.Data
{
	[TestFixture]
	public class PagedCollectionRetrievalTests
	{
		string hostName;
		
		/// <summary>
		/// Creates some entries and makes sure that the proper 
		/// number of pages and entries per page are created 
		/// for various page sizes.
		/// </summary>
		[RowTest]
		[Row(11, 10, 2, 1)]
		[Row(11, 5, 3, 1)]
		[Row(12, 5, 3, 2)]
		[Row(10, 5, 2, 5)]
		[Row(10, 20, 1, 10)]
		[RollBack]
		public void GetPagedEntriesHandlesPagingProperly(int total, int pageSize, int expectedPageCount, int itemsCountOnLastPage)
		{
			Assert.IsTrue(Config.CreateBlog("", "username", "password", this.hostName, "blog"));
			IPagedCollectionTester[] collectionTesters = {
			                           	new PagedEntryCollectionTester()
										, new PagedEntryByCategoryCollectionTester()
										, new FeedbackCollectionTester()
			                           };

			foreach (IPagedCollectionTester factory in collectionTesters)
			{
				AssertPagedCollection(factory, expectedPageCount, itemsCountOnLastPage, pageSize, total);
			}
		}

		[RowTest]
		[Row(11, 10, 2, 1)]
		[Row(11, 5, 3, 1)]
		[Row(12, 5, 3, 2)]
		[Row(10, 5, 2, 5)]
		[Row(10, 20, 1, 10)]
		[RollBack]
		public void GetPagedLinksHandlesPagingProperly(int total, int pageSize, int expectedPageCount, int itemsCountOnLastPage)
		{
			Assert.IsTrue(Config.CreateBlog("", "username", "password", this.hostName, "blog"));
			IPagedCollectionTester tester = new LinkCollectionTester();
			AssertPagedCollection(tester, expectedPageCount, itemsCountOnLastPage, pageSize, total);
		}

		[RowTest]
		[Row(11, 10, 2, 1)]
		[Row(11, 5, 3, 1)]
		[Row(12, 5, 3, 2)]
		[Row(10, 5, 2, 5)]
		[Row(10, 20, 1, 10)]
		[RollBack]
		public void GetPagedLogEntriesHandlesPagingProperly(int total, int pageSize, int expectedPageCount, int itemsCountOnLastPage)
		{
			Assert.IsTrue(Config.CreateBlog("", "username", "password", this.hostName, "blog"));
			IPagedCollectionTester tester = new LogEntryCollectionTester();
			AssertPagedCollection(tester, expectedPageCount, itemsCountOnLastPage, pageSize, total);
		}

		[RowTest]
		[Row(11, 10, 2, 1)]
		[Row(11, 5, 3, 1)]
		[Row(12, 5, 3, 2)]
		[Row(10, 5, 2, 5)]
		[Row(10, 20, 1, 10)]
		[RollBack]
		public void GetPagedKeywordsHandlesPagingProperly(int total, int pageSize, int expectedPageCount, int itemsCountOnLastPage)
		{
			Assert.IsTrue(Config.CreateBlog("", "username", "password", this.hostName, "blog"));
			IPagedCollectionTester tester = new KeyWordCollectionTester();
			AssertPagedCollection(tester, expectedPageCount, itemsCountOnLastPage, pageSize, total);
		}

		[RowTest]
		[Row(11, 10, 2, 1)]
		[Row(11, 5, 3, 1)]
		[Row(12, 5, 3, 2)]
		[Row(10, 5, 2, 5)]
		[Row(10, 20, 1, 10)]
		[RollBack]
		public void GetPagedBlogsHandlesPagingProperly(int total, int pageSize, int expectedPageCount, int itemsCountOnLastPage)
		{
			IPagedCollectionTester tester = new BlogCollectionTester();
			AssertPagedCollection(tester, expectedPageCount, itemsCountOnLastPage, pageSize, total);
		}

		[RowTest]
		[Row(11, 10, 2, 1)]
		[Row(11, 5, 3, 1)]
		[Row(12, 5, 3, 2)]
		[Row(10, 5, 2, 5)]
		[Row(10, 20, 1, 10)]
		[RollBack]
		[Ignore("This test fails when run within a Transaction via the RollBack attribute, but succeeds without it.")]
		public void GetPagedReferralsHandlesPagingProperly(int total, int pageSize, int expectedPageCount, int itemsCountOnLastPage)
		{
			Assert.IsTrue(Config.CreateBlog("", "username", "password", this.hostName, "blog"));
			IPagedCollectionTester tester = new ReferralsCollectionTester();
			AssertPagedCollection(tester, expectedPageCount, itemsCountOnLastPage, pageSize, total);
		}

		private static void AssertPagedCollection(IPagedCollectionTester pagedCollectionTester, int expectedPageCount, int itemsCountOnLastPage, int pageSize, int total)
		{
			//Create entries
			for(int i = 0; i < total; i++)
			{
				pagedCollectionTester.Create(i);
			}

			int pageCount = 0;
			int totalSeen = 0;
			for (int pageIndex = 0; pageIndex < expectedPageCount; pageIndex++)
			{
				IPagedCollection items = pagedCollectionTester.GetPagedItems(pageIndex, pageSize);
				Assert.AreEqual(total, items.MaxItems, "The paged collection got the max items wrong)");

				if (pageIndex < expectedPageCount - 1)
				{
					//Expect to see pageSize number of entries.
					Assert.AreEqual(pageSize, pagedCollectionTester.GetCount(items), "The page at index " + pageIndex + "Did not have the correct number of records.");
				}
				else
				{
					Assert.AreEqual(itemsCountOnLastPage, pagedCollectionTester.GetCount(items), "The last page did not have the correct number of records.");					
				}
				totalSeen += pagedCollectionTester.GetCount(items);

				pageCount++;
			}
			
			Assert.AreEqual(expectedPageCount, pageCount, "We did not see the expected number of pages.");
			Assert.AreEqual(total, totalSeen, "We did not see the expected number of records.");
		}

		[SetUp]
		public void SetUp()
		{
			this.hostName = UnitTestHelper.GenerateRandomString();
			UnitTestHelper.SetHttpContextWithBlogRequest(this.hostName, "blog");
			CommentFilter.ClearCommentCache();	
		}

		[TearDown]
		public void TearDown()
		{
			Config.ConfigurationProvider = null;
		}
	}
	
	internal interface IPagedCollectionTester
	{
		void Create(int index);
		IPagedCollection GetPagedItems(int pageIndex, int pageSize);
		int GetCount(IPagedCollection collection);
	}
	
	internal class PagedEntryCollectionTester : IPagedCollectionTester
	{
		public void Create(int index)
		{
			Entries.Create(UnitTestHelper.CreateEntryInstanceForSyndication("Phil", "Title" + index, "Who rocks the party that rocks the party?"));
		}

		public IPagedCollection GetPagedItems(int pageIndex, int pageSize)
		{
			return Entries.GetPagedEntries(PostType.BlogPost, -1, pageIndex, pageSize);
		}
		
		public int GetCount(IPagedCollection collection)
		{
			return ((IPagedCollection<Entry>)collection).Count;
		}		
	}

	internal class PagedEntryByCategoryCollectionTester : IPagedCollectionTester
	{
		int categoryId;
		
		public PagedEntryByCategoryCollectionTester()
		{
			LinkCategory category = new LinkCategory();
			category.BlogId = Config.CurrentBlog.Id;
			category.IsActive = true;
			category.Title = "Foobar";
			category.Description = "Unit Test";
			this.categoryId = Links.CreateLinkCategory(category);
		}
		
		public void Create(int index)
		{
			Entry entry = UnitTestHelper.CreateEntryInstanceForSyndication("Phil", "Title" + index, "Who rocks the party that rocks the party?");
			Entries.Create(entry, this.categoryId);
		}

		public IPagedCollection GetPagedItems(int pageIndex, int pageSize)
		{
			return Entries.GetPagedEntries(PostType.BlogPost, this.categoryId, pageIndex, pageSize);
		}

		public int GetCount(IPagedCollection collection)
		{
			return ((IPagedCollection<Entry>)collection).Count;
		}
	}
	
	internal class FeedbackCollectionTester : IPagedCollectionTester
	{
		public void Create(int index)
		{
			Entry comment = UnitTestHelper.CreateEntryInstanceForSyndication("Phil", "Title" + index, "Who rocks the party that rocks the party? " + index);
			comment.PostType = PostType.Comment;
			comment.AlternativeTitleUrl = "blah";
			Entries.CreateComment(comment);
		}

		public IPagedCollection GetPagedItems(int pageIndex, int pageSize)
		{
			return Entries.GetPagedFeedback(pageIndex, pageSize, PostConfig.IsActive);
		}

		public int GetCount(IPagedCollection collection)
		{
			return ((IPagedCollection<Entry>)collection).Count;
		}	
	}

	internal class LogEntryCollectionTester : IPagedCollectionTester
	{
		public void Create(int index)
		{
			SqlParameter[] parameters = {
			                            	new SqlParameter("@BlogId", Config.CurrentBlog.Id)
											, new SqlParameter("@Date", DateTime.Now)
											, new SqlParameter("@Thread", "SomeThread")
											, new SqlParameter("@Context", "SomeContext")
											, new SqlParameter("@Level", "unit test")
											, new SqlParameter("@Logger", "UnitTestLogger")
											, new SqlParameter("@Message", "This test was brought to you by the letter 'Q'.")
											, new SqlParameter("@Exception", "")
											, new SqlParameter("@Url", "http://localhost/")
			                            };
			SqlHelper.ExecuteNonQuery(ConfigurationManager.ConnectionStrings["subtextData"].ConnectionString, CommandType.StoredProcedure, "subtext_AddLogEntry", parameters);
			
		}

		public IPagedCollection GetPagedItems(int pageIndex, int pageSize)
		{
			return LoggingProvider.Instance().GetPagedLogEntries(pageIndex, pageSize);
		}

		public int GetCount(IPagedCollection collection)
		{
			return ((IPagedCollection<LogEntry>)collection).Count;
		}
	}

	internal class LinkCollectionTester : IPagedCollectionTester
	{
		int categoryId;

		public LinkCollectionTester()
		{
			LinkCategory category = new LinkCategory();
			category.BlogId = Config.CurrentBlog.Id;
			category.IsActive = true;
			category.Title = "Foobar";
			category.Description = "Unit Test";
			this.categoryId = Links.CreateLinkCategory(category);
			
			//Create a couple links that should be ignored because postId is not null.
			Entry entry = UnitTestHelper.CreateEntryInstanceForSyndication("Phil", "title", "in great shape");
			int entryId = Entries.Create(entry);
			UnitTestHelper.CreateLinkInDb(this.categoryId, "A Forgettable Link", entryId);
			UnitTestHelper.CreateLinkInDb(this.categoryId, "Another Forgettable Link", entryId);
			UnitTestHelper.CreateLinkInDb(this.categoryId, "Another Forgettable Link", entryId);
		}

		public void Create(int index)
		{
			UnitTestHelper.CreateLinkInDb(this.categoryId, "A Link To Remember Part " + index, null);
		}

		public IPagedCollection GetPagedItems(int pageIndex, int pageSize)
		{
			return Links.GetPagedLinks(categoryId, pageIndex, pageSize, true);
		}

		public int GetCount(IPagedCollection collection)
		{
			return ((IPagedCollection<Link>)collection).Count;
		}
	}

	
	internal class KeyWordCollectionTester : IPagedCollectionTester
	{
		public KeyWordCollectionTester()
		{
		}

		public void Create(int index)
		{
			KeyWord keyword = new KeyWord();
			keyword.BlogId = Config.CurrentBlog.Id;
			keyword.Text = "The Keyword" + index;
			keyword.Title = "Blah";
			keyword.Word = "The Word " + index;
			keyword.Url = "http://localhost/";
			KeyWords.CreateKeyWord(keyword);
		}

		public IPagedCollection GetPagedItems(int pageIndex, int pageSize)
		{
			return KeyWords.GetPagedKeyWords(pageIndex, pageSize);
		}

		public int GetCount(IPagedCollection collection)
		{
			return ((IPagedCollection<KeyWord>)collection).Count;
		}
	}

	internal class BlogCollectionTester : IPagedCollectionTester
	{
		string host = UnitTestHelper.GenerateRandomString();
		
		public BlogCollectionTester()
		{
		}

		public void Create(int index)
		{
			Config.CreateBlog("title " + index, "phil", "password", host, "Subfolder" + index);
		}

		public IPagedCollection GetPagedItems(int pageIndex, int pageSize)
		{
			return BlogInfo.GetBlogsByHost(this.host, pageIndex, pageSize, ConfigurationFlag.IsActive);
		}

		public int GetCount(IPagedCollection collection)
		{
			return ((IPagedCollection<BlogInfo>)collection).Count;
		}
	}

	internal class ReferralsCollectionTester : IPagedCollectionTester
	{
		int entryId;
		public ReferralsCollectionTester()
		{
			Config.Settings.Tracking.QueueStatsCount = 0;
			entryId = Entries.Create(UnitTestHelper.CreateEntryInstanceForSyndication("Phil", "title", "body"));
		}

		public void Create(int index)
		{
			EntryView view = new EntryView();
			view.EntryID = entryId;
			view.BlogId = Config.CurrentBlog.Id;
			view.PageViewType = PageViewType.WebView;
			view.ReferralUrl = string.Format("http://localhost:{0}/{1}/", index, UnitTestHelper.GenerateRandomString());
			Stats.AddQuedStats(view);
			Stats.ClearQueue(true);
			Thread.Sleep(100); //There's no way to fully wait for the worker processes.
		}

		public IPagedCollection GetPagedItems(int pageIndex, int pageSize)
		{
			return Stats.GetPagedReferrers(pageIndex, pageSize);
		}

		public int GetCount(IPagedCollection collection)
		{
			return ((IPagedCollection<Referrer>)collection).Count;
		}
	}
}