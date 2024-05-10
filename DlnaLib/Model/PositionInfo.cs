using System.Xml.Linq;
using System;
using System.Xml.XPath;

namespace DlnaLib.Model
{
    //<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/" s:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/"><s:Body>
    //<u:GetPositionInfoResponse xmlns:u="urn:schemas-upnp-org:service:AVTransport:1">
    //<Track>1</Track>
    //<TrackDuration>00:51:38</TrackDuration>
    //<TrackMetaData></TrackMetaData>
    //<TrackURI>http://192.168.1.248:1573/%2f%2f%e5%be%ae%e6%9a%97%e4%b9%8b%e7%81%ab01-03%2f01.1080p.HD%e5%9b%bd%e8%af%ad%e4%b8%ad%e5%ad%97%e6%97%a0%e6%b0%b4%e5%8d%b0%5b%e6%9c%80%e6%96%b0%e7%94%b5%e5%bd%b1www.dygangs.me%5d.mkv</TrackURI>
    //<RelTime>00:06:20</RelTime>
    //<AbsTime>00:06:20</AbsTime>
    //<RelCount>2147483647</RelCount>
    //<AbsCount>2147483647</AbsCount>
    //</u:GetPositionInfoResponse>
    //</s:Body></s:Envelope>
    public class PositionInfo
    {
        public int Track { get; set; }
        public string TrackDuration { get; set; }
        public string TrackURI { get; set; }
        public string RelTime { get; set; }
        public int RelCount { get; set; }

        public TimeSpan TrackDurationSpan
        {
            get
            {
                if (string.IsNullOrEmpty(TrackDuration))
                {
                    return TimeSpan.Zero;
                }
                return TimeSpan.Parse(TrackDuration);
            }
        }
        public TimeSpan RelTimeSpan
        {
            get
            {
                if (string.IsNullOrEmpty(RelTime)) { return TimeSpan.Zero; }
                return TimeSpan.Parse(RelTime);
            }
        }

        public PositionInfo() { }

        public static PositionInfo ParseXml(string xml)
        {
            var positionInfo = new PositionInfo();
            var xmlDoc = XElement.Parse(xml);

            var node = xmlDoc.XPathSelectElement("//Track");
            if (node != null)
            {
                positionInfo.Track = Convert.ToInt32(node.Value);
            }
            node = xmlDoc.XPathSelectElement("//TrackDuration");
            if (node != null)
            {
                positionInfo.TrackDuration = node.Value;
            }
            node = xmlDoc.XPathSelectElement("//TrackURI");
            if (node != null)
            {
                positionInfo.TrackURI = node.Value;
            }
            node = xmlDoc.XPathSelectElement("//RelTime");
            if (node != null)
            {
                positionInfo.RelTime = node.Value;
            }
            node = xmlDoc.XPathSelectElement("//RelCount");
            if (node != null)
            {
                positionInfo.RelCount = Convert.ToInt32(node.Value);
            }
            return positionInfo;
        }
    }
}
