using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SqlSync.SqlBuild;
using System.Data.EntityClient;
using System.Threading;
using System.Threading.Tasks;
namespace SqlBuildManager.Enterprise.CodeReview
{
    public class CodeReviewManager
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region Validation and CheckSums
        private const string randomStuff1 = "^%EGHYUqwqdq3qsa``08-";
        private const string randomStuff2 = "<>?JKTYrthdfgwrt,./.,UK>><<??>";
        private const string colsolidatedFormat = "{0}|{7}|{1}|{2}|{3}|{8}|{4}|{5}|{6}";
        private const string validationFormat = "{0}|{1}|{7}|{2}|{3}|{4}|{5}|{8}|{6}";

        public static string CalculateReviewCheckSum(Guid codeReviewId, string reviewer, DateTime reviewDate, string comment, string reviewNumber, short reviewStatus, string scriptText)
        {
            string merge = GetConsolidatedBase(codeReviewId,reviewer, reviewDate, comment, reviewNumber, reviewStatus, scriptText);
            System.Security.Cryptography.SHA1CryptoServiceProvider oSHA1Hasher = new System.Security.Cryptography.SHA1CryptoServiceProvider();

            byte[] textBytes = new ASCIIEncoding().GetBytes(merge);
            byte[] arrbytHashValue = oSHA1Hasher.ComputeHash(textBytes);
            string textHash = System.BitConverter.ToString(arrbytHashValue);
            textHash = textHash.Replace("-", "");
            return textHash;
        }
        public static string CalculateReviewCheckSum(SqlSyncBuildData.CodeReviewRow codeReviewRow, string scriptText)
        {
            return CalculateReviewCheckSum(
                codeReviewRow.CodeReviewId,
                codeReviewRow.ReviewBy,
                codeReviewRow.ReviewDate,
                codeReviewRow.Comment,
                codeReviewRow.ReviewNumber,
                codeReviewRow.ReviewStatus,
                scriptText);
        }
        internal static string GetConsolidatedBase(Guid codeReviewId, string reviewer, DateTime reviewDate, string comment, string reviewNumber, short reviewStatus, string scriptText)
        {
            string consolidated = String.Format(colsolidatedFormat, codeReviewId.ToString(), reviewer, reviewDate.ToString(), comment, reviewNumber, reviewStatus, scriptText, randomStuff1, randomStuff2);
            return consolidated;
        }
        public static bool ValidateReviewCheckSum(SqlSyncBuildData.CodeReviewRow codeReviewRow, string scriptText)
        {
            string existingCheckSum = codeReviewRow.CheckSum;

            string newCheckSum = CalculateReviewCheckSum(codeReviewRow, scriptText);

            return existingCheckSum == newCheckSum;
        }
        public static void ValidateReviewCheckSum(SqlSyncBuildData sqlSyncBuildData, string baseDirectory)
        {
            foreach (SqlSyncBuildData.ScriptRow scriptRow in sqlSyncBuildData.Script)
            {
                var codeReview = from cr in sqlSyncBuildData.CodeReview
                                 where cr.ScriptId == scriptRow.ScriptId
                                 select cr;
                if (codeReview.Any())
                {
                    string[] batch = SqlBuildHelper.ReadBatchFromScriptFile(baseDirectory + scriptRow.FileName, false, true);
                    string scriptText = String.Join("", batch);
                    foreach (var crRow in codeReview)
                    {
                        if (!ValidateReviewCheckSum(crRow, scriptText))
                        {
                            crRow.ReviewStatus = (short) CodeReviewStatus.OutOfDate;
                        }
                    }
                }
            }
            sqlSyncBuildData.AcceptChanges();
        }

        public static void SetValidationKey(ref SqlSyncBuildData.CodeReviewRow codeReviewRow)
        {
            codeReviewRow.ValidationKey = ValidationKey(codeReviewRow);
        }

