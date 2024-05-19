using DlnaLib.Utils;
using log4net;
using System;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DlnaLib.DlnaEvent
{
    public class DlnaPlayTrackChangedEvent : DlnaEventBase
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DlnaPlayTrackChangedEvent));
        // 1
        public int NumberOfTracks { get; set; }
        // 1
        public int CurrentTrack { get; set; }
        // 00:00:00
        public string CurrentMediaDuration { get; set; }
        // 00:00:00
        public string CurrentTrackDuration { get; set; }

        public bool IsPlayingFinished()
        {
            return NumberOfTracks == 1 &&
                    CurrentTrack == 1 &&
                    CurrentTrackDuration == "00:00:00" &&
                    CurrentTrackDuration == "00:00:00";
        }

        //<e:propertyset xmlns:e="urn:schemas-upnp-org:event-1-0">
        //<e:property>
        //<LastChange><Event xmlns = "urn:schemas-upnp-org:metadata-1-0/AVT/"><InstanceID val="0"><NumberOfTracks val = "1" />< CurrentTrack  val="1"/><CurrentMediaDuration val = "00:00:00" /><CurrentTrackDuration  val="00:00:00"/></InstanceID></Event></LastChange>
        //</e:property>
        //</e:propertyset>
        public static DlnaPlayTrackChangedEvent ParseXml(string sid, string xml)
        {
            try
            {
                var dlnaPlayTrackChangedEvent = new DlnaPlayTrackChangedEvent
                {
                    SID = sid
                };

                var xmlDoc = XElement.Parse(xml);

                XNamespace ns = "urn:schemas-upnp-org:metadata-1-0/AVT/";

                var node = xmlDoc.Descendants(ns + "InstanceID").FirstOrDefault();
                if (node == null)
                {
                    return null;
                }
                dlnaPlayTrackChangedEvent.InstanceID = int.Parse(node.Attribute("val").Value);

                node = xmlDoc.Descendants(ns + "NumberOfTracks").FirstOrDefault();
                if (node == null)
                {
                    return null;
                }
                dlnaPlayTrackChangedEvent.NumberOfTracks = int.Parse(node.Attribute("val").Value);
                node = xmlDoc.Descendants(ns + "CurrentTrack").FirstOrDefault();
                if (node == null)
                {
                    return null;
                }
                dlnaPlayTrackChangedEvent.CurrentTrack = int.Parse(node.Attribute("val").Value);
                node = xmlDoc.Descendants(ns + "CurrentMediaDuration").FirstOrDefault();
                if (node == null)
                {
                    return null;
                }
                dlnaPlayTrackChangedEvent.CurrentMediaDuration = node.Attribute("val").Value;
                node = xmlDoc.Descendants(ns + "CurrentTrackDuration").FirstOrDefault();
                if (node == null)
                {
                    return null;
                }
                dlnaPlayTrackChangedEvent.CurrentTrackDuration = node.Attribute("val").Value;

                return dlnaPlayTrackChangedEvent;
            }
            catch (Exception ex)
            {
                LogUtils.Error(logger, ex);
                return null;
            }
        }
    }
}
