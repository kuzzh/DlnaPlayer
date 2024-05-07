using System;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DlnaLib
{
    public class PlayMediaInfo
    {

        public int NrTracks { get; set; }
        public string MediaDuration { get; set; }
        public string CurrentURI { get; set; }

        //<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/" s:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/"><s:Body>
        //<u:GetMediaInfoResponse xmlns:u="urn:schemas-upnp-org:service:AVTransport:1">
        //<NrTracks>0/1</NrTracks>
        //<MediaDuration>02:28:09</MediaDuration>
        //<CurrentURI>http://192.168.1.248:1573/%2f%2f%e8%9c%98%e8%9b%9b%e4%be%a0%e5%85%a8%e5%ae%b6%e6%a1%b6%2f%e8%9c%98%e8%9b%9b%e4%be%a0%ef%bc%9a%e8%8b%b1%e9%9b%84%e6%97%a0%e5%bd%92.1080p.BD%e4%b8%ad%e8%8b%b1%e5%8f%8c%e5%ad%97%2f1080p.BD%e4%b8%ad%e8%8b%b1%e5%8f%8c%e5%ad%97%5b66%e5%bd%b1%e8%a7%86www.66Ys.Co%5d.mp4</CurrentURI>
        //<CurrentURIMetaData></CurrentURIMetaData>
        //<NextURI></NextURI>
        //<NextURIMetaData></NextURIMetaData>
        //<PlayMedium>NONE</PlayMedium>
        //<RecordMedium>NOT_IMPLEMENTED</RecordMedium>
        //<WriteStatus>NOT_IMPLEMENTED</WriteStatus>
        //</u:GetMediaInfoResponse>
        //</s:Body></s:Envelope>
        public static PlayMediaInfo ParseXml(string xml)
        {
            var mediaInfo = new PlayMediaInfo();
            var xmlDoc = XElement.Parse(xml);

            var node = xmlDoc.XPathSelectElement("//NrTracks");
            if (node != null)
            {
                mediaInfo.NrTracks = Convert.ToInt32(node.Value);
            }
            node = xmlDoc.XPathSelectElement("//MediaDuration");
            if (node != null)
            {
                mediaInfo.MediaDuration = node.Value;
            }
            node = xmlDoc.XPathSelectElement("//CurrentURI");
            if (node != null)
            {
                mediaInfo.CurrentURI = node.Value;
            }
            return mediaInfo;
        }
    }
}
