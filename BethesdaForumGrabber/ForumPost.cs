using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BethesdaForumGrabber
{
    public class ForumPost
    {
        [XmlAttribute]
        public long Id { get; set; }
        [XmlAttribute]
        public string Author { get; set; }
        [XmlAttribute]
        public string Date { get; set; }
        [XmlElement]
        public string Post { get; set; }
    }
}
