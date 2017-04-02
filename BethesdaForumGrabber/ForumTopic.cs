using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BethesdaForumGrabber
{
    public class ForumTopic
    {
        [XmlAttribute]
        public long Id { get; set; }
        [XmlAttribute]
        public string Date { get; set; }
        [XmlAttribute]
        public string Title { get; set; }
        [XmlAttribute]
        public string URL { get; set; }
        [XmlAttribute]
        public string Board { get; set; }
        [XmlArray]
        public List<ForumPost> Posts { get; set; }
    }
}
