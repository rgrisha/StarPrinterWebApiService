using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarPrinterWebServiceAppNamespace
{

    public class PrinterException: Exception
    {

        public enum PrinterError
        {
            ERROR_OPEN_PORT,
            PORT_IS_NULL,
            PAPER_IS_EMPTY,
            COVER_IS_OPEN,
            PRINTER_IS_OFFLINE,
            WRITE_PORT_FAILED,
        }

        private PrinterError printerError { get; set; }

        public PrinterException(PrinterError error) : base("Printer error: " + error)
        {
            this.printerError = error;
        }
    }
}
