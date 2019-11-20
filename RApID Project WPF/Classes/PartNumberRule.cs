using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RApID_Project_WPF.Classes
{
    public class PartNumberRule : ValidationRule
    {
        public static int CALL_COUNT = 1;

        public PartNumberRule() { }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string pnum = value.ToString();
            if (string.IsNullOrWhiteSpace(pnum)) return ValidationResult.ValidResult;
            else return CheckSQLTable(pnum);
        }

        private ValidationResult CheckSQLTable(string pnum)
        {
            using (var conn = new SqlConnection(csObjectHolder.csObjectHolder.ObjectHolderInstance().HummingBirdConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT PartNumber, PartName, VendorName FROM [HummingBird].[dbo].[ItemMaster] WHERE [PartNumber] = @PNum", conn))
                {
                    string item = string.Empty;
                    cmd.Parameters.AddWithValue("@PNum", pnum);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            item = !reader.HasRows ? "" : reader[0].ToString();
                            Console.WriteLine($"Call #{CALL_COUNT}({pnum}) -> Found an item!");
                        }
                    }

                    CALL_COUNT++;

                    return !string.IsNullOrWhiteSpace(item)
                        ? ValidationResult.ValidResult
                        : new ValidationResult(false, $"This part number wasn't found in the ItemMaster table!");
                }
            }
        }
    }
}
