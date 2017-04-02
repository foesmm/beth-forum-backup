using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BethesdaForumGrabber
{
    public class ForumBoard
    {
        [XmlAttribute]
        public string Alias { get; set; }
        [XmlAttribute]
        public string Title { get; set; }
        [XmlAttribute]
        public string URL { get; set; }
        [XmlAttribute]
        public int PageCompleted { get; set; }
    }
}
