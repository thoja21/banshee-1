//
// PersistentColumnController.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;

using Hyena.Data.Gui;
using Banshee.Configuration;

namespace Banshee.Collection.Gui
{
    public class PersistentColumnController : ColumnController
    {
        private string root_namespace;
        private bool loaded = false;
        
        public PersistentColumnController (string rootNamespace) : base ()
        {
            if (String.IsNullOrEmpty (rootNamespace)) {
                throw new ArgumentException ("Argument must not be null or empty", "rootNamespace");
            }
            
            root_namespace = rootNamespace;
        }
        
        public void Load ()
        {
            lock (this) {
                foreach (Column column in this) {
                    if (column.Id != null) {
                        column.Visible = ConfigurationClient.Get<bool> (MakeNamespace (column.Id), 
                            "visible", column.Visible);
                    }
                }
                
                // Create a copy so we can read the original index
                List<Column> columns = new List<Column> (Columns);
            
                Columns.Sort (delegate (Column a, Column b) {
                    int a_order = a.Id == null ? -1 : ConfigurationClient.Get<int> (
                        MakeNamespace (a.Id), "order", columns.IndexOf (a));
                    int b_order = b.Id == null ? -1 : ConfigurationClient.Get<int> (
                        MakeNamespace (b.Id), "order", columns.IndexOf (b));
                    
                    return a_order.CompareTo (b_order);
                });
                
                loaded = true;
            }
            
            OnUpdated ();
        }
        
        public void Save ()
        {
            lock (this) {
                for (int i = 0; i < Count; i++) {
                    if (Columns[i].Id != null) {
                        Save (Columns[i], i);
                    }
                }
            }
        }
        
        private void Save (Column column, int index)
        {
            string @namespace = MakeNamespace (column.Id);
            ConfigurationClient.Set<int> (@namespace, "order", index);
            ConfigurationClient.Set<bool> (@namespace, "visible", column.Visible);
        }
        
        protected override void OnUpdated ()
        {
            if (loaded) {
                Save ();
            }
            
            base.OnUpdated ();
        }
        
        private string MakeNamespace (string name)
        {
            return String.Format ("{0}.{1}", root_namespace, name);
        }
        
        public override bool EnableColumnMenu {
            get { return true; }
        }
    }
}
