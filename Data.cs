

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using PF_TRANSFER_IN__REGISTER_FORMAT.Models;

namespace PF_TRANSFER_IN__REGISTER_FORMAT.Data
{
    public class PFTransferRepository
    {
        private readonly string? _connectionString;
        private readonly ILogger<PFTransferRepository> _logger;

        public PFTransferRepository(IConfiguration configuration, ILogger<PFTransferRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is missing.");
            _logger = logger;
        }

        public List<PFSettlementModel> GetPFTransferData()
        {
            List<PFSettlementModel> data = new List<PFSettlementModel>();

            try
            {
                using (OracleConnection con = new OracleConnection(_connectionString))
                {
                    string query = "SELECT * FROM PF_TransferIn_Register";
                    OracleCommand cmd = new OracleCommand(query, con);
                    con.Open();
                    OracleDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        data.Add(new PFSettlementModel
                        {
                            SrNo = Convert.ToInt32(reader["SrNo"]),
                            EmpNo = reader["EmpNo"]?.ToString() ?? string.Empty,
                            PF_Number = reader["PF_Number"]?.ToString() ?? string.Empty,
                            Date_Of_Transfer_In = Convert.ToDateTime(reader["Date_Of_TransferIn"]),
                            TRNS_Type = reader["TRNS_Type"]?.ToString() ?? string.Empty,
                            Date_Of_Joining_Prior = reader["Date_Of_Joining_Prior"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["Date_Of_Joining_Prior"]),
                            Name_Of_Member = reader["Name_Of_Member"]?.ToString() ?? string.Empty,
                            Company_Name = reader["Company_Name"]?.ToString() ?? string.Empty,
                            Trust_RPFC_Address = reader["Trust_RPFC_Address"]?.ToString() ?? string.Empty,
                            From_PF_Account = reader["From_PF_Account"]?.ToString() ?? string.Empty,
                            To_PF_Account = reader["To_PF_Account"]?.ToString() ?? string.Empty,
                            Employee_Contb_Amount = reader["Employee_Contb_Amount"] == DBNull.Value
                                ? 0 : Convert.ToDecimal(reader["Employee_Contb_Amount"]),
                            Employer_Contb_Amount = reader["Employer_Contb_Amount"] == DBNull.Value
                                ? 0 : Convert.ToDecimal(reader["Employer_Contb_Amount"]),
                            Total_Contb_Amount = (reader["Employee_Contb_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Employee_Contb_Amount"]))
                                + (reader["Employer_Contb_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Employer_Contb_Amount"])),
                            Status = reader["Status"]?.ToString() ?? string.Empty,
                            FI_Document_Number = reader["FI_Document_Number"]?.ToString() ?? string.Empty
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching PF transfer data.");
            }

            return data;
        }
    }
}
