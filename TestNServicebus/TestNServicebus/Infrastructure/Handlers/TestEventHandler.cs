using NServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestNServicebus.Infrastructure.Handlers
{

    public interface IEvent
    {

    }

    public class InseritaFirmaBolla : IEvent
    {
        public string ViaggioId { get; set; }
        public string NumeroBolla { get; set; }
        public int VersioneEsitoBolla { get; set; }
    }


    public class TestEventHandler : IHandleMessages<InseritaFirmaBolla>
    {
       

        public TestEventHandler()
        {
            
        }

        public  Task Handle(InseritaFirmaBolla message, IMessageHandlerContext context)
        {
            Console.WriteLine("Ricevuto InseritaFirmaBolla");
            return Task.CompletedTask;
        }
    }
}
