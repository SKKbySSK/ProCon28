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
    public class Client : IDisposable
    {
        string channel;
        int port;
        string uri;
        TcpClientChannel client;
        bool reg = false;

        public Client(string Channel, int Port, string Uri)
        {
            channel = Channel;
            port = Port;
            uri = Uri;
            client = new TcpClientChannel();
        }

        public void Dispose()
        {
            if (client != null && reg)
            {
                try
                {
                    ChannelServices.UnregisterChannel(client);
                }
                catch (Exception) { }
                client = null;
            }
            GC.SuppressFinalize(this);
        }

        public T GetObject<T>() where T : MarshalByRefObject
        {
            try
            {
                ChannelServices.RegisterChannel(client, false);
                reg = true;
                MarshalByRefObject obj = Activator.GetObject(typeof(T), "tcp://" + channel + ":" + port + "/" + uri) as MarshalByRefObject;
                return (T)obj;
            }
            finally
            {
                ChannelServices.UnregisterChannel(client);
            }
        }
    }
}
