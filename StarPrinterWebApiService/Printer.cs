using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarMicronics.StarIO;
using StarMicronics.StarIOExtension;
using NLog;

namespace StarPrinterWebServiceAppNamespace
{
    public class Printer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public PortInfo portInfo { get; private set; }
        private IPort port;

        public bool available()
        {
            return portInfo != null;
        }

        public string getInfo()
        {
            return portInfo.PortName + " " + portInfo.ModelName;
        }

        public void findPrinterIfNotAvailable()
        {
            if (available()) return;

            logger.Info("Searching for printer");

            List<PortInfo> portList = Factory.I.SearchPrinter(PrinterInterfaceType.Ethernet);
            logger.Info("Found {count} printers", portList.Count);
            foreach(PortInfo port in portList)
            {
                logger.Info("port: " + port.ToString() + " " + port.PortName + " " + port.ModelName + " " + port.GetType());
            }
            portInfo = portList.Any() ? portList.First() : null;
        }

        public void printTemplate(IPrintTemplate template)
        {
            ICommandBuilder builder = beginPrint();
            template.printTemplate(builder, Encoding.UTF8);
            endPrint(builder);
        }

        private ICommandBuilder beginPrint()
        {
            Emulation emulation = Emulation.None;
            int paperSize = 576;

            ICommandBuilder builder = StarIoExt.CreateCommandBuilder(emulation);

            builder.BeginDocument();

            builder.AppendFontStyle(FontStyleType.A);
            //builder.AppendInternational(InternationalType.USA);
            builder.AppendCodePage(CodePageType.UTF8);

            //builder.AppendCharacterSpace(0);

            return builder;
        }

        public void endPrint(ICommandBuilder builder)
        {
            builder.AppendCutPaper(CutPaperAction.PartialCutWithFeed);
            builder.AppendPeripheral(PeripheralChannel.No1);
            builder.EndDocument();
            byte[] commands = builder.Commands;
            sendCommands(commands, port); 
        }

        private void sendCommands(byte[] commands, IPort port)
        {
            if (!available())
            {
                throw new PrinterException(PrinterException.PrinterError.PORT_IS_NULL);
            }

            port = Factory.I.GetPort(portInfo.PortName, "", 10000);
            if (port == null)
            {
                throw new PrinterException(PrinterException.PrinterError.PORT_IS_NULL);
            }

            StarPrinterStatus printerStatus;

            printerStatus = port.BeginCheckedBlock();

            if (printerStatus.Offline)
            {
                if (printerStatus.ReceiptPaperEmpty)
                {
                    throw new PrinterException(PrinterException.PrinterError.PAPER_IS_EMPTY);    
                }
                if (printerStatus.CoverOpen)
                {
                    throw new PrinterException(PrinterException.PrinterError.COVER_IS_OPEN);    
                }
                throw new PrinterException(PrinterException.PrinterError.PRINTER_IS_OFFLINE);    
            }

            uint commandsLength = (uint)commands.Length;
            uint writtenLength = port.WritePort(commands, 0, commandsLength);

            if (writtenLength != commandsLength)
            {
                throw new PrinterException(PrinterException.PrinterError.WRITE_PORT_FAILED);    
            }

            printerStatus = port.EndCheckedBlock();

            if (printerStatus.Offline)
            {
                if (printerStatus.ReceiptPaperEmpty)
                {
                    throw new PrinterException(PrinterException.PrinterError.PAPER_IS_EMPTY);    
                }
                if (printerStatus.CoverOpen)
                {
                    throw new PrinterException(PrinterException.PrinterError.COVER_IS_OPEN);    
                }
                throw new PrinterException(PrinterException.PrinterError.PRINTER_IS_OFFLINE);    
            }
        }
    }
}
