using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetestVerifierAppWPF.Classes
{
    public class BoardModel
    {
        private String serialNumber;

        public String SerialNumber
        {
            get { return serialNumber; }
            set { serialNumber = value; }
        }

        public override string ToString()
        {
            return serialNumber;
        }

        public static implicit operator BoardModel(String arg)
        {
            return new BoardModel() { SerialNumber = arg };
        }

        public static implicit operator String(BoardModel model)
        {
            return model.SerialNumber;
        }

    }
}
