using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace AssGameFramework.Helpers
{
    /// <summary>
    /// The Producted List prevents concurrent modification of a list by way of a double buffer
    /// </summary>
    /// <typeparam name="T">The type stored inside the internal list</typeparam>
    public class ProtectedList<T>
    {
        /// <summary>
        /// The current List, this will always be the most up to date list and is not protected
        /// </summary>
        public List<T> List { get; private set; } = null;

        /// <summary>
        /// The protected list is the double buffered list, updated when protection starts
        /// </summary>
        protected List<T> _protectedList = new List<T>();

        /// <summary>
        /// Stores the validity state of the cache, i.e. Both lists are the same
        /// </summary>
        protected bool _cacheValid = true;
        /// <summary>
        /// Locked marks that the list is under protection
        /// </summary>
        protected bool _locked = false;

        /// <summary>
        /// Returns if the list is currently under protection
        /// </summary>
        public bool IsLocked => _locked;
        
        /// <summary>
        /// Constructor that optionally creates the protected list from an existing list
        /// </summary>
        /// <param name="protectedList">The list to be protected</param>
        public ProtectedList(List<T> protectedList = null)
        {
            if (protectedList == null)
            {
                List = new List<T>();
            }
            else
            {
                List = protectedList;
                _cacheValid = false;
            }
        }

        /// <summary>
        /// Start protecting the list. This returns a snapshot of the current list.
        /// </summary>
        /// <returns>A snapshot of the current list</returns>
        public ref List<T> StartProtection()
        {
            Debug.Assert(!_locked, "List already locked");

            _locked = true;

            if(!_cacheValid)
            {
                _protectedList.Clear();
                _protectedList.AddRange(List);
                _cacheValid = true;
            }
            
            return ref _protectedList;
        }

        /// <summary>
        /// Remove the protection of the list
        /// </summary>
        public void StopProtection()
        {
            _locked = false;
        }

        /// <summary>
        /// Add a new item to the list
        /// </summary>
        /// <param name="item">New item</param>
        public void Add(T item)
        {
            _cacheValid = false;
            List.Add(item);
        }

        /// <summary>
        /// Remove an item from the list
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <returns></returns>
        public bool Remove(T item)
        {
            _cacheValid = false;
            return List.Remove(item);
        }
    }
}
