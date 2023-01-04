using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
namespace SqlSync.SprocTest.Configuration
{
    public partial class Database
    {
        [System.Xml.Serialization.XmlIgnore]
        string fileName = string.Empty;

        [System.Xml.Serialization.XmlIgnore]
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }



        public bool AddNewStoredProcedure(string storedProcName)
        {
            return AddNewStoredProcedures(new string[] { storedProcName });
        }
        public bool AddNewStoredProcedures(string[] storedProcNames)
        {
            try
            {
                List<StoredProcedure> spColl = new List<StoredProcedure>();
                if (StoredProcedure != null)
                    spColl.AddRange(StoredProcedure);

                bool exclude = false;
                for (int i = 0; i < storedProcNames.Length; i++)
                {
                    exclude = false;
                    for (int j = 0; j < spColl.Count; j++)
                        if (spColl[j].Name.Trim().ToLower() == storedProcNames[i].Trim().ToLower())
                        {
                            exclude = true;
                            break;
                        }

                    if (!exclude)
                    {
                        StoredProcedure sp = new StoredProcedure();
                        sp.Name = storedProcNames[i];
                        sp.ID = Guid.NewGuid().ToString();
                        spColl.Add(sp);

                    }

                }
                StoredProcedure = null;
                StoredProcedure = spColl.ToArray();
            }
            catch
            {
                return false;
            }

            return true;
        }
        public bool RemoveStoredProcedure(string storedProcName, string ID)
        {
            KeyValuePair<string, string> sp = new KeyValuePair<string, string>(storedProcName, ID);
            return RemoveStoredProcedures(new KeyValuePair<string, string>[] { sp });
        }
        public bool RemoveStoredProcedures(KeyValuePair<string, string>[] storedProcs)
        {
            bool found = false;
            if (StoredProcedure == null)
                return true;

            for (int i = 0; i < StoredProcedure.Length; i++)
            {
                for (int j = 0; j < storedProcs.Length; j++)
                {
                    if (storedProcs[j].Key.Trim().ToLower() == StoredProcedure[i].Name.Trim().ToLower() &&
                       storedProcs[j].Value.Trim().ToLower() == StoredProcedure[i].ID.Trim().ToLower())
                    {
                        StoredProcedure[i] = null;
                        found = true;
                        break;
                    }
                }
            }

            List<StoredProcedure> tmp = new List<StoredProcedure>();
            for (int i = 0; i < StoredProcedure.Length; i++)
                if (StoredProcedure[i] != null)
                    tmp.Add(StoredProcedure[i]);

            StoredProcedure = null;
            StoredProcedure = tmp.ToArray();

            return found;

        }

