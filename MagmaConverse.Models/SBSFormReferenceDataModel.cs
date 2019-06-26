using System;
using MagmaConverse.Data;
using MagmaConverse.Interfaces;

namespace MagmaConverse.Models
{
    public sealed class SBSFormReferenceDataModel : SBSPersistableModelBase<FormCreationReferenceData>, IHasLookup
    {
        #region Events
        #endregion

        #region Variables
        #endregion

        #region Constructors
        private static SBSFormReferenceDataModel m_instance;
        public static SBSFormReferenceDataModel Instance => m_instance ?? (m_instance = new SBSFormReferenceDataModel());

        private SBSFormReferenceDataModel()
        {
            StringSubstitutor.ReferenceDataRepository = this;
        }
        #endregion

        #region Cleanup
        #endregion

        #region Methods        
        public void AddData(FormCreateRequest request)
        {
            if (request?.ReferenceData == null)
                return;

            foreach (var refData in request.ReferenceData)
            {
                if (this.Repository.ContainsKey(refData.Name))
                    throw new ApplicationException($"Duplicate key {refData.Name} in the Form Reference Data Repository");
                this.Repository.Add(refData.Name, refData);
            }
        }

        public object Get(string name)
        {
            if (this.Repository.TryGetValue(name, out var data))
            {
                if (data.SortedDictionary != null)
                    return data.SortedDictionary;
                if (data.Array != null)
                    return data.Array;
            }

            return null;
        }
        #endregion

        public override bool LoadFromDatabase()
        {
            return base.LoadFromDatabase();
#if DEPRECATED
            this.Logger.Info("SBSFormReferenceDataModel - starting to load from Mongo");

            try
            {
                var list = this.DatabaseAdapter.Load();
                if (list == null)
                    return false;  // Maybe the dataqbase is down?

                var data = list as IList<FormCreationReferenceData> ?? list.ToList();
                if (data.Count == 0)
                    return true;

                this.Repository.Clear();

                foreach (var refData in data)
                {
                    if (!this.Repository.ContainsKey(refData.Name))
                        this.Repository.Add(refData.Name, refData);
                }
            }
            catch (Exception exc)
            {
                this.Logger.Error(exc.Message);
                throw;
            }

            this.Logger.Info("SBSFormReferenceDataModel - successfully loaded from Mongo");
            return true;
#endif
        }

        public override bool SaveToDatabase()
        {
            return base.SaveToDatabase();
#if DEPRECATED
            bool rc = true;

            try
            {
                foreach (var kvp in this.Repository)
                {
                    var document = this.DatabaseAdapter.Save(kvp.Value);
                    if (document == null)
                        rc = false;
                }
            }
            catch (DatabaseNotAliveException)
            {
                rc = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return rc;
#endif
        }
    }
}
