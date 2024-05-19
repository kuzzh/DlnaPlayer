using DlnaLib.Model;
using DlnaLib.Utils;
using log4net;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DlnaLib.DlnaEvent
{
    public class DlnaPlayStateEvent : DlnaEventBase
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DlnaPlayStateEvent));
        public EnumTransportState TransportState { get; set; }
        public string TransportStatus { get; set; }

        //<e:propertyset xmlns:e="urn:schemas-upnp-org:event-1-0">
        //<e:property>
        //<LastChange><Event xmlns = "urn:schemas-upnp-org:metadata-1-0/AVT/" >< InstanceID val="0"><TransportState val = "PLAYING" />< TransportStatus val="OK"/></InstanceID></Event></LastChange>
        //</e:property>
        //</e:propertyset>
        public static DlnaPlayStateEvent ParseXml(string sid, string xml)
        {
            try
            {
                var dlnaPlayStateEvent = new DlnaPlayStateEvent
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
                dlnaPlayStateEvent.InstanceID = int.Parse(node.Attribute("val").Value);

                node = xmlDoc.Descendants(ns + "TransportState").FirstOrDefault();
                if (node == null)
                {
                    return null;
                }
                dlnaPlayStateEvent.TransportState = (EnumTransportState)Enum.Parse(typeof(EnumTransportState), node.Attribute("val").Value);

                node = xmlDoc.Descendants(ns + "TransportStatus").FirstOrDefault();
                if (node == null)
                {
                    return null;
                }
                dlnaPlayStateEvent.TransportStatus = node.Attribute("val").Value;

                return dlnaPlayStateEvent;
            }
            catch (Exception ex)
            {
                LogUtils.Error(logger, ex);
                return null;
            }
        }
    }
}
