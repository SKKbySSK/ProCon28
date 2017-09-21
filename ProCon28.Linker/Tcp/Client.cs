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
    public class Client
    {
        string channel;
        int port;
        string uri;
        TcpClientChannel client;

        public Client(string Channel, int Port, string Uri)
        {
            channel = Channel;
            port = Port;
            uri = Uri;
            client = new TcpClientChannel();
        }

        public MarshalByRefObject GetObject(Type RemoteObject)
        {
            try
            {
                ChannelServices.RegisterChannel(client, true);
                MarshalByRefObject obj = Activator.GetObject(RemoteObject, "tcp://" + channel + ":" + port + "/" + uri) as MarshalByRefObject;
                return obj;
            }
            finally
            {
                ChannelServices.UnregisterChannel(client);
            }
        }
    }
}
