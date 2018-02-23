using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace PluggableCLI
{
    public class ProviderLoader
    {
        [ImportMany]
        private List<ICLIProvider> _providers;


        public IEnumerable<ICLIProvider> LoadAllProviders()
        {
            var directoryCatalog = new DirectoryCatalog(Directory.GetCurrentDirectory(), "*.*");
            using (var container = new CompositionContainer(directoryCatalog, true))
            {
                container.ComposeParts(this);
            }
            return _providers;

        }

    }
}
