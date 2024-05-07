using System;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DlnaLib
{
    public class TransportInfo
    {
        public EnumTransportState CurrentTransportState { get; set; }
        public string CurrentTransportStatus { get; set; }
        public string CurrentSpeed { get; set; }

        //<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/" s:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/"><s:Body>
        //<u:GetTransportInfoResponse xmlns:u="urn:schemas-upnp-org:service:AVTransport:1">
        //<CurrentTransportState>PLAYING</CurrentTransportState>
        //<CurrentTransportStatus>OK</CurrentTransportStatus>
        //<CurrentSpeed>1</CurrentSpeed>
        //</u:GetTransportInfoResponse>
        //</s:Body></s:Envelope>
        public static TransportInfo ParseXml(string xml)
        {
            var transportInfo = new TransportInfo();
            var xmlDoc = XElement.Parse(xml);

            var node = xmlDoc.XPathSelectElement("//CurrentTransportState");
            if (node != null)
            {
                transportInfo.CurrentTransportState = (EnumTransportState)Enum.Parse(typeof(EnumTransportState), node.Value);
            }
            node = xmlDoc.XPathSelectElement("//CurrentTransportStatus");
            if (node != null)
            {
                transportInfo.CurrentTransportStatus = node.Value;
            }
            node = xmlDoc.XPathSelectElement("//CurrentSpeed");
            if (node != null)
            {
                transportInfo.CurrentSpeed = node.Value;
            }

            return transportInfo;

        }
    }
}
