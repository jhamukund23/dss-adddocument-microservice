using dss_adddocument_microservice.models;
using dss_adddocument_microservice_models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace dss_adddocument_microservice.data
{
    // PostgresqlWrapper for Create, Update and Delete Operation in database.
    [ExcludeFromCodeCoverage]
    public class PostgresqlWrapper : IPostgresqlWrapper
    {
        private readonly ILogger<PostgresqlWrapper> _logger;
        private readonly IDBHelper _dbHelper;
        public PostgresqlWrapper(ILogger<PostgresqlWrapper> logger, IDBHelper dbHelper)
        {
            _logger = logger;
            this._dbHelper = dbHelper;
        }

        // Call adddocument sp for insert request in database.
        public OutputResponse AddRequest(AddDocumentInbound addDocumentInbound)
        {
            var outputResponse = new OutputResponse();
            if (addDocumentInbound == null)
            {
                throw new ArgumentNullException(nameof(addDocumentInbound));
            }
            NpgsqlConnection con = new(_dbHelper.GetConnectionString());
            try
            {
                con.Open();
                string metadatajsonString = JsonConvert.SerializeObject(addDocumentInbound);               
                using var cmd = new NpgsqlCommand("idmdss.adddocument", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = con;

                //In Parameter
                cmd.Parameters.AddWithValue("in_requestid", NpgsqlDbType.Uuid, Guid.Parse(addDocumentInbound.Id));
                cmd.Parameters.AddWithValue("in_request", NpgsqlDbType.Json, metadatajsonString);
                cmd.Parameters.AddWithValue("in_operation", NpgsqlDbType.Text, addDocumentInbound.Type);

                //Out Parameter
                cmd.Parameters.Add(new NpgsqlParameter("documentid", NpgsqlDbType.Bigint) { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new NpgsqlParameter("docversion", NpgsqlDbType.Smallint) { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new NpgsqlParameter("out_date_added", NpgsqlDbType.Timestamp) { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new NpgsqlParameter("errorcode", NpgsqlDbType.Integer) { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new NpgsqlParameter("errormsg", NpgsqlDbType.Text) { Direction = ParameterDirection.Output });

                cmd.Prepare();
                // Execute the command
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        // Read output value                                     
                        outputResponse.DocumentId =  (dataReader["documentid"] is DBNull) ? 0 : Convert.ToInt64(dataReader["documentid"]);
                        outputResponse.DocVersion =  (dataReader["docversion"] is DBNull) ? (short)0 : (short)dataReader["docversion"];
                        outputResponse.DateAdded =  (dataReader["out_date_added"] is DBNull) ? null : (DateTime)dataReader["out_date_added"];
                        outputResponse.ErrorCode  =  (dataReader["errorcode"] is DBNull) ? 0 : Convert.ToInt32(dataReader["errorcode"]);
                        outputResponse.ErrorMsg   =  (dataReader["errormsg"] is DBNull) ? null : Convert.ToString(dataReader["errormsg"]);
                    }
                }
                cmd.Dispose();
                return outputResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError("PostgresqlWrapper Error : " + ex.Message);
                outputResponse.ErrorMsg= ex.Message;
                outputResponse.ErrorCode= Errors.Unhandled_Error_Code;
                return outputResponse;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }

        // Call updateresponse sp for update response in database.
        public OutputResponse UpdateResponse(AddDocumentOutbound addDocumentOutbound)
        {
            var outputResponse = new OutputResponse();
            if (addDocumentOutbound == null)
            {
                throw new ArgumentNullException(nameof(addDocumentOutbound));
            }
            NpgsqlConnection con = new(_dbHelper.GetConnectionString());
            try
            {
                con.Open();
                string metadatajsonString = JsonConvert.SerializeObject(addDocumentOutbound);                
                using var cmd = new NpgsqlCommand("idmdss.update_response", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = con;

                //In Parameter 
                cmd.Parameters.AddWithValue("in_requestid", NpgsqlDbType.Uuid, Guid.Parse(addDocumentOutbound.Response.RequestId));
                cmd.Parameters.AddWithValue("in_request", NpgsqlDbType.Json, metadatajsonString);
                cmd.Parameters.AddWithValue("in_is_success", NpgsqlDbType.Boolean, addDocumentOutbound.Response.Success);
                cmd.Parameters.AddWithValue("in_docid", NpgsqlDbType.Bigint, addDocumentOutbound.Response.DocId);
                cmd.Parameters.AddWithValue("in_version", NpgsqlDbType.Smallint, addDocumentOutbound.Response.Version);

                //Out Parameter               
                cmd.Parameters.Add(new NpgsqlParameter("errorcode", NpgsqlDbType.Integer) { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new NpgsqlParameter("errormsg", NpgsqlDbType.Text) { Direction = ParameterDirection.Output });

                cmd.Prepare();
                // Execute the command
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        // Read output value from errorcode and errormsg                                  
                        outputResponse.ErrorCode  =  (dataReader["errorcode"] is DBNull) ? 0 : Convert.ToInt32(dataReader["errorcode"]);
                        outputResponse.ErrorMsg   =  (dataReader["errormsg"] is DBNull) ? null : Convert.ToString(dataReader["errormsg"]);
                    }
                }
                cmd.Dispose();
                return outputResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError("PostgresqlWrapper -> UpdateResponse Error : " + ex.Message);
                outputResponse.ErrorMsg= ex.Message;
                outputResponse.ErrorCode= Errors.Unhandled_Error_Code;
                return outputResponse;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }
    }
}
