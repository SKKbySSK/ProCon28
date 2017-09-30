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
    public class Server : IDisposable
    {
        int port;
        TcpServerChannel server;
        MarshalByRefObject obj;
        string uri;

        public Server(int Port, MarshalByRefObject Object, string Uri)
        {
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

        public string ChannelIP
        {
            get
            {
                string uri = ChannelUri;
                uri = uri.Replace("tcp://", "");
                int index = uri.IndexOf(':');
                return uri.Substring(0, index);
            }
        }

        public void Marshal()
        {
            server = new TcpServerChannel(port);
            
            ChannelServices.RegisterChannel(server, false);
            RemotingServices.Marshal(obj, uri, obj.GetType());
        }

        public void Dispose()
        {
            RemotingServices.Disconnect(obj);
            ChannelServices.UnregisterChannel(server);
            server = null;
            obj = null;
            GC.SuppressFinalize(this);
        }
    }
}
