using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BethesdaForumGrabber
{
    [XmlRoot]
    class Config
    {
        [XmlArray]
        public List<ForumBoard> Boards { get; set; }

        public Config()
        {
            Boards = new List<ForumBoard>();
        }
    }
}
