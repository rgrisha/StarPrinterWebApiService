using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarMicronics.StarIOExtension;

namespace StarPrinterWebServiceAppNamespace
{
    public interface IPrintTemplate
    {
        void printTemplate(ICommandBuilder builder, Encoding encoding);
        IPrintTemplate fromJson(String json);
    }
}
