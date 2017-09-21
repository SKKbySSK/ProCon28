using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace ProCon28.Linker.Tcp
{
    public class Server
    {
        string channel;
        int port;
        TcpServerChannel server;
        ObjRef oref;
        MarshalByRefObject obj;
        string uri;

        public Server(string Channel, int Port, MarshalByRefObject Object, string Uri)
        {
            channel = Channel;
            port = Port;
            obj = Object;
            uri = Uri;
        }

        public string[] GetUrls()
        {
            return server.GetUrlsForUri(uri);
        }

        public string ChannelUri
        {
            get { return server.GetChannelUri(); }
        }

        public void Marshal()
        {
            server = new TcpServerChannel(channel, port);
            ChannelServices.RegisterChannel(server, true);
            oref = RemotingServices.Marshal(obj, uri, typeof(RemotePieces));
        }

        public void Unmarshal()
        {
            RemotingServices.Unmarshal(oref);
            oref = null;

            ChannelServices.UnregisterChannel(server);
            server = null;
        }

        ~Server()
        {
            if (oref != null)
                Unmarshal();
        }
    }
}
