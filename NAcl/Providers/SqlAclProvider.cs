using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;

namespace NAcl.Providers
{
    public class SqlAclProvider : IAclProvider
    {
        private string connectionString;
        public SqlAclProvider()
        {
            connectionString = ConfigurationManager.ConnectionStrings["Acl"].ConnectionString;
        }

        public SqlAclProvider(Configuration.AclConfigurationSection configSection, NameValueCollection parameters)
            : this()
        {
            string connectionStringName = parameters["connectionStringName"];
            this.connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

        }

        #region IAclProvider Members

        public IEnumerable<AccessRule> GetAcls(string resource, string verb)
        {
            IDbConnection connection = new SqlConnection(connectionString);
            var command = connection.CreateCommand();
            if(verb=="*")
                command.CommandText = "SELECT Res, Subject, Mode FROM ACL WHERE @Res LIKE Res+'%'";
            else
                command.CommandText = "SELECT Res, Subject, Mode FROM ACL WHERE @Res LIKE Res+'%' AND (@Verb LIKE Verb+'%' or Verb='*')";
            var resParam = command.CreateParameter();
            resParam.ParameterName = "Res";
            resParam.DbType = DbType.String;
            resParam.Value = resource;
            command.Parameters.Add(resParam);
            var vebParam = command.CreateParameter();
            vebParam.ParameterName = "Verb";
            vebParam.DbType = DbType.String;
            vebParam.Value = verb;
            command.Parameters.Add(vebParam);
            connection.Open();
            List<AccessRule> rules = new List<AccessRule>();
            try
            {
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string res = reader.GetString(0);
                        string subject = reader.GetString(1);
                        string mode = reader.GetString(2);
                        switch (mode)
                        {
                            case "Allow":
                                rules.Add(new Allow(res, verb, subject));
                                break;
                            case "Deny":
                                rules.Add(new Deny(res, verb, subject));
                                break;
                        }
                    }
                }
            }
            finally
            {
                connection.Close();
            }
            return rules;
        }

        public IEnumerable<AccessRule> GetAclsBySubject(params string[] subjects)
        {
            throw new NotImplementedException();
        }

        public IAclProvider SetAcls(params AccessRule[] acls)
        {
            IDbConnection connection = new SqlConnection(connectionString);
            connection.Open();
            IDbTransaction transaction = connection.BeginTransaction();
            try
            {

                foreach (AccessRule ar in acls)
                {
                    IDbCommand command = connection.CreateCommand();
                    command.Transaction = transaction;
                    command.CommandText = "INSERT INTO ACL(Mode,Res,Verb,Subject) VALUES(@Mode, @Res,@Verb, @Subject)";

                    IDbDataParameter modeParam = command.CreateParameter();
                    modeParam.Direction = ParameterDirection.Input;
                    modeParam.ParameterName = "Mode";
                    modeParam.DbType = DbType.String;
                    modeParam.Value = ar.Type.ToString();
                    command.Parameters.Add(modeParam);

                    IDbDataParameter resParam = command.CreateParameter();
                    resParam.Direction = ParameterDirection.Input;
                    resParam.ParameterName = "Res";
                    resParam.DbType = DbType.String;
                    resParam.Value = ar.Resource;
                    command.Parameters.Add(resParam);

                    IDbDataParameter verbParam = command.CreateParameter();
                    verbParam.Direction = ParameterDirection.Input;
                    verbParam.ParameterName = "Verb";
                    verbParam.DbType = DbType.String;
                    verbParam.Value = ar.Verb;
                    command.Parameters.Add(verbParam);

                    IDbDataParameter subjectParam = command.CreateParameter();
                    subjectParam.Direction = ParameterDirection.Input;
                    subjectParam.ParameterName = "Subject";
                    subjectParam.DbType = DbType.String;
                    subjectParam.Value = ar.Subject;
                    command.Parameters.Add(subjectParam);

                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch (SqlException)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
            return this;
        }

        public IAclProvider DeleteAcls(params AccessRule[] acls)
        {
            throw new NotImplementedException();
        }

        public IAclProvider DeleteAcls(string resource, params string[] subjects)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IAclProvider Members


        public event AclChangedHandler AclChanged;

        #endregion
    }
}
