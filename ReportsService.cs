using Sabio.Web.Domain;
using Sabio.Web.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Sabio.Web.Models;
using Sabio.Web.Models.Requests.chnForm;
using System.Data;
using Sabio.Data;

namespace Sabio.Web.Services
{
    public class ReportsService : BaseService, IMapperGenderDistribution, IReportsService
    {
        private IUserService _userService;


        public ReportsService(IUserService userService)
        {
            _userService = userService;
        }

        public GenderReport GetGendersInLast30Days(int? tenantId)
        {
            GenderReport report = new GenderReport();
 
            if (!tenantId.HasValue)
            {
                tenantId = _userService.GetCurrentTenantId();
            }

            DataProvider.ExecuteCmd(GetConnection, "dbo.Reports_SelectGendersFromFormsInLast30Days"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@TenantId", tenantId);
                }, map: delegate (IDataReader reader, short selectStatementIndex)
                {
                    switch (selectStatementIndex)
                    {
                        case 0:
                            {
                                report.CHN = MapReaderToGenderDistribution(reader);
                                break;
                            }
                        case 1:
                            {
                                report.CA = MapReaderToGenderDistribution(reader);
                                break;
                            }
                    }
                    
                }
                );

            
            return report;

        }

        public AgeReport GetAgeInLast30Days(int? tenantId)
        {
            AgeReport report = new AgeReport();
            if (!tenantId.HasValue)
            {
                tenantId = _userService.GetCurrentTenantId();
            }
            
            DataProvider.ExecuteCmd(GetConnection, "dbo.Reports_SelectAgeGroupFromFormsInLast30Days"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@TenantId", tenantId);
                }, map: delegate (IDataReader reader, short selectStatementIndex)
                {
                    switch (selectStatementIndex)
                    {
                        case 0:
                            {
                                report.CHN = MapReaderToAgeRange(reader);
                                break;
                            }
                        case 1:
                            {
                                report.CA = MapReaderToAgeRange(reader);
                                break;
                            }
                    }
                }
                );
            return report;
        }

        public GenderDistribution MapReaderToGenderDistribution(IDataReader reader, int startingIndex = 0)
        {
            GenderDistribution distribution = new GenderDistribution();

            distribution.Male = reader.GetSafeInt32(startingIndex++);
            distribution.Female = reader.GetSafeInt32(startingIndex++);

            return distribution;
        }

        public List<CAFormReport> GetCAFormsByWeek(int? tenantId)
        {
            List<CAFormReport> report = null;
            if (!tenantId.HasValue)
            {
                tenantId = _userService.GetCurrentTenantId();
            }
            
            DataProvider.ExecuteCmd(GetConnection, "dbo.CAForms_ReportFormCount"
                    , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                        {
                            paramCollection.AddWithValue("@TenantId", tenantId);
                        }, map: delegate (IDataReader reader, short set)
                        {
                            CAFormReport count = new CAFormReport();
                            int startingIndex = 0;

                            count.WeeksOld = reader.GetSafeInt32(startingIndex++);
                            count.FormCount = reader.GetSafeInt32(startingIndex++);

                            if(report == null)
                            {
                                report = new List<CAFormReport>();
                            }
                            report.Add(count);
                        }
                        );

            return report;
        }

        public CHNandCAForm FormCountPerDay(int? tenantId)
        {
            CHNandCAForm Forms = new CHNandCAForm();
            if (!tenantId.HasValue)
            {
                tenantId = _userService.GetCurrentTenantId();
            }

            DataProvider.ExecuteCmd(GetConnection, "dbo.Reports_SelectCHNandCAFormsCreatedInLast30Days"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection) 
                {
                    paramCollection.AddWithValue("@TenantId", tenantId);

                }, map: delegate (IDataReader reader, short selectStatementIndex) 
                {

                    switch (selectStatementIndex)
                    {
                        case 0:
                            {
                                FormCountPerDay p = MapReaderToCountPerDay(reader);

                                if (Forms.CHN == null)
                                {
                                    Forms.CHN = new List<FormCountPerDay>();
                                }
                                Forms.CHN.Add(p);
                                break;
                            }
                        case 1:
                            {
                                FormCountPerDay q = MapReaderToCountPerDay(reader);

                                if (Forms.CA == null)
                                {
                                    Forms.CA = new List<FormCountPerDay>();
                                }
                                Forms.CA.Add(q);
                                break;
                            }
                    }
                });
            return Forms;
        }

        public FormCountPerDay MapReaderToCountPerDay(IDataReader reader, int startingindex = 0)
        {
            FormCountPerDay r = new FormCountPerDay();

            r.Day = reader.GetSafeString(startingindex++);
            r.Count = reader.GetSafeInt32(startingindex++);

            return r;
        }

        public AgeRange MapReaderToAgeRange(IDataReader reader, int startingIndex = 0)
        {
            AgeRange range = new AgeRange();

            range.ZeroToFive = reader.GetSafeInt32(startingIndex++);
            range.SixToTwelve = reader.GetSafeInt32(startingIndex++);
            range.ThirteenToEighteen = reader.GetSafeInt32(startingIndex++);
            range.NineteenToTwentyfive = reader.GetSafeInt32(startingIndex++);
            range.TwentysixToForty = reader.GetSafeInt32(startingIndex++);
            range.FortyoneToSixty = reader.GetSafeInt32(startingIndex++);
            range.SixtyOnePlus = reader.GetSafeInt32(startingIndex++);

            return range;
        }
    }
}