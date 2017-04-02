using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using CsQuery;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Threading;
using System.Xml;

namespace BethesdaForumGrabber
{
    class Program
    {
        static string BOARD_URL_FORMAT = "{0}page-{1}?prune_day=100&sort_by=A-Z&sort_key=start_date&topicfilter=all";
        static string TOPIC_URL_FORMAT = "{0}page-{1}";
        static string PAGE_PATTERN = @".+(\d+)[^\d]+(\d+).*";

        static void Main(string[] args)
        {
            Config config = null;

            if (config == null)
            {
                config = new Config();
                config.Boards.Add(new ForumBoard { Alias = "Fo3_GECK", Title = "The G.E.C.K. (Fallout 3)", URL = "http://forums.bethsoft.com/forum/46-the-geck-fallout-3/" });
                config.Boards.Add(new ForumBoard { Alias = "NV_GECK", Title = "The G.E.C.K. (Fallout: New Vegas)", URL = "http://forums.bethsoft.com/forum/111-the-geck-fallout-new-vegas/" });
                config.Boards.Add(new ForumBoard { Alias = "Fo3_Mods", Title = "Fallout 3 Mods", URL = "http://forums.bethsoft.com/forum/45-fallout-3-mods/" });
                config.Boards.Add(new ForumBoard { Alias = "NV_Mods", Title = "Fallout: New Vegas Mods", URL = "http://forums.bethsoft.com/forum/110-fallout-new-vegas-mods/" });
                config.Boards.Add(new ForumBoard { Alias = "MW_CS", Title = "Construction Set (Morrowind)", URL = "http://forums.bethsoft.com/forum/11-construction-set-morrowind/" });
                config.Boards.Add(new ForumBoard { Alias = "OB_CS", Title = "Construction Set (Oblivion)", URL = "http://forums.bethsoft.com/forum/24-construction-set-oblivion/" });
                config.Boards.Add(new ForumBoard { Alias = "SK_CK", Title = "The Creation Kit (Skyrim)", URL = "http://forums.bethsoft.com/forum/184-the-creation-kit/" });
            }

            Parallel.ForEach(config.Boards, board => FetchBoard(board));
            //foreach (var board in config.Boards)
            //{
            //    FetchBoard(board);
            //}
        }

        static void FetchBoard(ForumBoard board)
        {
            var outDir = Path.GetFullPath(board.Alias);
            Directory.CreateDirectory(outDir);

            var i = 1;
            var totalPages = 1;

            var file = Path.Combine(outDir, board.Alias + ".xml");

            if (File.Exists(file)) {
                var xml = new XmlSerializer(typeof(ForumBoard));
                var fs = new FileStream(file, FileMode.Open);
                var reader = XmlReader.Create(fs);
                var sBoard = (ForumBoard)xml.Deserialize(reader);
                i = sBoard.PageCompleted;
                totalPages = sBoard.PageCompleted;
                reader.Close();
                fs.Close();
            }

            var client = new CookiesAwareWebClient();
            var referer = "";

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineHandling = NewLineHandling.Entitize;
            settings.CloseOutput = true;

            do
            {
                client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");
                client.Headers.Add(HttpRequestHeader.Referer, referer);
                var url = String.Format(BOARD_URL_FORMAT, board.URL, i);

                Console.WriteLine(url);
                CQ boardContents = ((CQ)client.DownloadString(url)).Render(DomRenderingOptions.RemoveComments);
                referer = url;

                var pager = boardContents.Find("li.pagejump > a").First().Text();
                var pages = Regex.Match(pager, PAGE_PATTERN);
                if (pages.Success)
                {
                    totalPages = int.Parse(pages.Groups[2].ToString());
                } else
                {
                    totalPages = 1;
                }

                var topics = boardContents.Find("tr.__topic");
                //foreach (var topic in topics)
                Parallel.ForEach(topics, topic => 
                {
                    var e = topic.Cq();
                    var forumTopic = new ForumTopic();
                    forumTopic.Board = board.Title;
                    forumTopic.Id = long.Parse(topic.Attributes["data-tid"]);
                    if (File.Exists(Path.Combine(outDir, forumTopic.Id.ToString() + ".xml")))
                    {
                        return;
                    }
                    forumTopic.Title = e.Find("span[itemprop=name]").Text();
                    forumTopic.Date = e.Find("span[itemprop=dateCreated]").Text();
                    forumTopic.URL = e.Find("a[itemprop=url]").Attr("href");
                    forumTopic.Posts = FetchTopic(client, url, forumTopic.URL);

                    using (var writer = XmlWriter.Create(new StreamWriter(Path.Combine(outDir, forumTopic.Id.ToString() + ".xml")), settings))
                    {
                        var serializer = new XmlSerializer(typeof(ForumTopic));
                        serializer.Serialize(writer, forumTopic);
                        writer.Flush();
                        writer.Close();
                    }
                }//;
                );

                board.PageCompleted = i;
                using (var writer = XmlWriter.Create(new StreamWriter(Path.Combine(outDir, board.Alias + ".xml")), settings))
                {
                    var serializer = new XmlSerializer(typeof(ForumBoard));
                    serializer.Serialize(writer, board);
                    writer.Flush();
                    writer.Close();
                }
                i++;
            } while (i <= totalPages);
        }

        static List<ForumPost> FetchTopic(CookiesAwareWebClient client, string referer, string url)
        {
            var result = new List<ForumPost>();

            var i = 1;
            var totalPages = 1;

            client = client.Copy();

            do
            {
                client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");
                try
                {
                    client.Headers.Add(HttpRequestHeader.Referer, referer);
                }
                catch (Exception e)
                {

                }

                var topicUrl = String.Format(TOPIC_URL_FORMAT, url, i);
                referer = topicUrl;
                Console.WriteLine(topicUrl);
                CQ topicContents = ((CQ)client.DownloadString(topicUrl)).Render(DomRenderingOptions.RemoveComments);

                var pager = topicContents.Find("li.pagejump > a").First().Text();
                var pages = Regex.Match(pager, PAGE_PATTERN);
                if (pages.Success)
                {
                    totalPages = int.Parse(pages.Groups[2].ToString());
                } else
                {
                    totalPages = 1;
                }

                var posts = topicContents.Find("div.post_block");
                foreach (var post in posts)
                {
                    var e = post.Cq();
                    var forumPost = new ForumPost();
                    forumPost.Id = long.Parse(Regex.Replace(post.Attributes["id"].ToString(), @"[^\d]", ""));
                    forumPost.Author = ReplaceHexadecimalSymbols(e.Find("span[itemprop=creator name]").Text().Trim());
                    forumPost.Date = e.Find("abbr[itemprop=commentTime]").Attr("title").ToString();
                    forumPost.Post = ReplaceHexadecimalSymbols(e.Find("div[itemprop=commentText]").Html().Trim());
                    result.Add(forumPost);
                }

                i++;
            } while (i <= totalPages);

            return result;
        }

        static string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(txt, r, "", RegexOptions.Compiled);
        }
    }
}
