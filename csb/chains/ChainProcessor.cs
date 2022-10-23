﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace csb.chains
{
    public class ChainProcessor
    {
        #region vars
        string path;
        List<Chain> chainList = new();
        #endregion

        #region properties
        public int Count => chainList.Count;
        public List<Chain> Chains => chainList;
        #endregion

        public ChainProcessor(string userId)
        {
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "chains", $"{userId}");
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            this.path = Path.Combine(dirPath, "chains.json");
        }

        public void Load()
        {
            if (!File.Exists(path))
            {
                Save();
            }
            string rd = File.ReadAllText(path);
            chainList = JsonConvert.DeserializeObject<List<Chain>>(rd);
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(chainList, Formatting.Indented);
            try
            {
                if (File.Exists(path))
                    File.Delete(path);

                File.WriteAllText(path, json);

            } catch (Exception ex)
            {
                throw new Exception("Не удалось сохранить файл JSON");
            }
        }

        #region public
        public int Add(string name)
        {

            bool found = chainList.Any(o => o.Name.Equals(name));
            if (found)
                throw new Exception("Цепочка с таким именем уже существует, введите другое имя");

            int id = 0;
            if (chainList.Count == 0)
                id = 1;
            else
            {
                var ids = chainList.Select(x => x.Id).ToList();
                ids = ids.OrderBy(x => x).ToList();
                id = ids[ids.Count - 1];
                id++;
            }

            var chain = new Chain() { Name = name, Id = id, State = ChainState.creating };

            chainList.Add(chain);

            return id;
        }

        public IChain Get(int id)
        {
            var found = chainList.FirstOrDefault(x => x.Id == id);
            if (found == null)
                throw new Exception("Цепочки с таким ID не существует");
            return found;
        }

        public void Start(int id)
        {
            var found = chainList.FirstOrDefault(x => x.Id == id);
            if (found == null)
                throw new Exception("Цепочки с таким ID не существует");

            found.NeedVerifyCodeEvent -= Chain_NeedVerifyCodeEvent;
            found.NeedVerifyCodeEvent += Chain_NeedVerifyCodeEvent;
            found.UserStartedEvent -= Chain_UserStartedEvent;
            found.UserStartedEvent += Chain_UserStartedEvent;

            try
            {
                found.Start();
            } catch (Exception ex)
            {
                throw new Exception("Не удалось запустить цепочку");
            }
        }

        public void Delete(int id)
        {
            var found = chainList.FirstOrDefault(x => x.Id == id);
            if (found == null)
                throw new Exception("Цепочки с таким ID не существует");
            found.Stop();
            chainList.Remove(found);
            Save();
        }

        public void Stop(int id)
        {
            var found = chainList.FirstOrDefault(x => x.Id == id);
            if (found == null)
                throw new Exception("Цепочки с таким ID не существует");
            found.NeedVerifyCodeEvent -= Chain_NeedVerifyCodeEvent;
            found.UserStartedEvent -= Chain_UserStartedEvent;
            found.Stop();
        }

        public void StartAll()
        {
            if (chainList == null)
                return;

            foreach (var item in chainList)
            {
                if (item.IsRunning)
                    return;

                item.NeedVerifyCodeEvent += Chain_NeedVerifyCodeEvent;
                item.UserStartedEvent += Chain_UserStartedEvent;
                //item.Start();
            }
        }
        #endregion

        #region events
        public event Action<int, string> NeedVerifyCodeEvent;
        public event Action<IChain> ChainStartedEvent;
        #endregion

        private void Chain_NeedVerifyCodeEvent(int id, string phone)
        {
            NeedVerifyCodeEvent?.Invoke(id, phone);
        }

        private void Chain_UserStartedEvent(IChain chain)
        {
            ChainStartedEvent?.Invoke(chain);            
        }
    }
}
