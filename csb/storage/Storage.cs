using Newtonsoft.Json;
using System;
using System.IO;

namespace csb.storage
{
    public class Storage<T> : IStorage<T> {

        #region vars
        T t;
        string path;
        #endregion

        public Storage(string filename, T t) {                        
            string userPath = Path.Combine(Directory.GetCurrentDirectory(), "users");
            if (!Directory.Exists(userPath))
                Directory.CreateDirectory(userPath);    
            this.path = Path.Combine(userPath, filename);
            this.t = t;
        }

        public Storage(string filename, string subdir, T t)
        {
            string userPath = Path.Combine(Directory.GetCurrentDirectory(), subdir);
            if (!Directory.Exists(userPath))
                Directory.CreateDirectory(userPath);
            this.path = Path.Combine(userPath, filename);
            this.t = t;
        }

        #region public
        public T load() {

            if (!File.Exists(path)) {
                save(t);
            }

             string rd = File.ReadAllText(path);

             var p = JsonConvert.DeserializeObject<T>(rd);

             return p;
            

        }

        public void save(T p) {

            var json = JsonConvert.SerializeObject(p, Formatting.Indented);
            try {

                if (File.Exists(path))
                    File.Delete(path);

                File.WriteAllText(path, json);

            }
            catch (Exception ex) {
                throw new Exception("Не удалось сохранить файл JSON");
            }

        }
        #endregion
    }
}
