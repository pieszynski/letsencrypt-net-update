using Oocx.Acme.Console;
using Pieszynski.LenuManager.Core;
using Pieszynski.LenuManager.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pieszynski.LenuManager
{
    class Program
    {
        static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        static async Task MainAsync(string[] args)
        {
            try
            {
                Options options = LenuHelpers.GetAppSettingsOptions();
                await LenuHelpers.RequestSslCertificate(options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("ENTER = stop");
            Console.ReadLine();
        }
    }
}
