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

        public static implicit operator BoardModel(string arg)
        {
            return new BoardModel() { SerialNumber = arg };
        }

        public static implicit operator string(BoardModel model)
        {
            if (model == null) return "";
            return model.SerialNumber;
        }

        public bool Equals(BoardModel that)
        {
            if (this == null && that == null) return true;
            else if (that == null) return false;
            return SerialNumber.Equals(that.SerialNumber, StringComparison.OrdinalIgnoreCase);
        }

        public static BoardModel ToBoardModel(string arg)
        {
            return new BoardModel() { SerialNumber = arg };
        }
    }
}
