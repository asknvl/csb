using csb.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csb.addme_service
{
    public class AddMeService
    {
        #region vars
        List<long> approvedIDs = new();
        IStorage<List<long>> storage;
        bool isLoaded = false;
        #endregion

        #region singletone
        private static AddMeService instance;
        private AddMeService()
        {            
            storage = new Storage<List<long>>("approvedids.json", approvedIDs);
            try
            {
                approvedIDs = storage.load();
                isLoaded = true;
            } catch (Exception ex)
            {
            }
        }
        public static AddMeService getInstance()
        {
            if (instance == null)
                instance = new AddMeService();
            return instance;
        }
        #endregion

        #region public 
        public void Add(long id)
        {
            if (!isLoaded)
                throw new Exception("Не удалось добавить пользователя");

            if (!approvedIDs.Contains(id))
            {
                approvedIDs.Add(id);
                storage.save(approvedIDs);
            }
        }
        public bool IsApproved(long id)
        {   
            if (!isLoaded)
                throw new Exception("Не удалось проверить правда доступа пользователя");
            return approvedIDs.Contains(id);
        }
        #endregion
    }
}
