using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nougat.Projects;
using Nougat.Updates;

namespace Nougat.Display
{
    /// <summary>
    /// Manages how different models are rendered.
    /// </summary>
    public class DisplayManager : IDisplayManager
    {
        private readonly TextWriter _textWriter;

        private DisplayManager(TextWriter writer)
        {
            _textWriter = writer;
        }

        public static IDisplayManager Create(TextWriter writer)
            => new DisplayManager(writer);

        public void List(string caption, Package[] packages)
        {
            if (caption == null) throw new ArgumentNullException(nameof(caption));
            if (packages == null) throw new ArgumentNullException(nameof(packages));
           
            var dataRows = packages.Select(p => {
                var data  = new DataRow();
                data.Values["Package"] = p.Name;
                data.Values["Version"] = p.Version;
                data.Values["Max Version"] = p.MaxVersion ?? "";
                data.Values["Latest Version"] = p.LatestVersion ?? "NOT FOUND";
                data.Values["Has Conflict"] = p.HasConflict ? "YES" : "NO";
                data.Values["Update Available"] = p.HasUpdate ? "YES" : "NO";

                return data;
            }).ToArray();

            var dataSet = new DataSet(dataRows);

            DataSetViewer.Display(caption, dataSet, _textWriter);
        }

        public void List(string caption, ProjectUpdate update, bool displayDetails)
        {
            if (caption == null) throw new ArgumentNullException(nameof(caption));
            if (update == null) throw new ArgumentNullException(nameof(update));

            var dataRows = update.Packages.Select(p => {
                                
                var data = new DataRow(() => 
                {
                    if (! displayDetails) return new string[] {};
                      
                    return update.ProjectFiles.Select(f => new FileInfo(f).Name); 
                });

                    
                data.Values["Package"] = p.Name;
                data.Values["Current Version"] = p.CurrentVersion;
                data.Values["Updated Version"] = p.UpdatedVersion ?? "NOT FOUND";

                return data;
            }).ToArray();

            var dataSet = new DataSet(dataRows);

            DataSetViewer.Display(caption, dataSet, _textWriter);
        }
    }
}