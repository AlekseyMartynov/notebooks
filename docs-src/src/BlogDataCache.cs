using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Blog {

    public class BlogDataCache {
        Entries _entries;
        Dictionary<string, EntryPath> _paths = new Dictionary<string, EntryPath>();
        Dictionary<string, EntryContent> _contents = new Dictionary<string, EntryContent>();

        public Entries GetEntries() {
            if(_entries == null)
                _entries = new Entries();
            return _entries;
        }

        public EntryPath GetPathObj(string entryPath) {
            if(!_paths.ContainsKey(entryPath))
                _paths[entryPath] = new EntryPath(entryPath);
            return _paths[entryPath];
        }

        public EntryContent GetContent(string entryPath) {
            if(!_contents.ContainsKey(entryPath))
                _contents[entryPath] = new EntryContentParser().Parse(entryPath);
            return _contents[entryPath];
        }

        static BlogDataCache _singleton;

        public static void UseSingleton() {
            _singleton = new BlogDataCache();
        }

        public static void Register(IServiceCollection services) {
            if(_singleton != null)
                services.AddSingleton(_singleton);
            else
                services.AddScoped<BlogDataCache>();
        }
    }

}
