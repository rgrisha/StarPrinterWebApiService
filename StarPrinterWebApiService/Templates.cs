using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using StarMicronics.StarIOExtension;
using StarPrinterWebServiceAppNamespace;

namespace StarPrinterWebServiceAppNamespace
{

    public class Templates 
    {
        
        private Dictionary<string, IPrintTemplate> templates { get; } = 
            new Dictionary<string, IPrintTemplate>
                {
                    { "ac21a37d-8c73-4884-81cd-1eedc300379b", new Vaztarastis() }
                };

        public IPrintTemplate findTemplate(string templateId)
        {
            IPrintTemplate value;
            return templates.TryGetValue(templateId, out value) ? value : null;
        }
    }

    public class Vaztarastis : IPrintTemplate
    {
        public String companyName { get; set; }
        public String companyAddress { get; set; }
        public Decimal amount { get; set; }

        override
        public String ToString()
        {
            return $"Company Name: {companyName}, Company Address: {companyAddress}, Amount: {amount}";
        }

        public void printTemplate(ICommandBuilder builder, Encoding encoding)
        {

            builder.AppendAlignment(AlignmentPosition.Center);

            builder.Append(
                    encoding.GetBytes(this.companyName + "\n")); 
            builder.Append(
                    encoding.GetBytes(this.companyAddress + "\n")); 

            /*
            for(int i = 256; i < 1000; i++) {
                char c = (char)i;
                builder.Append(encoding.GetBytes(Char.ToString(c)));
            }   
            */

            builder.AppendAlignment(AlignmentPosition.Right);

            builder.AppendMultiple(encoding
                .GetBytes("Suma: " + this.amount.ToString()), 2, 2);

        }

        public IPrintTemplate fromJson(String json)
        {
            return JsonConvert.DeserializeObject<Vaztarastis>(json);
        }
    }

}