        public static string GetValidationKey(SqlSyncBuildData.CodeReviewRow codeReviewRow)
        {
            return ValidationKey(codeReviewRow);

        }
        internal static string ValidationKey(SqlSyncBuildData.CodeReviewRow codeReviewRow)
        {
            string x = String.Format(validationFormat, 
                codeReviewRow.CodeReviewId.ToString(), 
                codeReviewRow.ReviewBy,
                codeReviewRow.ReviewDate,
                codeReviewRow.Comment,
                codeReviewRow.ReviewNumber,
                codeReviewRow.ReviewStatus,
                codeReviewRow.CheckSum,
                randomStuff1,
                randomStuff2);

            System.Security.Cryptography.SHA1CryptoServiceProvider oSHA1Hasher = new System.Security.Cryptography.SHA1CryptoServiceProvider();

            byte[] textBytes = new ASCIIEncoding().GetBytes(x);
            byte[] arrbytHashValue = oSHA1Hasher.ComputeHash(textBytes);
            string textHash = System.BitConverter.ToString(arrbytHashValue);
            textHash = textHash.Replace("-", "");
            return textHash;
        }
        #endregion

        #region Database Saving/Update
        private static EntityConnection entityConn = null;
        private static EntityConnection Connection
        {
            get
            {
                if(entityConn == null)
                {
                    entityConn = new EntityConnection();
                   
                }
                return entityConn;
            }
        }
        private static SqlCodeReviewEntities GetNewEntity()
        {
            if(EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig != null && EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.DatabaseConnectionString.Length > 0)
                return new SqlCodeReviewEntities(EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.DatabaseConnectionString);
            else
                return new SqlCodeReviewEntities();

        }
        public static bool SaveCodeReview(ref SqlSyncBuildData buildData,ref SqlSyncBuildData.ScriptRow scriptRow, string scriptText, string comment, string reviewBy, DateTime reviewDate, string reviewNumber, int reviewStatus)
        {
            try
            {
                SqlSyncBuildData.CodeReviewRow newRow = buildData.CodeReview.NewCodeReviewRow();
                newRow.CodeReviewId = Guid.NewGuid();
                newRow.Comment = comment;
                newRow.ReviewBy = reviewBy;
                newRow.ReviewDate = reviewDate;
                newRow.ReviewNumber = reviewNumber;
                newRow.ReviewStatus = (short)reviewStatus;
                newRow.CheckSum = CodeReviewManager.CalculateReviewCheckSum(newRow.CodeReviewId,
                    newRow.ReviewBy,
                    newRow.ReviewDate,
                    newRow.Comment,
                    newRow.ReviewNumber,
                    newRow.ReviewStatus,
                    scriptText);

                newRow.SetParentRow(scriptRow);
                CodeReviewManager.SetValidationKey(ref newRow);
                buildData.CodeReview.AddCodeReviewRow(newRow);
                buildData.CodeReview.AcceptChanges();

                CodeReviewManager.SaveCodeReviewToDatabase(newRow);

                return true;
            }
            catch
            {
                return false;
            }

        }
        public static void SaveCodeReviewToDatabase(SqlSyncBuildData.CodeReviewRow codeReviewRow)
        {
            var task = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        CodeReview r = new CodeReview()
                         {
                             CheckSum = codeReviewRow.CheckSum,
                             CodeReviewId = codeReviewRow.CodeReviewId,
                             Comment = codeReviewRow.Comment,
                             ReviewBy = codeReviewRow.ReviewBy,
                             ReviewDate = codeReviewRow.ReviewDate,
                             ReviewNumber = codeReviewRow.ReviewNumber,
                             ReviewStatus = codeReviewRow.ReviewStatus,
                             ScriptId = codeReviewRow.ScriptId,
                             ValidationKey = codeReviewRow.ValidationKey

                         };


                        SqlCodeReviewEntities e = GetNewEntity();
                        e.AddToCodeReviews(r);
                        e.SaveChanges();
                        log.DebugFormat("Saved new CodeReview entity for {0}.", codeReviewRow.CodeReviewId);
                    }
                    catch (Exception exe)
                    {
                        log.Error(String.Format("Unable to save new CodeReview entity for {0}.",codeReviewRow.CodeReviewId), exe);
                    }
                });
        }

        public static bool UpdateCodeReview(ref SqlSyncBuildData buildData, ref SqlSyncBuildData.CodeReviewRow reviewRow, string scriptText)
        {
            try
            {
                reviewRow.CheckSum = CodeReviewManager.CalculateReviewCheckSum(reviewRow, scriptText);
                CodeReviewManager.SetValidationKey(ref reviewRow);
                buildData.CodeReview.AcceptChanges();
                CodeReviewManager.UpdateCodeReviewToDatabase(reviewRow);
                return true;
            }
            catch
            {
                return false;
            }

            
        }
        public static void UpdateCodeReviewToDatabase(SqlSyncBuildData.CodeReviewRow codeReviewRow)
        {
            var task = Task.Factory.StartNew(() =>
             {
                 try
                 {

                     SqlCodeReviewEntities e = GetNewEntity();
                     var r = from cr in e.CodeReviews
                             where cr.CodeReviewId == codeReviewRow.CodeReviewId
                             select cr;
                     if (!r.Any())
                     {
                         SaveCodeReviewToDatabase(codeReviewRow);
                     }
                     else
                     {
                         CodeReview review = r.First();
                         review.CheckSum = codeReviewRow.CheckSum;
                         review.Comment = codeReviewRow.Comment;
                         review.ReviewBy = codeReviewRow.ReviewBy;
                         review.ReviewDate = codeReviewRow.ReviewDate;
                         review.ReviewNumber = codeReviewRow.ReviewNumber;
                         review.ReviewStatus = codeReviewRow.ReviewStatus;
                         review.ScriptId = codeReviewRow.ScriptId;
                         review.ValidationKey = codeReviewRow.ValidationKey;

                         e.SaveChanges();

                     }
                     log.DebugFormat("Updated CodeReview entity for {0}.", codeReviewRow.CodeReviewId);
                 }
                 catch (Exception exe)
                 {
                     log.Error(String.Format("Unable to update CodeReview entity for {0}.", codeReviewRow.CodeReviewId), exe);
                 }
             });
        }

        public static bool MarkCodeReviewOutOfDate(ref SqlSyncBuildData buildData, ref SqlSyncBuildData.CodeReviewRow reviewRow)
        {
            reviewRow.ReviewStatus = (short)CodeReviewStatus.OutOfDate;
            buildData.CodeReview.AcceptChanges();

            return true;
        }


        public static SqlSyncBuildData LoadCodeReviewData(SqlSyncBuildData buildData, out bool databaseSuccess)
        {
            //Make sure the table it empty...
            SqlSyncBuildData.CodeReviewDataTable crTable = buildData.CodeReview;

            //Get the list of script ids...
            var scriptIds = from s in buildData.Script
                            select s.ScriptId;
            List<string> lstScriptIds = scriptIds.ToList();
            try //sync the database and the local reviews if possible...
            {

                SqlCodeReviewEntities e = GetNewEntity();

                //Get all reviews from the database that match our scripts...
                var reviews = from crEntity in e.CodeReviews
                              where lstScriptIds.Contains(crEntity.ScriptId)
                              select crEntity;
                //string sql = ((System.Data.Objects.ObjectQuery)reviews).ToTraceString();
                List<CodeReview> lstDatabaseReviews = reviews.ToList();


                //find the matching EF CodeReview and CodeReview table entries and sync as appropriate
                var matches = from r in lstDatabaseReviews
                              join c in crTable on r.CodeReviewId equals c.CodeReviewId
                              select new {r, c};
                var matchList = matches.ToList();

                //Update the EF reviews where there is a matching, more current local version
                bool localChanges = false;
                foreach (var pair in matchList)
                {
                    //Only need to update if the XML review is more current...
                    if (pair.c.ReviewDate > pair.r.ReviewDate)
                    {
                        SyncXmlReviewDatatoEfReview(pair.c, pair.r);
                        localChanges = true;
                    }
                }


                //Add to the EF reviews if there are local ones that don't exist in the database...
                var efReview = from r in reviews select r.CodeReviewId;
                List<Guid> efReviewIds = efReview.ToList();
                var localOnlyId = (from c in crTable
                                   select c.CodeReviewId).Except(efReviewIds);

                var localOnlyRow = from c in crTable
                                   where localOnlyId.Contains(c.CodeReviewId)
                                   select c;

                foreach (SqlSyncBuildData.CodeReviewRow row in localOnlyRow)
                {
                    CodeReview codeReview = new CodeReview();
                    SyncXmlReviewDatatoEfReview(row, codeReview);
                    lstDatabaseReviews.Add(codeReview); //add to local collection
                    e.CodeReviews.AddObject(codeReview); //add to database collection
                    localChanges = true;
                }

                if (localChanges)
                    e.SaveChanges();


                //Now that the database can be considered the "master" set of data, load it up!
                try
                {


                    //Clear out the local rows and pull from database...
                    buildData.CodeReview.Rows.Clear();

                    List<SqlSyncBuildData.CodeReviewRow> newRows = new List<SqlSyncBuildData.CodeReviewRow>();
                    Parallel.ForEach(buildData.Script, row =>
                        {
                            string scriptId = row.ScriptId;

                            var match = from r in lstDatabaseReviews
                                        where r.ScriptId == scriptId
                                        select r;

                            foreach (CodeReview cr in match)
                            {
                                SqlSyncBuildData.CodeReviewRow newRow = buildData.CodeReview.NewCodeReviewRow();
                                newRow.ScriptId = row.ScriptId;
                                newRow.CheckSum = cr.CheckSum;
                                newRow.CodeReviewId = cr.CodeReviewId;
                                newRow.Comment = cr.Comment;
                                newRow.ReviewBy = cr.ReviewBy;
                                newRow.ReviewDate = (DateTime) cr.ReviewDate;
                                newRow.ReviewNumber = cr.ReviewNumber;
                                newRow.ReviewStatus = (short) cr.ReviewStatus;
                                newRow.ValidationKey = cr.ValidationKey;
                                newRow.ScriptRow = row;

                                newRows.Add(newRow);

                            }
                        });

                    //Add them on the same thread...
                    foreach (SqlSyncBuildData.CodeReviewRow newRow in newRows)
                        buildData.CodeReview.AddCodeReviewRow(newRow);

                    buildData.AcceptChanges();
                    databaseSuccess = true;
                }
                catch (System.Data.EntityException exe)
                {
                    log.Error(
                        "Unable to connect to SqlCodeReview database. Code Reviews will not be sync'd with the database",
                        exe);
                    databaseSuccess = false;
                }
                catch (Exception generalExe)
                {
                    log.Error("Unable to read SqlCodeReview", generalExe);
                    databaseSuccess = false;
                }
            }
            catch (System.Data.SqlClient.SqlException sqlExe)
            {
                log.Error(string.Format("Current user '{0}' errored at the database", System.Environment.UserName), sqlExe);
                databaseSuccess = false;
            }
            catch (Exception ex)
            {
                log.Error(
                    "Unable to sync local code reviews with SqlCodeReview database. Code Reviews will not be sync'd with the database",
                    ex);
                databaseSuccess = false;
            }
            return buildData;
        }

        internal static void SyncXmlReviewDatatoEfReview(SqlSyncBuildData.CodeReviewRow row, CodeReview review)
        {
            //var rows = codeReviewTable.Where(t => t.CodeReviewId == codeReviewId);
            //if (rows.Count() > 0)
            //{
            //    SqlSyncBuildData.CodeReviewRow row = rows.First();
                review.CheckSum = row.CheckSum;
                review.CodeReviewId = row.CodeReviewId;
                review.Comment = row.Comment;
                review.ReviewBy = row.ReviewBy;
                review.ReviewDate = row.ReviewDate;
                review.ReviewNumber = row.ReviewNumber;
                review.ReviewStatus = row.ReviewStatus;
                review.ScriptId = row.ScriptId;
                review.ValidationKey = row.ValidationKey;
        }
        #endregion


      
    }
}
