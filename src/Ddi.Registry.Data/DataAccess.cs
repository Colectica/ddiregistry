using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Configuration;
using System.Diagnostics;

namespace Ddi.Registry.Data
{
    public static class DataAccess
    {
        private static DbProviderFactory factory;
        private static ConnectionStringSettings settings;

        /// <summary>
        /// Singleton instance of the DbProviderFactory for a specific database
        /// </summary>
        public static DbProviderFactory Factory
        {
            get
            {
                if (settings == null)
                {
                    settings = ConfigurationManager.ConnectionStrings["DdiRegistry"];
                }

                if (factory == null)
                {
                    try
                    {
                        factory = DbProviderFactories.GetFactory(settings.ProviderName);
                    }
                    catch (ArgumentException e)
                    {
                        Trace.WriteLine("Could not create database provider: " + e.Message);
                        throw e;
                    }
                }
                return factory;
            }
        }

        public static DbConnection GetDbConnection()
        {
            if (settings == null)
            {
                settings = ConfigurationManager.ConnectionStrings["DdiRegistry"];
            }

            DbConnection connection = Factory.CreateConnection();
            connection.ConnectionString = settings.ConnectionString;
            try
            {
                connection.Open();
            }
            catch (Exception e)
            {
                Trace.WriteLine("Could not connect to database: " + e.Message);                
                throw;
            }

            return connection;
        }

        public static void AddParameter(this DbCommand command, DbType dbType, string parameterName, object parameterValue)
        {
            DbParameter param = command.CreateParameter();
            param.DbType = dbType;
            param.ParameterName = parameterName;
            if (parameterValue is Guid && ((Guid)parameterValue) == default(Guid))
            {
                param.Value = DBNull.Value;
            }
            else if (parameterValue is DateTime && ((DateTime)parameterValue) == DateTime.MinValue)
            {
                param.Value = DBNull.Value;
            }
            else if (parameterValue != null)
            {
                param.Value = parameterValue;
            }
            else
            {
                param.Value = DBNull.Value;
            }
            command.Parameters.Add(param);
        }



    }
}