        public bool AddNewTestCase(string storedProcedureName, TestCase newTestcase)
        {
            try
            {
                //Try to add the SP.. if it already exists, it won't duplicate
                if (!AddNewStoredProcedure(storedProcedureName))
                    return false;

                for (int i = 0; i < StoredProcedure.Length; i++)
                {
                    if (StoredProcedure[i].Name.ToLower() == storedProcedureName.ToLower())
                    {
                        List<TestCase> tc = new List<TestCase>();
                        if (StoredProcedure[i].TestCase != null)
                            tc.AddRange(StoredProcedure[i].TestCase);
                        tc.Add(newTestcase);
                        StoredProcedure[i].TestCase = tc.ToArray();
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;


        }
        public TestCase AddNewTestCase(string storedProcedureName, Dictionary<string, string> paramKeyValuePairs)
        {
            TestCase tc = new TestCase();
            tc.TestCaseId = Guid.NewGuid().ToString();
            tc.Parameter = new Parameter[paramKeyValuePairs.Count];
            int i = 0;
            foreach (KeyValuePair<string, string> pair in paramKeyValuePairs)
            {
                tc.Parameter[i] = new Parameter();
                tc.Parameter[i].Name = pair.Key;
                tc.Parameter[i].Value = pair.Value;
                tc.Parameter[i].HasDerivedParameterMatch = true;
                i++;
            }
            tc.Name = "Execute from Script";
            tc.ExpectedResult = new ExpectedResult();
            tc.ExpectedResult.RowCount = 0;
            tc.ExpectedResult.RowCountSpecified = true;
            tc.ExpectedResult.RowCountOperator = RowCountOperator.GreaterThan;
            tc.ExpectedResult.RowCountOperatorSpecified = true;

            if (AddNewTestCase(storedProcedureName, tc))
                return tc;
            else
                return null;
        }
        public bool ModifyExistingTestCase(string storedProcedureName, TestCase modifiedTestCase)
        {
            if (StoredProcedure == null)
            {
                if (!AddNewStoredProcedures(new string[] { storedProcedureName }))
                    return false;

                if (!AddNewTestCase(storedProcedureName, modifiedTestCase))
                    return false;
            }
            else
            {
                bool found = false;
                for (int i = 0; i < StoredProcedure.Length; i++)
                {
                    if (StoredProcedure[i].Name.ToLower() == storedProcedureName.ToLower())
                    {
                        List<TestCase> tc = new List<TestCase>();
                        if (StoredProcedure[i].TestCase != null)
                            tc.AddRange(StoredProcedure[i].TestCase);

                        for (int j = 0; j < tc.Count; j++)
                        {
                            if (tc[j].TestCaseId.ToLower() == modifiedTestCase.TestCaseId.ToLower())
                            {
                                tc[j] = modifiedTestCase;
                                found = true;
                                break;
                            }
                        }

                        if (found)
                        {
                            StoredProcedure[i].TestCase = tc.ToArray();
                            break;
                        }
                    }
                }
            }
            return true;
        }
        public void RemovedTestCase(string storedProcedureName, TestCase testCaseToRemove)
        {
            StoredProcedure sp = GetStoredProcedureByName(storedProcedureName);
            bool found = false;
            if (sp != null)
            {
                if (sp.TestCase != null)
                    for (int j = 0; j < sp.TestCase.Length; j++)
                    {
                        if (sp.TestCase[j].TestCaseId == testCaseToRemove.TestCaseId)
                        {
                            sp.TestCase[j] = null;
                            found = true;
                            break;
                        }
                    }
            }
            if (found)
            {
                List<TestCase> tc = new List<TestCase>();
                for (int i = 0; i < sp.TestCase.Length; i++)
                    if (sp.TestCase[i] != null)
                        tc.Add(sp.TestCase[i]);

                sp.TestCase = tc.ToArray();
            }
        }
        private StoredProcedure GetStoredProcedureByName(string storedProcName)
        {

            for (int i = 0; i < StoredProcedure.Length; i++)
            {
                if (storedProcName.ToLower() == StoredProcedure[i].Name.ToLower())
                {
                    return StoredProcedure[i];
                }

            }
            return null;
        }
        public bool SaveConfiguration(string fileName)
        {
            System.Xml.XmlTextWriter tw = null;
            try
            {
                XmlSerializer xmlS = new XmlSerializer(typeof(SprocTest.Configuration.Database));
                tw = new System.Xml.XmlTextWriter(fileName, Encoding.UTF8);
                tw.Formatting = System.Xml.Formatting.Indented;
                tw.Indentation = 3;
                xmlS.Serialize(tw, this);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (tw != null)
                    tw.Close();
            }

        }

        public bool Import(Database importCfg)
        {
            if (importCfg.StoredProcedure == null)
                return false;

            List<StoredProcedure> sp = new List<StoredProcedure>();
            if (StoredProcedure != null)
                sp.AddRange(StoredProcedure);

            bool found;
            for (int j = 0; j < importCfg.StoredProcedure.Length; j++)
            {
                found = false;
                for (int i = 0; i < sp.Count; i++)

                    if (sp[i].Name.Trim().ToLower() == importCfg.StoredProcedure[j].Name.Trim().ToLower())
                    {
                        if (importCfg.StoredProcedure[j].TestCase != null)
                        {
                            StoredProcedure spI = sp[i];
                            ImportTestCases(ref spI, importCfg.StoredProcedure[j]);
                        }
                        found = true;
                    }


                if (!found)
                    sp.Add(importCfg.StoredProcedure[j]);
            }
            StoredProcedure = sp.ToArray();
            return true;

        }
        private void ImportTestCases(ref StoredProcedure masterSP, StoredProcedure importSP)
        {
            if (importSP.TestCase == null)
                return;

            List<TestCase> tc = new List<TestCase>();
            if (masterSP.TestCase != null)
                tc.AddRange(masterSP.TestCase);


            for (int i = 0; i < importSP.TestCase.Length; i++)
            {
                for (int j = 0; j < tc.Count; j++)
                {
                    if (importSP.TestCase[i].TestCaseId.ToLower() == tc[j].TestCaseId.ToLower())
                    {
                        importSP.TestCase[i].TestCaseId = Guid.NewGuid().ToString();
                        break;
                    }
                }
                tc.Add(importSP.TestCase[i]);
            }

            masterSP.TestCase = tc.ToArray();
        }
        public void SelectAllTests()
        {
            if (StoredProcedure != null)
                foreach (StoredProcedure sp in StoredProcedure)
                {
                    if (sp.TestCase != null)
                        foreach (TestCase tc in sp.TestCase)
                        {
                            tc.SelectedForRun = true;
                        }
                }
        }
    }
}
